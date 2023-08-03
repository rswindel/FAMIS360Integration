using Newtonsoft.Json;
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
            public DateTime? UpdateDate { get; set; }
            public string ExternalId { get; set; }
            public bool TimeCardFlag { get; set; }
            public bool VendorFlag { get; set; }
            public bool MinorityFlag { get; set; }
            public bool WomanOwnedFlag { get; set; }
            public bool PreferredVendorFlag { get; set; }
            public bool SupplierFlag { get; set; }
            public bool SubcontractorAuthFlag { get; set; }
            public bool W9OnFileFlag { get; set; }
            public int? CurrencyInstallId { get; set; }
            public string Addr2 { get; set; }
            public string Fax { get; set; }
            public string Website { get; set; }
            public string EmergencyPhone { get; set; }
            public string Email { get; set; }
            public string PagerNumber { get; set; }
            public string PrimaryContactName { get; set; }
            public int? CategoryId { get; set; }
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
            public int? RemStateId { get; set; }
            public string Description { get; set; }
            public bool VisitAutoCreateFlag { get; set; }
            public bool DebtorFlag { get; set; }
            public bool LandOwnerFlag { get; set; }
            public bool MeterSiteFlag { get; set; }
            public bool ExtMasterCompanyFlag { get; set; }
            public companytype CompanyType { get; set; }
            public Currency Currency { get; set; }
            public Companycategory CompanyCategory { get; set; }
            public object SecondaryCategory { get; set; }
            public object PaymentTerm { get; set; }
            public object ShippingMethod { get; set; }
            public object TypeOfAccess { get; set; }
            public object CompanyFreeOnBoard { get; set; }

            /// <summary>
            /// Checks to see if there are any changes between two companies
            /// </summary>
            /// <param name="other">Company to compare</param>
            /// <returns>True if differences are detected</returns>
            public bool hasChanges(company other)
            {
                bool retval = false;
                if(this.ExternalId != other.ExternalId) //These companies are not for the some company
                    return retval;

                if(this.Name != other.Name) retval = true;
                if (this.Phone != other.Phone) retval = true;
                if (this.Zip != other.Zip) retval = true;
                if (this.StateId != other.StateId) retval = true;
                if (this.CountryId != other.CountryId) retval = true;
                if (this.Addr1 != other.Addr1) retval = true;
                if (this.Addr2 != other.Addr2) retval = true;
                if (this.City != other.City) retval = true;
                if (this.ActiveFlag != other.ActiveFlag) retval = true;
                if(this.PaymentTermId != other.PaymentTermId) retval = true;

                return retval;
            }

            /// <summary>
            /// Merges two company records into one.
            /// </summary>
            /// <param name="other">changes to merge into this company</param>
            /// <returns></returns>
            public company mergeChanges(company other)
            {
                company retval = JsonConvert.DeserializeObject<company>(JsonConvert.SerializeObject(this));

                if (other.Name != null && this.Name != other.Name)
                {
                    retval.Name = other.Name;
                    
                }
                if (this.ActiveFlag != other.ActiveFlag)
                {
                    retval.ActiveFlag = other.ActiveFlag;
                }
                if (other.Phone != null && this.Phone != other.Phone)
                {
                    retval.Phone = other.Phone;
                }
                if (other.Addr1 != null && this.Addr1 != other.Addr1)
                {
                    retval.Addr1 = other.Addr1;
                }
                if (other.Addr2 != null && this.Addr2 != other.Addr2)
                {
                    retval.Addr2 = other.Addr2;
                }
                if (other.City != null && this.City != other.City)
                {
                    retval.City = other.City;
                }
                if (other.Zip != null && this.Zip != other.Zip)
                {
                    retval.Zip = other.Zip;
                }
                if (this.StateId != other.StateId)
                {
                    retval.StateId = other.StateId;
                }
                if (this.CountryId != other.CountryId)
                {
                    retval.CountryId = other.CountryId;
                }
                return retval;
            }

            public string toJsonString()
            {
                string retval;
                PostPatchObject json = JsonConvert.DeserializeObject<PostPatchObject>(JsonConvert.SerializeObject(this));
                retval = JsonConvert.SerializeObject(json);
                return retval;
            }

        }

        public class companytype
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


        /// <summary>
        /// This object is need to convert a company object into something that can be used for posting or patching.
        /// </summary>
        public class PostPatchObject
        {
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
            public DateTime? UpdateDate { get; set; }
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
            public int? CategoryId { get; set; }
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
            public string Addr3 { get; set; }
            public string RemAddr1 { get; set; }
            public string RemAddr2 { get; set; }
            public string RemAddr3 { get; set; }
            public string RemCity { get; set; }
            public string RemZip { get; set; }
            public int? RemStateId { get; set; }
            public string Description { get; set; }
            public bool MeterSiteFlag { get; set; }
        }



    }
}
