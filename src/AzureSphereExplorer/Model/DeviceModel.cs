using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSphereExplorer
{
    class DeviceModel
    {
        public AzureSphereDevice Context;

        public string Product { get; set; }
        public string DeviceGroup { get; set; }
        public string ChipSku { get; set; }
        public string Id { get; set; }
    }
}
