using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereTenant
    {
        private AzureSphereAPI Api;
        internal List<AzureSphereProductModel> Products { get; private set; } = null;
        internal List<AzureSphereDeviceGroup> DeviceGroups { get; private set; } = null;

        public string Name { get; private set; }
        public string Id { get; private set; }

        internal AzureSphereTenant(AzureSphereAPI api, JToken json)
        {
            Api = api;

            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
        }

        internal async Task<string> GetAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            return await Api.GetAsync($"v2/tenants/{Id}/{relativeUrl}", cancellationToken);
        }

        public async Task<List<AzureSphereDevice>> GetDevicesAsync(CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync("devices", cancellationToken);
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

        public async Task<List<AzureSphereProductModel>> GetProductsAsync(CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync("products", cancellationToken);
            Console.WriteLine("GetProductsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonProducts = json.Value<JArray>("Items");

            var products = new List<AzureSphereProductModel>();
            foreach (var jsonProduct in jsonProducts)
            {
                products.Add(new AzureSphereProductModel(jsonProduct));
            }

            Products = products;

            return products;
        }

        public async Task<List<AzureSphereDeviceGroup>> GetDeviceGroupsAsync(CancellationToken cancellationToken)
        {
            var jsonString = await GetAsync("devicegroups", cancellationToken);
            Console.WriteLine("GetDeviceGroupsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonDeviceGroups = json.Value<JArray>("Items");

            var deviceGroups = new List<AzureSphereDeviceGroup>();
            foreach (var jsonDeviceGroup in jsonDeviceGroups)
            {
                deviceGroups.Add(new AzureSphereDeviceGroup(jsonDeviceGroup));
            }

            DeviceGroups = deviceGroups;

            return deviceGroups;
        }

    }
}
