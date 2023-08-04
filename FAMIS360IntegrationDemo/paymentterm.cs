using System;
using System.Collections.Generic;
using System.Text;

namespace FAMIS360IntegrationDemo
{
    public class paymentterm
    {
        public static string url = "/apis/360facility/v1/paymentterms";

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TabOrder { get; set; }
        public bool Active { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime UpdateDate { get; set; }


    }
}
