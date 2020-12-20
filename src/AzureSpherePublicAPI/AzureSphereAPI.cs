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
        public string Username { get; private set; } = string.Empty;

        public enum Method
        {
            GET,
            PUT,
            POST,
            DELETE
        };

        public enum StateNum
        {
            NotStarted,
            InProgress,
            Complete,
            Failed
        };


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
            Username = authResult.Account.Username;
        }

        public async Task<List<AzureSphereTenant>> GetTenantsAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("GetTenantsAsync()");
            var tenants = new List<AzureSphereTenant>();

            var jsonString = await MethodAsync("v2/tenants", Method.GET, null, null, cancellationToken);
            Console.WriteLine(jsonString);
            var jsonTenants = JArray.Parse(jsonString);

            foreach (var jsonTenant in jsonTenants)
            {
                tenants.Add(new AzureSphereTenant(jsonTenant));
            }

            return tenants;
        }

        public async Task<List<string>> GetRolesAsync(AzureSphereTenant tenant, string username, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetRolesAsync()");
            var roleNames = new List<string>();

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/users/{username}/role", Method.GET, null, null, cancellationToken);
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonRoles = json.Value<JArray>("Roles");
            if (jsonRoles.Count != 1) throw new ApplicationException();
            if (jsonRoles[0].Value<string>("TenantId") != tenant.Id) throw new ApplicationException();
            var jsonRoleNames = jsonRoles[0]["RoleNames"];

            foreach (var jsonRoleName in jsonRoleNames)
            {
                roleNames.Add(jsonRoleName.Value<string>());
            }

            return roleNames;
        }

        public async Task<List<AzureSphereProduct>> GetProductsAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetProductsAsync()");
            var products = new List<AzureSphereProduct>();

            string continuationToken = null;
            while (true)
            {
                var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/products", Method.GET, null, null, cancellationToken);
                Console.WriteLine(jsonString);
                var json = JToken.Parse(jsonString);
                var jsonProducts = json.Value<JArray>("Items");

                foreach (var jsonProduct in jsonProducts)
                {
                    products.Add(new AzureSphereProduct(jsonProduct));
                }

                if (json["ContinuationToken"] == null || json["ContinuationToken"].Type == JTokenType.Null) break;
                continuationToken = json.Value<string>("ContinuationToken");
            }

            return products;
        }

        public async Task<bool> PostCreateProductGroupAsync(AzureSphereTenant tenant, HttpContent jsonContent,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("PostCreateProductAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/products",
                Method.POST,
                jsonContent,
                null,
                cancellationToken);
            Console.WriteLine(jsonString);
            var operation = new AzureSphereOperation(JObject.Parse(jsonString));

            return await GetAsyncOperation(tenant, operation.OperationId, cancellationToken);
        }

        public async Task<bool> DeleteProductAsync(AzureSphereTenant tenant, AzureSphereProduct product, CancellationToken cancellationToken)
        {
            Console.WriteLine("DeleteProductAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/products/{product.Id}", Method.DELETE, null, null, cancellationToken);
            Console.WriteLine(jsonString);
            var operation = new AzureSphereOperation(JObject.Parse(jsonString));

            return await GetAsyncOperation(tenant, operation.OperationId, cancellationToken);
        }

        public async Task<List<AzureSphereDeviceGroup>> GetDeviceGroupsAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetDeviceGroupsAsync()");
            var deviceGroups = new List<AzureSphereDeviceGroup>();

            string continuationToken = null;
            while (true)
            {
                var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devicegroups", Method.GET, null, continuationToken, cancellationToken);
                Console.WriteLine(jsonString);
                var json = JToken.Parse(jsonString);
                var jsonDeviceGroups = json.Value<JArray>("Items");

                foreach (var jsonDeviceGroup in jsonDeviceGroups)
                {
                    deviceGroups.Add(new AzureSphereDeviceGroup(jsonDeviceGroup));
                }

                if (json["ContinuationToken"] == null || json["ContinuationToken"].Type == JTokenType.Null) break;
                continuationToken = json.Value<string>("ContinuationToken");
            }

            return deviceGroups;
        }
        public async Task<bool> PostCreateDeviceGroupAsync(AzureSphereTenant tenant, HttpContent jsonContent,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("PostCreateDeviceGroupAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devicegroups",
                Method.POST,
                jsonContent,
                null,
                cancellationToken);
            Console.WriteLine(jsonString);

            return true;
        }

        public async Task<bool> DeleteDeviceGroupAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup deviceGroup, CancellationToken cancellationToken)
        {
            Console.WriteLine("DeleteDeviceGroupAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devicegroups/{deviceGroup.Id}", Method.DELETE, null, null, cancellationToken);
            Console.WriteLine(jsonString);
            var operation = new AzureSphereOperation(JObject.Parse(jsonString));

            return await GetAsyncOperation(tenant, operation.OperationId, cancellationToken);

        }

        public async Task<List<AzureSphereDevice>> GetDevicesAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetDevicesAsync()");
            var devices = new List<AzureSphereDevice>();

            string continuationToken = null;
            while (true)
            {
                var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devices", Method.GET, null, null, cancellationToken);
                Console.WriteLine(jsonString);
                var json = JToken.Parse(jsonString);
                var jsonDevices = json.Value<JArray>("Items");

                foreach (var jsonDevice in jsonDevices)
                {
                    devices.Add(new AzureSphereDevice(jsonDevice));
                }

                if (json["ContinuationToken"] == null || json["ContinuationToken"].Type == JTokenType.Null) break;
                continuationToken = json.Value<string>("ContinuationToken");
            }

            return devices;
        }

        public async Task<bool> PutChangeDeviceGroupAsync(AzureSphereTenant tenant, string deviceId, HttpContent jsonContent,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("PutChangeDeviceGroupAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devices/{deviceId}/devicegroup",
                Method.PUT,
                jsonContent,
                null,
                cancellationToken);
            Console.WriteLine(jsonString);
            var operation = new AzureSphereOperation(JObject.Parse(jsonString));

            return await GetAsyncOperation(tenant, operation.OperationId, cancellationToken);
        }

        public async Task<List<AzureSphereDeployment>> GetDeploymentsAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup deviceGroup, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetDeploymentsAsync()");
            var deployments = new List<AzureSphereDeployment>();

            string continuationToken = null;
            while (true)
            {
                var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devicegroups/{deviceGroup.Id}/deployments",
                                    Method.GET, null, null, cancellationToken);
                Console.WriteLine(jsonString);
                var json = JToken.Parse(jsonString);
                var jsonDeployments = json.Value<JArray>("Items");

                foreach (var jsonDeployment in jsonDeployments)
                {
                    deployments.Add(new AzureSphereDeployment(jsonDeployment));
                }

                if (json["ContinuationToken"] == null || json["ContinuationToken"].Type == JTokenType.Null) break;
                continuationToken = json.Value<string>("ContinuationToken");
            }

            return deployments;
        }

        public async Task<AzureSphereImage> GetImageAsync(AzureSphereTenant tenant, string imageId, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetImageAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/images/{imageId}/metadata", Method.GET, null, null, cancellationToken);
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);

            return new AzureSphereImage(json);
        }

        public async Task<List<AzureSphereDeviceInsight>> GetDeviceInsightsAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetDeviceInsightsAsync()");
            var deviceInsights = new List<AzureSphereDeviceInsight>();

            string continuationToken = null;
            while (true)
            {
                var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/getDeviceInsights", Method.GET, null, null, cancellationToken);
                Console.WriteLine(jsonString);
                var json = JToken.Parse(jsonString);
                var jsonDeviceInsights = json.Value<JArray>("Items");

                foreach (var jsonDeviceInsight in jsonDeviceInsights)
                {
                    deviceInsights.Add(new AzureSphereDeviceInsight(jsonDeviceInsight));
                }

                if (json["ContinuationToken"] == null || json["ContinuationToken"].Type == JTokenType.Null) break;
                continuationToken = json.Value<string>("ContinuationToken");
            }

            return deviceInsights;
        }

        public async Task<bool> PostImageUploadAsync(AzureSphereTenant tenant, HttpContent jsonContent,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("PostImageUploadAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/images",
                Method.POST,
                jsonContent,
                null,
                cancellationToken);
            Console.WriteLine(jsonString);
            var operation = new AzureSphereOperation(JObject.Parse(jsonString));

            return await GetAsyncOperation(tenant, operation.OperationId, cancellationToken);
        }

        public async Task<bool> PostDeployAsync(AzureSphereTenant tenant, AzureSphereDeviceGroup deviceGroup, HttpContent jsonContent,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("PostDeployAsync()");

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/devicegroups/{deviceGroup.Id}/deployments",
                Method.POST,
                jsonContent,
                null,
                cancellationToken);
            Console.WriteLine(jsonString);
            var operation = new AzureSphereOperation(JObject.Parse(jsonString));

            return await GetAsyncOperation(tenant, operation.OperationId, cancellationToken);
        }

        private async Task<bool> GetAsyncOperation(AzureSphereTenant tenant, string operationId, CancellationToken cancellationToken)
        {
            bool continueOperationAsync = true;
            bool ret = false;
            while (continueOperationAsync)
            {
                var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/operations/{operationId}",
                    Method.GET,
                    null,
                    null,
                    cancellationToken);

                Console.WriteLine("GetAsyncOperation()");
                Console.WriteLine(jsonString);
                var operation = new AzureSphereOperation(JObject.Parse(jsonString));
                StateNum state = (StateNum)operation.State;

                switch (state)
                {
                    case StateNum.Complete:
                        ret = true;
                        continueOperationAsync = false;
                        break;
                    case StateNum.Failed:
                        ret = false;
                        continueOperationAsync = false;
                        break;
                    default:
                        break;
                }
            }
            return ret;
        }

        public async Task<List<AzureSphereUser>> GetUsersAsync(AzureSphereTenant tenant, CancellationToken cancellationToken)
        {
            Console.WriteLine("GetUsersAsync()");
            var users = new List<AzureSphereUser>();

            var jsonString = await MethodAsync($"v2/tenants/{tenant.Id}/users", Method.GET, null, null, cancellationToken);
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonUsers = json.Value<JArray>("Principals");

            foreach (var jsonUser in jsonUsers)
            {
                users.Add(new AzureSphereUser(jsonUser));
            }

            return users;
        }

        private async Task<string> MethodAsync(string relativeUrl, Method method, HttpContent content, string continuationToken, CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                if (continuationToken != null) client.DefaultRequestHeaders.Add("Sphere-Continuation", continuationToken);

                Uri uri = new Uri(AzureSphereApiEndpoint, relativeUrl);

                Task<HttpResponseMessage> responseTask;

                switch (method)
                {
                    case Method.GET:
                    default:
                        responseTask = client.GetAsync(uri, cancellationToken);
                        break;
                    case Method.PUT:
                        responseTask = client.PutAsync(uri, content, cancellationToken);
                        break;
                    case Method.POST:
                        responseTask = client.PostAsync(uri, content, cancellationToken);
                        break;
                    case Method.DELETE:
                        responseTask = client.DeleteAsync(uri, cancellationToken);
                        break;
                }

                using (HttpResponseMessage response = await responseTask)
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
