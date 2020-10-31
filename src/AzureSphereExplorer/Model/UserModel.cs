using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSphereExplorer
{
    class UserModel
    {
        public AzureSphereUser Context;

        public string User { get; set; }
        public string Mail { get; set; }
        public string Roles { get; set; }
    }
}
