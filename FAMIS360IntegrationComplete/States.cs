using System;
using System.Collections.Generic;
using System.Text;

namespace FAMIS360IntegrationComplete
{
    public class states
    {
        public static string url = "/apis/360facility/v1/states";

        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool ActiveFlag { get; set; }
        public string Description { get; set; }
        public string StateType { get; set; }
        public string StateCode { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatedByName { get; set; }
        public int? TabOrder { get; set; }
        public bool? Default { get; set; }
        public string CoStAbbr { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", this.Id, this.CountryName, this.Name);
        }

    }
}
