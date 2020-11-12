using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureSpherePublicAPI
{
    class AzureSphereOperation
    {
        public DateTime CompletedDateUTC { get; private set; }
        public DateTime CreatedDateUTC { get; private set; }
        public AzureSphereError Error { get; private set; }
        public string OperationId { get; private set; }
        public string OperationType { get; private set; }
        public string ResourceLocation { get; private set; }
        public DateTime StartedDateUTC { get; private set; }
        public int State { get; private set; }
        public string TenantId { get; private set; }
        internal AzureSphereOperation(JToken json)
        {
            string tmpCompletedDateUTC = json.Value<string>("CompletedDateUTC");
            if (tmpCompletedDateUTC != null)
            {
                CompletedDateUTC = changeDateTimefromString(tmpCompletedDateUTC);
            }
            string tmpCreatedDateUTC = json.Value<string>("CreatedDateUTC");

            if (tmpCreatedDateUTC != null)
            {
                CreatedDateUTC = changeDateTimefromString(tmpCreatedDateUTC);
            }

            Error = json["Error"].HasValues ? new AzureSphereError(json["Error"]) : null;
            OperationId = json.Value<string>("OperationId");
            OperationType = json.Value<string>("OperationType");

            ResourceLocation = json.Value<string>("ResourceLocation");

            string tmpStartedDateUTC = json.Value<string>("StartedDateUTC");

            if (tmpStartedDateUTC != null)
            {
                StartedDateUTC = changeDateTimefromString(tmpStartedDateUTC);
            }
            State = json.Value<int>("State");
            TenantId = json.Value<string>("TenantId");
        }

        private DateTime changeDateTimefromString(string datetime)
        {
            return DateTime.Parse(datetime);
        }
    }
}
