using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereImage
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ComponentId { get; private set; }
        public int ImageType { get; private set; }

        internal AzureSphereImage(JToken json)
        {
            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
            Description = json.Value<string>("Description");
            ComponentId = json.Value<string>("ComponentId");
            ImageType = json.Value<int>("ImageType");
        }

    }
}
