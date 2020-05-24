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
        public string Id { get; private set; }
        public string DeploymentDateUtc { get; private set; }
        public List<string> DeployedImages { get; private set; }

        internal AzureSphereDeployment(JToken json)
        {
            Id = json.Value<string>("Id");
            DeploymentDateUtc = json.Value<string>("DeploymentDateUtc");
            DeployedImages = new List<string>();
            foreach (var jsonImageId in json["DeployedImages"])
            {
                DeployedImages.Add(jsonImageId.Value<string>());
            }
        }

    }
}
