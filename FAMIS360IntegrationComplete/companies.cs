using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMIS360IntegrationComplete
{

    /// <summary>
    /// Complete companies class from FAMIS360
    /// </summary>
    public class companies
    {
        public static string url = "/apis/360facility/v1/companies";

        public class company
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Addr1 { get; set; }
            public string City { get; set; }
            public string Zip { get; set; }
            public int StateId { get; set; }
            public string State { get; set; }
            public int CountryId { get; set; }
            public string Country { get; set; }
            public int TypeId { get; set; }
            public string Phone { get; set; }
            public bool ActiveFlag { get; set; }
            public DateTime UpdateDate { get; set; }
            public string ExternalId { get; set; }
            public bool TimeCardFlag { get; set; }
            public bool VendorFlag { get; set; }
            public bool MinorityFlag { get; set; }
            public bool WomanOwnedFlag { get; set; }
            public bool PreferredVendorFlag { get; set; }
            public bool SupplierFlag { get; set; }
            public bool SubcontractorAuthFlag { get; set; }
            public bool W9OnFileFlag { get; set; }
            public int CurrencyInstallId { get; set; }
            public string Addr2 { get; set; }
            public string Fax { get; set; }
            public string Website { get; set; }
            public string EmergencyPhone { get; set; }
            public string Email { get; set; }
            public string PagerNumber { get; set; }
            public string PrimaryContactName { get; set; }
            public int CategoryId { get; set; }
            public int? SecondaryCategoryId { get; set; }
            public string SicCode { get; set; }
            public string InternalVendorCode { get; set; }
            public string TaxpayerId { get; set; }
            public int? ContractTypeId { get; set; }
            public string ContractComments { get; set; }
            public string MobilePhone { get; set; }
            public string InternalVendorCode2 { get; set; }
            public string RiskRating { get; set; }
            public int? TypeOfAccessId { get; set; }
            public int? PaymentTermId { get; set; }
            public int? ShippingMethodId { get; set; }
            public int? FreeOnBoardId { get; set; }
            public string RoutingNumber { get; set; }
            public string Addr3 { get; set; }
            public string RemAddr1 { get; set; }
            public string RemAddr2 { get; set; }
            public string RemAddr3 { get; set; }
            public string RemCity { get; set; }
            public string RemZip { get; set; }
            public int RemStateId { get; set; }
            public string Description { get; set; }
            public bool VisitAutoCreateFlag { get; set; }
            public bool DebtorFlag { get; set; }
            public bool LandOwnerFlag { get; set; }
            public bool MeterSiteFlag { get; set; }
            public bool ExtMasterCompanyFlag { get; set; }
            public Companytype CompanyType { get; set; }
            public Currency Currency { get; set; }
            public Companycategory CompanyCategory { get; set; }
            public object SecondaryCategory { get; set; }
            public object PaymentTerm { get; set; }
            public object ShippingMethod { get; set; }
            public object TypeOfAccess { get; set; }
            public object CompanyFreeOnBoard { get; set; }
        }

        public class Companytype
        {
            public int Id { get; set; }
            public string Desc { get; set; }
            public bool OccupantFlag { get; set; }
        }

        public class Currency
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Abbreviation { get; set; }
            public int Code { get; set; }
            public bool ActiveFlag { get; set; }
            public bool InstalledFlag { get; set; }
            public int CurrencyInstallId { get; set; }
            public string Sign { get; set; }
        }

        public class Companycategory
        {
            public int Id { get; set; }
            public string Desc { get; set; }
            public DateTime UpdateDate { get; set; }
            public int UpdatedById { get; set; }
            public bool ActiveFlag { get; set; }
        }

    }
}
