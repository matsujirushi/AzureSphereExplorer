using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureSpherePublicAPI;

namespace AzureSphereExplorer
{
    class DeviceModelEx
    {
        public DeviceModel DeviceModel;

        public string Product { get; set; }
        public string DeviceGroup { get; set; }
        public string ChipSku { get; set; }
        public string Id { get; set; }
        public bool IsChecked { get; set; }

        public DeviceModelEx(DeviceModel model, bool isChecked)
        {
            this.DeviceModel = model;
            this.Product = model.Product;
            this.DeviceGroup = model.DeviceGroup;
            this.ChipSku = model.ChipSku;
            this.Id = model.Id;
            this.IsChecked = isChecked;
        }
    }
}
