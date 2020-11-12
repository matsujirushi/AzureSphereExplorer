using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureSpherePublicAPI
{
    class AzureSphereError
    {
        public int Code { get; private set; }
        public string Message { get; private set; }
        internal AzureSphereError(JToken json)
        {
            Code = json.Value<int>("Code");
            Message = json.Value<string>("Message");
        }
    }
}
