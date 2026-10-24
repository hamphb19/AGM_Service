using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AGM_API.Services
{
    public enum KeycloakDeleteResult { Deleted, NotConfigured, Failed }

    /// <summary>
    /// Zugriff auf die Keycloak Admin-API über einen Service-Account-Client
    /// (Keycloak:AdminClientId / Keycloak:AdminClientSecret, Rolle realm-management → manage-users).
    /// </summary>
    public class KeycloakAdminService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<KeycloakAdminService> logger)
    {
        public async Task<KeycloakDeleteResult> DeleteUserAsync(string subject)
        {
            var authority = config["Keycloak:Authority"]?.TrimEnd('/');
            var clientId = config["Keycloak:AdminClientId"];
            var secret = config["Keycloak:AdminClientSecret"];
            if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(secret))
            {
                logger.LogWarning("Keycloak admin client not configured — user {Subject} was not deleted in Keycloak.", subject);
                return KeycloakDeleteResult.NotConfigured;
            }

            try
            {
                var http = httpFactory.CreateClient();

                var tokenResponse = await http.PostAsync($"{authority}/protocol/openid-connect/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["client_id"] = clientId,
                        ["client_secret"] = secret,
                    }));
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Keycloak admin token request failed: {Status}", tokenResponse.StatusCode);
                    return KeycloakDeleteResult.Failed;
                }

                using var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
                var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();

                // https://host/realms/agm -> https://host/admin/realms/agm
                var adminBase = authority.Replace("/realms/", "/admin/realms/");
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{adminBase}/users/{Uri.EscapeDataString(subject)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await http.SendAsync(request);

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                    return KeycloakDeleteResult.Deleted;

                logger.LogError("Keycloak user deletion failed for {Subject}: {Status}", subject, response.StatusCode);
                return KeycloakDeleteResult.Failed;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Keycloak user deletion failed for {Subject}", subject);
                return KeycloakDeleteResult.Failed;
            }
        }
    }
}
