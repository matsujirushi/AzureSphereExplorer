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
        public int Type { get; private set; }

        public string TypeStr
        {
            get
            {
                switch (Type)
                {
                    case 0:
                        return "Applications";
                    case 1:
                        return "BaseSystemUpdateManifest";
                    case 2:
                        return "BootManifest";
                    case 3:
                        return "CustomerBoardConfig";
                    case 4:
                        return "CustomerUpdateManifest";
                    case 5:
                        return "FirmwareUpdateManifest";
                    case 6:
                        return "FwConfig";
                    case 7:
                        return "InvalidImageType";
                    case 8:
                        return "ManifestSet";
                    case 9:
                        return "NormalWorldDtb";
                    case 10:
                        return "NormalWorldKernel";
                    case 11:
                        return "NormalWorldLoader";
                    case 12:
                        return "Nwfs";
                    case 13:
                        return "OneBl";
                    case 14:
                        return "Other";
                    case 15:
                        return "PlutonRuntime";
                    case 16:
                        return "Policy";
                    case 17:
                        return "RecoveryManifest";
                    case 18:
                        return "RootFs";
                    case 19:
                        return "SecurityMonitor";
                    case 20:
                        return "Services";
                    case 21:
                        return "TrustedKeystore";
                    case 22:
                        return "UpdateCertStore";
                    case 23:
                        return "WifiFirmware";
                    default:
                        return "?";
                }
            }
        }


        internal AzureSphereImage(JToken json)
        {
            Id = json.Value<string>("Id");
            Name = json.Value<string>("Name");
            Description = json.Value<string>("Description");
            ComponentId = json.Value<string>("ComponentId");
            Type = json.Value<int>("Type");
        }

    }
}
