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
        public AzureSphereDeploymentModel CurrentDeployment { get; private set; }

        public string OsFeedTypeStr
        {
            get
            {
                switch (OsFeedType)
                {
                    case 0:
                        return "Retail";
                    case 1:
                        return "RetailEval";
                    default:
                        return "?";
                }
            }
        }

        public string UpdatePolicyStr
        {
            get
            {
                switch (UpdatePolicy)
                {
                    case 0:
                        return "No3rdParty";
                    case 1:
                        return "AppUpdates";
                    case 2:
                        return "NoUpdates";
                    case 3:
                        return "UpdateAll";
                    default:
                        return "?";
                }
            }
        }

        internal AzureSphereDeviceGroup(AzureSphereTenant tenant, JToken json)
        {
            Tenant = tenant;

            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
            Description = json.Value<string>("Description");
            OsFeedType = json.Value<int>("OsFeedType");
            ProductId = json.Value<string>("ProductId");
            UpdatePolicy = json.Value<int>("UpdatePolicy");
            CurrentDeployment = json["CurrentDeployment"].HasValues ? new AzureSphereDeploymentModel(json["CurrentDeployment"]) : null;
        }

    }
}
