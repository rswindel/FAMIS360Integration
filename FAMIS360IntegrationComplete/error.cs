using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMIS360IntegrationComplete
{
    /// <summary>
    /// Class for if we get an error from the API
    /// </summary>
    public class error
    {

        public class Rootobject
        {
            public string odatacontext { get; set; }
            public string value { get; set; }
        }

        public class httperror
        {
            public bool Result { get; set; }
            public int Context { get; set; }
            public string Message { get; set; }
        }

    }
}
