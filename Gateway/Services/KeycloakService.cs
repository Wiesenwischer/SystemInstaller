using System.Text.Json;
using System.Text;
using SystemInstaller.Gateway.Models;
using System.IdentityModel.Tokens.Jwt;

namespace SystemInstaller.Gateway.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;
    
    private readonly string _keycloakUrl;
    private readonly string _realm;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public KeycloakService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _keycloakUrl = _configuration["Keycloak:Url"] ?? "http://keycloak:8080";
        _realm = _configuration["Keycloak:Realm"] ?? "systeminstaller";
        _clientId = _configuration["Keycloak:ClientId"] ?? "systeminstaller-client";
        _clientSecret = _configuration["Keycloak:ClientSecret"] ?? "";
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        try
        {
            var tokenUrl = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";
            
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("client_id", _clientId),
                new("username", username),
                new("password", password),
                new("scope", "openid profile email")
            };

            if (!string.IsNullOrEmpty(_clientSecret))
            {
                parameters.Add(new("client_secret", _clientSecret));
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Keycloak authentication failed for user {Username}. Status: {StatusCode}", 
                    username, response.StatusCode);
                return new AuthResult(false);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(responseContent, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogWarning("Invalid token response from Keycloak for user {Username}", username);
                return new AuthResult(false);
            }

            var userInfo = await GetUserInfoFromTokenAsync(tokenResponse.AccessToken);
            
            return new AuthResult(
                IsAuthenticated: true,
                User: userInfo,
                AccessToken: tokenResponse.AccessToken,
                RefreshToken: tokenResponse.RefreshToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user {Username}", username);
            return new AuthResult(false);
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenUrl = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";
            
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "refresh_token"),
                new("client_id", _clientId),
                new("refresh_token", refreshToken)
            };

            if (!string.IsNullOrEmpty(_clientSecret))
            {
                parameters.Add(new("client_secret", _clientSecret));
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token refresh failed. Status: {StatusCode}", response.StatusCode);
                return new AuthResult(false);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(responseContent,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogWarning("Invalid token response from Keycloak during refresh");
                return new AuthResult(false);
            }

            var userInfo = await GetUserInfoFromTokenAsync(tokenResponse.AccessToken);
            
            return new AuthResult(
                IsAuthenticated: true,
                User: userInfo,
                AccessToken: tokenResponse.AccessToken,
                RefreshToken: tokenResponse.RefreshToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthResult(false);
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string accessToken)
    {
        try
        {
            return await GetUserInfoFromTokenAsync(accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var userInfoUrl = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/userinfo";
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync(userInfoUrl);
            
            // Clear the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    private Task<UserInfo?> GetUserInfoFromTokenAsync(string accessToken)
    {
        try
        {
            // Parse JWT token to get user info
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(accessToken);

            var username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "";
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
            var firstName = jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? "";
            var lastName = jwt.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value ?? "";
            
            // Get roles from realm_access
            var realmAccessClaim = jwt.Claims.FirstOrDefault(c => c.Type == "realm_access");
            var roles = new List<string>();
            
            if (realmAccessClaim != null)
            {
                try
                {
                    var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value);
                    roles = realmAccess?.Roles ?? new List<string>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing realm access from token");
                }
            }

            return Task.FromResult<UserInfo?>(new UserInfo(username, email, firstName, lastName, roles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing user info from token");
            return Task.FromResult<UserInfo?>(null);
        }
    }

    private record KeycloakTokenResponse(
        string? AccessToken,
        string? RefreshToken,
        string? TokenType,
        int? ExpiresIn
    );

    private record RealmAccess(List<string>? Roles);
}
