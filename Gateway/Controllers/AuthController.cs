using Microsoft.AspNetCore.Mvc;
using SystemInstaller.Gateway.Models;
using SystemInstaller.Gateway.Services;

namespace SystemInstaller.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IKeycloakService keycloakService, ILogger<AuthController> logger)
    {
        _keycloakService = keycloakService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);
            
            var authResult = await _keycloakService.AuthenticateAsync(request.Username, request.Password);
            
            if (!authResult.IsAuthenticated)
            {
                _logger.LogWarning("Authentication failed for user: {Username}", request.Username);
                return Ok(new LoginResponse(false, "Invalid credentials"));
            }

            // Set HTTP-only secure cookies
            SetAuthCookies(authResult.AccessToken!, authResult.RefreshToken);
            
            _logger.LogInformation("User {Username} authenticated successfully", request.Username);
            return Ok(new LoginResponse(true, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user: {Username}", request.Username);
            return Ok(new LoginResponse(false, "Authentication service unavailable"));
        }
    }

    [HttpPost("logout")]
    public ActionResult<LoginResponse> Logout()
    {
        try
        {
            // Clear auth cookies
            ClearAuthCookies();
            
            _logger.LogInformation("User logged out successfully");
            return Ok(new LoginResponse(true, "Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Ok(new LoginResponse(false, "Logout failed"));
        }
    }

    [HttpGet("user")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var accessToken = Request.Cookies["access_token"];
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized();
            }

            var userInfo = await _keycloakService.GetUserInfoAsync(accessToken);
            
            if (userInfo == null)
            {
                return Unauthorized();
            }

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return Unauthorized();
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refresh_token"];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Ok(new LoginResponse(false, "No refresh token available"));
            }

            var authResult = await _keycloakService.RefreshTokenAsync(refreshToken);
            
            if (!authResult.IsAuthenticated)
            {
                ClearAuthCookies();
                return Ok(new LoginResponse(false, "Token refresh failed"));
            }

            // Update cookies with new tokens
            SetAuthCookies(authResult.AccessToken!, authResult.RefreshToken);
            
            return Ok(new LoginResponse(true, "Token refreshed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            ClearAuthCookies();
            return Ok(new LoginResponse(false, "Token refresh failed"));
        }
    }

    private void SetAuthCookies(string accessToken, string? refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only over HTTPS in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1) // Access token expiry
        };

        Response.Cookies.Append("access_token", accessToken, cookieOptions);

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7) // Refresh token expiry
            };
            
            Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
        }
    }

    private void ClearAuthCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire immediately
        };

        Response.Cookies.Append("access_token", "", cookieOptions);
        Response.Cookies.Append("refresh_token", "", cookieOptions);
    }
}
