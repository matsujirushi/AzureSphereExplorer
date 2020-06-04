using AzureSpherePublicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSphereExplorer
{
    class DeviceInsightModel
    {
        public AzureSphereDeviceInsight Context;

        public string DeviceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public string EventClass { get; set; }
        public string EventCategory { get; set; }
        public int EventCount { get; set; }
    }
}
