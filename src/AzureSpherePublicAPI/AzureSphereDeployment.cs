using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereDeployment
    {
        private AzureSphereTenant Tenant;

        public string Id { get; private set; }

        internal AzureSphereDeployment(AzureSphereTenant tenant, JToken json)
        {
            Tenant = tenant;

            //Id = json.Value<string>("DeviceId");
        }

    }
}
