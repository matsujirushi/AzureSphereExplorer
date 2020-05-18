using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereAPI
    {
        private const string AuthAzureSphereClientApplicationId = "0B1C8F7E-28D2-4378-97E2-7D7D63F7C87F";
        private readonly List<string> AuthScopes = new List<string>() { "https://sphere.azure.net/api/user_impersonation" };
        private const string AuthAzureSphereTenantId = "7d71c83c-ccdf-45b7-b3c9-9c41b94406d9";
        private static readonly Uri AzureSphereApiEndpoint = new Uri("https://prod.core.sphere.azure.net/");

        private string AccessToken = string.Empty;

        public async Task AuthenticationAsync(CancellationToken cancellationToken)
        {
            var publicClientApp = PublicClientApplicationBuilder
                .Create(AuthAzureSphereClientApplicationId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AuthAzureSphereTenantId)
                .WithRedirectUri("http://localhost")
                .Build();

            var authResult = await publicClientApp
                .AcquireTokenInteractive(AuthScopes)
                .ExecuteAsync();

            AccessToken = authResult.AccessToken;
        }

        public async Task<List<AzureSphereTenant>> GetTenantsAsync(CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync("v2/tenants", cancellationToken);
            Console.WriteLine("GetTenantsAsync()");
            Console.WriteLine(jsonString);
            var jsonTenants = JArray.Parse(jsonString);

            var tenants = new List<AzureSphereTenant>();
            foreach (var jsonTenant in jsonTenants)
            {
                tenants.Add(new AzureSphereTenant(this, jsonTenant));
            }

            return tenants;
        }

        internal async Task<string> GetAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(AccessToken)) throw new ApplicationException();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var uri = new Uri(AzureSphereApiEndpoint, relativeUrl);
                using (var response = await client.GetAsync(new Uri(AzureSphereApiEndpoint, relativeUrl), cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

    }
}
