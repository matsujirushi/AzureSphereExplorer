using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereDeviceModel
    {
        private AzureSphereTenant Tenant;

        public string Id { get; private set; }
        public string DeviceGroupId { get; private set; }
        public int ChipSku { get; private set; }

        public string ChipSkuStr
        {
            get
            {
                switch (ChipSku)
                {
                    case 0:
                        return "MT3620AN";
                    default:
                        return "?";
                }
            }
        }

        internal AzureSphereDeviceModel(AzureSphereTenant tenant, JToken json)
        {
            Tenant = tenant;

            Id = json.Value<string>("DeviceId");
            DeviceGroupId = json.Value<string>("DeviceGroupId");
            ChipSku = json.Value<int>("ChipSku");
        }

    }
}
