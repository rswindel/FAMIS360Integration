using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMIS360IntegrationDemo
{
    internal class baseobject
    {
        [JsonProperty("@odata.context")]
        public string odatacontext { get; set; }
        public bool Result { get; set; }
        public int Context { get; set; }
        public string Message { get; set; }
        public object value { get; set; }
        [JsonProperty("@odata.nextLink")]
        public string odatanextlink { get; set; }
        [JsonProperty("@odata.count")]
        public int odatacount { get; set; }

        public string tojsonstring()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
