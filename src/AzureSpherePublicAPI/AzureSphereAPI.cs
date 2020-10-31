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
                tenants.Add(new AzureSphereTenant(jsonTenant));
            }

            return tenants;
        }

        public async Task<List<AzureSphereProduct>> GetProductsAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/products", cancellationToken);
            Console.WriteLine("GetProductsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonProducts = json.Value<JArray>("Items");

            var products = new List<AzureSphereProduct>();
            foreach (var jsonProduct in jsonProducts)
            {
                products.Add(new AzureSphereProduct(jsonProduct));
            }

            return products;
        }

        public async Task DeleteProductAsync(AzureSphereTenant tenant, AzureSphereProduct product, CancellationToken cancellationToken)
        {
            await DeleteAsync($"v2/tenants/{tenant.Id}/products/{product.Id}", cancellationToken);
        }

        public async Task<List<AzureSphereDeviceGroup>> GetDeviceGroupsAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/devicegroups", cancellationToken);
            Console.WriteLine("GetDeviceGroupsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonDeviceGroups = json.Value<JArray>("Items");

            var deviceGroups = new List<AzureSphereDeviceGroup>();
            foreach (var jsonDeviceGroup in jsonDeviceGroups)
            {
                deviceGroups.Add(new AzureSphereDeviceGroup(jsonDeviceGroup));
            }

            return deviceGroups;
        }

        public async Task DeleteDeviceGroupAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup deviceGroup, CancellationToken cancellationToken)
        {
            await DeleteAsync($"v2/tenants/{tenant.Id}/devicegroups/{deviceGroup.Id}", cancellationToken);
        }

        public async Task<List<AzureSphereDevice>> GetDevicesAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/devices", cancellationToken);
            Console.WriteLine("GetDevicesAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonDevices = json.Value<JArray>("Items");

            var devices = new List<AzureSphereDevice>();
            foreach (var jsonDevice in jsonDevices)
            {
                devices.Add(new AzureSphereDevice(jsonDevice));
            }

            return devices;
        }

        public async Task<List<AzureSphereDeployment>> GetDeploymentsAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup deviceGroup, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/devicegroups/{deviceGroup.Id}/deployments", cancellationToken);
            Console.WriteLine("GetDeploymentsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonDeployments = json.Value<JArray>("Items");

            var deployments = new List<AzureSphereDeployment>();
            foreach (var jsonDeployment in jsonDeployments)
            {
                deployments.Add(new AzureSphereDeployment(jsonDeployment));
            }

            return deployments;
        }

        public async Task<AzureSphereImage> GetImageAsync(AzureSphereTenant tenant, string imageId, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/images/{imageId}/metadata", cancellationToken);
            Console.WriteLine("GetImageAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);

            return new AzureSphereImage(json);
        }

        public async Task<List<AzureSphereDeviceInsight>> GetDeviceInsightsAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/getDeviceInsights", cancellationToken);
            Console.WriteLine("GetDeviceInsightsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonDeviceInsights = json.Value<JArray>("Items");

            var deviceInsights = new List<AzureSphereDeviceInsight>();
            foreach (var jsonDeviceInsight in jsonDeviceInsights)
            {
                deviceInsights.Add(new AzureSphereDeviceInsight(jsonDeviceInsight));
            }

            return deviceInsights;
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

        internal async Task DeleteAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(AccessToken)) throw new ApplicationException();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var uri = new Uri(AzureSphereApiEndpoint, relativeUrl);
                using (var response = await client.DeleteAsync(new Uri(AzureSphereApiEndpoint, relativeUrl), cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task<List<AzureSphereUser>> GetUsersAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync($"v2/tenants/{tenant.Id}/users", cancellationToken);
            Console.WriteLine("GetUsersAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonUsers = json.Value<JArray>("Principals");

            var users = new List<AzureSphereUser>();
            foreach (var jsonUser in jsonUsers)
            {
                users.Add(new AzureSphereUser(jsonUser));
            }

            return users;
        }

    }
}
