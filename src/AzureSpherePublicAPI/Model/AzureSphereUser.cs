using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereUser
    {
        public string DisplayName { get; private set; }
        public string Mail { get; private set; }
        public string PrincipalId { get; private set; }
        public List<string> Roles { get; private set; }

        internal AzureSphereUser(JToken json)
        {
            DisplayName = json.Value<string>("DisplayName");
            Mail = json.Value<string>("Mail");
            PrincipalId = json.Value<string>("PrincipalId");
            Roles = new List<string>();
            foreach (var role in json["Roles"])
            {
                Roles.Add(role.Value<string>());
            }
        }

    }
}
