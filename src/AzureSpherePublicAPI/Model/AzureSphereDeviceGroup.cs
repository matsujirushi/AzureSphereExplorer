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
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int OsFeedType { get; private set; }
        public string ProductId { get; private set; }
        public int UpdatePolicy { get; private set; }
        public AzureSphereDeployment CurrentDeployment { get; private set; }

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
                        return "UpdateAll";
                    case 1:
                        return "No3rdPartyAppUpdates";
                    case 2:
                        return "NoUpdates";
                    default:
                        return "?";
                }
            }
        }

        internal AzureSphereDeviceGroup(JToken json)
        {
            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
            Description = json.Value<string>("Description");
            OsFeedType = json.Value<int>("OsFeedType");
            ProductId = json.Value<string>("ProductId");
            UpdatePolicy = json.Value<int>("UpdatePolicy");
            CurrentDeployment = json["CurrentDeployment"].HasValues ? new AzureSphereDeployment(json["CurrentDeployment"]) : null;
        }

    }
}
