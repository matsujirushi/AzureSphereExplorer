using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereDeviceGroup
    {
        private AzureSphereTenant Tenant;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int OsFeedType { get; private set; }
        public string ProductId { get; private set; }
        public int UpdatePolicy { get; private set; }

        internal AzureSphereDeviceGroup(AzureSphereTenant tenant, JToken json)
        {
            Tenant = tenant;

            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
            Description = json.Value<string>("Description");
            OsFeedType = json.Value<int>("OsFeedType");
            ProductId = json.Value<string>("ProductId");
            UpdatePolicy = json.Value<int>("UpdatePolicy");
        }

        public async Task<List<AzureSphereDeployment>> GetDeploymentsAsync(CancellationToken cancellationToken)
        {
            var jsonString = await Tenant.GetAsync($"devicegroups/{Id}/deployments", cancellationToken);
            Console.WriteLine("GetDeploymentsAsync()");
            Console.WriteLine(jsonString);
            var json = JToken.Parse(jsonString);
            var jsonDeployments = json.Value<JArray>("Items");

            var deployments = new List<AzureSphereDeployment>();
            foreach (var jsonDeployment in jsonDeployments)
            {
                deployments.Add(new AzureSphereDeployment(Tenant, jsonDeployment));
            }

            return deployments;
        }

    }
}
