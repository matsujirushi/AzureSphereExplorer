using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSphereExplorer
{
    class DeviceGroupModel
    {
        public AzureSphereDeviceGroup Context;

        public string Product { get; set; }
        public string DeviceGroup { get; set; }
        public string Description { get; set; }
        public string OsFeedType { get; set; }
        public string UpdatePolicy { get; set; }
        public DateTime? CurrentDeploymentDate { get; set; }
    }
}
