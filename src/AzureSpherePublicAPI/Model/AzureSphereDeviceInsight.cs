using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSpherePublicAPI
{
    public class AzureSphereDeviceInsight
    {
        public string DeviceId { get; private set; }
        public DateTime StartTimestamp { get; private set; }
        public DateTime EndTimestamp { get; private set; }
        public string Description { get; private set; }
        public string EventType { get; private set; }
        public string EventClass { get; private set; }
        public string EventCategory { get; private set; }
        public int EventCount { get; private set; }

        internal AzureSphereDeviceInsight(JToken json)
        {
            DeviceId = json.Value<string>("DeviceId");
            StartTimestamp = DateTimeOffset.FromUnixTimeSeconds(json.Value<int>("StartTimestampInUnix")).DateTime;
            EndTimestamp = DateTimeOffset.FromUnixTimeSeconds(json.Value<int>("EndTimestampInUnix")).DateTime;
            Description = json.Value<string>("Description");
            EventType = json.Value<string>("EventType");
            EventClass = json.Value<string>("EventClass");
            EventCategory = json.Value<string>("EventCategory");
            EventCount = json.Value<int>("EventCount");
        }

    }
}
