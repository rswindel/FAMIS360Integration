using System;
using System.Collections.Generic;
using System.Text;

namespace FAMIS360IntegrationComplete
{
   
    public class countries
    {
        public static string url = "/apis/360facility/v1/countries";
        public int Id { get; set; }
        public string Name { get; set; }
        public bool ActiveFlag { get; set; }
        public string Description { get; set; }
        public string Abbreviation { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatedByName { get; set; }
        public int? TabOrder { get; set; }
        public bool? DefaultFlag { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", this.Id, this.Name); ;
        }

    }
}
