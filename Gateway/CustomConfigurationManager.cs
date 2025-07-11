using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace SystemInstaller.Gateway;

public class CustomConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
{
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _innerManager;
    private readonly string _externalUrl;
    private readonly string _internalUrl;
    private readonly string _realm;

    public CustomConfigurationManager(
        IConfigurationManager<OpenIdConnectConfiguration> innerManager,
        string externalUrl,
        string internalUrl,
        string realm)
    {
        _innerManager = innerManager;
        _externalUrl = externalUrl;
        _internalUrl = internalUrl;
        _realm = realm;
    }

    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
    {
        // Get the original configuration from Keycloak
        var config = await _innerManager.GetConfigurationAsync(cancel);
        
        // Create a new configuration with modified URLs
        var customConfig = new OpenIdConnectConfiguration
        {
            Issuer = $"{_externalUrl}/realms/{_realm}",
            AuthorizationEndpoint = $"{_externalUrl}/realms/{_realm}/protocol/openid-connect/auth",
            TokenEndpoint = $"{_internalUrl}/realms/{_realm}/protocol/openid-connect/token",
            UserInfoEndpoint = $"{_internalUrl}/realms/{_realm}/protocol/openid-connect/userinfo",
            EndSessionEndpoint = $"{_externalUrl}/realms/{_realm}/protocol/openid-connect/logout",
            JwksUri = config.JwksUri, // Keep the original JwksUri from the metadata
        };

        // Copy signing keys from the original configuration
        foreach (var key in config.SigningKeys)
        {
            customConfig.SigningKeys.Add(key);
        }

        // Copy additional properties if needed
        customConfig.ResponseTypesSupported.Clear();
        foreach (var responseType in config.ResponseTypesSupported)
        {
            customConfig.ResponseTypesSupported.Add(responseType);
        }

        customConfig.ScopesSupported.Clear();
        foreach (var scope in config.ScopesSupported)
        {
            customConfig.ScopesSupported.Add(scope);
        }

        customConfig.ResponseModesSupported.Clear();
        foreach (var responseMode in config.ResponseModesSupported)
        {
            customConfig.ResponseModesSupported.Add(responseMode);
        }

        customConfig.GrantTypesSupported.Clear();
        foreach (var grantType in config.GrantTypesSupported)
        {
            customConfig.GrantTypesSupported.Add(grantType);
        }

        customConfig.SubjectTypesSupported.Clear();
        foreach (var subjectType in config.SubjectTypesSupported)
        {
            customConfig.SubjectTypesSupported.Add(subjectType);
        }

        customConfig.IdTokenSigningAlgValuesSupported.Clear();
        foreach (var alg in config.IdTokenSigningAlgValuesSupported)
        {
            customConfig.IdTokenSigningAlgValuesSupported.Add(alg);
        }

        Console.WriteLine($"DEBUG: CustomConfigurationManager - AuthorizationEndpoint = {customConfig.AuthorizationEndpoint}");
        Console.WriteLine($"DEBUG: CustomConfigurationManager - TokenEndpoint = {customConfig.TokenEndpoint}");
        Console.WriteLine($"DEBUG: CustomConfigurationManager - JwksUri = {customConfig.JwksUri}");
        Console.WriteLine($"DEBUG: CustomConfigurationManager - SigningKeys count = {customConfig.SigningKeys.Count}");

        return customConfig;
    }

    public void RequestRefresh()
    {
        _innerManager.RequestRefresh();
    }
}
