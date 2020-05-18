using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereDevice
    {
        private AzureSphereTenant Tenant;

        public string Id { get; private set; }
        public string DeviceGroupId { get; private set; }

        internal AzureSphereDevice(AzureSphereTenant tenant, JToken json)
        {
            Tenant = tenant;

            Id = json.Value<string>("DeviceId");
            DeviceGroupId = json.Value<string>("DeviceGroupId");
        }

    }
}
