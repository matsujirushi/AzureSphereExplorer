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
        public string Name { get; private set; }
        public string Id { get; private set; }

        internal AzureSphereTenant(JToken json)
        {
            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
        }

    }
}
