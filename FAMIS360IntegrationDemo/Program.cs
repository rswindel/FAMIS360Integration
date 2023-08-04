using Newtonsoft.Json;
using static FAMIS360IntegrationDemo.companies;

namespace FAMIS360IntegrationDemo
{
    internal class Program
    {
        internal static JsonSerializerSettings jsonsettingsin = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };
        internal static JsonSerializerSettings jsonsettingsout = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        private static login.loginobject f360Login;  //Login infrmation if we need to get a refresh;
        private static login.loginresponse.Rootobject logincreds = new login.loginresponse.Rootobject(); //Response data from getting a login token.
        private static string username;
        private static string password;
        private static string url;
        private static string proxyserver;
        private static int? timeout;

        private static List<company> companyList;
        private static List<countries> countriesList;
        private static List<states> statesList;
        private static List<paymentterm> paymenttermList;
        private static List<workday> workdayList;

        private static List<company> newCompanies = new List<company>();
        private static List<company> updatedCompanies = new List<company>();

        static async Task Main(string[] args)
        {
            url = f360Resource.url; // URL for the famis enviroment you are connecting to.
            username = f360Resource.user;//Username for connecting to FAMIS360
            password = f360Resource.password; //Password for connecting to FAMIS360
            timeout = 300000; //Timeout for API functions.  This is probalby need as some endpoint take longer to return.

            //We first need to get logged into FAMIS360
            try
            {
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Login Token");
                getlogintoken(null, null, null);
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Login Token");
            }
            catch (Exception e)
            {

                throw e;
            }

            //Now we can get all our data need for posting/patching Companies in FAMIS 360
            try
            {
                getFamisData();
                //await getFamisDataAsync();
                workdayList = getWorkdayData();
            }
            catch (Exception e)
            {

                throw e;
            }

            //Next steps are to look over all your incoming data to see what needs to go to FAMIS360
            foreach (var record in workdayList)
            {
                //we will convert the incoming data to a company record
                company incoming = new company();

                //Defualt values that are required by the API to post or patch.
                incoming.CurrencyInstallId = 1;
                incoming.TypeId = 1;


                incoming.ExternalId = record.Supplier_ID; //This is the unique idenifier from Workday
                incoming.Name = record.Supplier_Name;
                incoming.Phone = record.Supplier_Landline_Phones;
                incoming.City = record.city;
                incoming.Addr1 = record.addressLine1;
                incoming.Addr2 = record.addressLine2;
                incoming.Zip = record.postalCode;

                if (record.Supplier_Status == "Active")
                    incoming.ActiveFlag = true;
                else
                    incoming.ActiveFlag = false;

                //We have to find the Country ID out of data we got from FAMIS360
                incoming.CountryId = countriesList.Find(x => x.Name.ToUpper() == record.country.ToUpper()).Id;

                //now that we have the countryId, we can use that to help us get the stateId
                incoming.StateId = statesList.Find(x => x.CountryId == incoming.CountryId && x.Abbreviation == record.State_ISO_Code).Id;

                //Finding the payment term we just need to search for it.
                incoming.PaymentTermId = paymenttermList.Find(x => x.Name == record.Payment_Term_ID.ToUpper()).Id;

                //With the company converted to a FAMIS360 record we can check if it's already in FAMIS360.
                //If we find it we need to check if there are any changes.
                company exsiting = companyList.Find(x => x.ExternalId == incoming.ExternalId);
                if (exsiting != null)
                {
                    if (exsiting.hasChanges(incoming))
                    {
                        company merged = exsiting.mergeChanges(incoming);
                        merged.UpdateDate = DateTime.Now;
                        updatedCompanies.Add(merged);
                    }
                }
                else
                {
                    //Could not find company so It's is a new company that needs to be created.
                    incoming.UpdateDate = DateTime.Now;
                    newCompanies.Add(incoming);
                }

            }

            //last step is to upload new records and patch any changes
            foreach (var upload in updatedCompanies)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Patching Company {upload.Name}");
                    patchcompanies(upload);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }

            foreach (var upload in newCompanies)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Posting Company {upload.Name}");
                    postcompanies(upload);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }



        }

        public static void getFamisData()
        {
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Company Data");
            companyList = getCompanies();
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Company Data");

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Countries Data");
            countriesList = getcountries();
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Countries Data");

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting State Data");
            statesList = getstates();
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting State Data");

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Payment Term Data");
            paymenttermList = getpaymentterm();
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Payment Term Data");
        }


        public static List<workday> getWorkdayData()
        {
            string jsonFile = File.ReadAllText("workday.json");
            return JsonConvert.DeserializeObject<List<workday>>(jsonFile);
        }

        #region api calls

        /// <summary>
        /// Get you a Bearer Token to make API calls
        /// </summary>
        /// <param name="username">username to use for login</param>
        /// <param name="password">password to use for login</param>
        /// <param name="url">url for service instance</param>
        /// <param name="proxyserver">proxyserver info (host:port), (can be null)</param>
        /// <param name="timeout">timeout in milliseconds (can be null)</param>
        public static void getlogintoken(string _username, string _password, string _url, string _proxyserver = null, int? _timeout = null)
        {

            if (username == null)
                username = _username;
            if (password == null)
                password = _password;
            if (url == null)
                url = _url;
            if (proxyserver == null)
                proxyserver = _proxyserver;
            if (timeout == null)
                timeout = _timeout;

            if (logincreds.Item.expires < DateTime.Now.ToUniversalTime()) //
            {
                try
                {
                    login.loginresponse.Rootobject lt = new login.loginresponse.Rootobject();
                    login.loginobject lo = new login.loginobject(username, password);
                    lt = JsonConvert.DeserializeObject<login.loginresponse.Rootobject>(apifunctions.getlogin(url + "/api/login", lo, proxyserver, timeout));
                    logincreds = lt;

                    if (logincreds.Item.access_token != null) //do this if we actually have a token.  if we don't something failed and this won't help.
                    {
                        f360Login = lo;
                    }
                }
                catch (Exception e)
                {

                    throw e;
                }

            }
            else
            {


            }
        }

        private static bool refreshtoken()
        {


            try
            {
                //call the refresh function and replace the logincreds object.
                login.refreshobject thisrefresh = new login.refreshobject(logincreds.Item.refresh_token);
                login.loginresponse.Rootobject lt = new login.loginresponse.Rootobject();
                lt = JsonConvert.DeserializeObject<login.loginresponse.Rootobject>(apifunctions.postobject(logincreds.Item.access_token, url + "/api/refreshtoken", thisrefresh, out int httpcode, proxyserver, timeout));
                logincreds = lt;
                return true;
            }
            catch (NullReferenceException)
            {
                getlogintoken(null, null, null);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        /// <summary>
        /// Gets all Companies from FAMIS360
        /// </summary>
        /// <param name="urlargs">OData</param>
        /// <returns>List of companies</returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public static List<company> getCompanies(string urlargs = null)
        {
            
        }

        /// <summary>
        /// Gets all Countries from FAMIS 360
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public static List<countries> getcountries()
        {
            List<countries> retval = new List<countries>();
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(apifunctions.getobject(logincreds.Item.access_token, url + countries.url, out int httpcode, proxyserver, timeout));
            string tmpstrng = holder.value.ToString();
            if (httpcode.ToString() == "200")
            {
                retval = JsonConvert.DeserializeObject<List<countries>>(holder.value.ToString(), jsonsettingsin);
            }

            else
            {
                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                throw new Exception(newerror.Message);
            }
            return retval;
        }

        /// <summary>
        /// Gets all Payment Terms from FAMIS 360
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public static List<paymentterm> getpaymentterm()
        {

            List<paymentterm> retval = new List<paymentterm>();
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(apifunctions.getobject(logincreds.Item.access_token, url + paymentterm.url, out int httpcode, proxyserver, timeout));
            //string tmpstrng = holder.value.ToString();
            if (httpcode.ToString() == "200")
                retval = JsonConvert.DeserializeObject<List<paymentterm>>(holder.value.ToString(), jsonsettingsin);
            else
            {
                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                throw new Exception(newerror.Message);
            }
            return retval;
        }

        /// <summary>
        /// Gets all States from FAMIS 360
        /// </summary>
        /// <param name="urlargs">OData</param>
        /// <returns></returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public static List<states> getstates(string urlargs = null)
        {
            List<states> retval = new List<states>();
            string serviceurl = url + states.url;
            if (urlargs != null)
                serviceurl += urlargs;
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(apifunctions.getobject(logincreds.Item.access_token, serviceurl, out int httpcode, proxyserver, timeout));
            //string tmpstrng = holder.value.ToString();
            if (httpcode.ToString() == "200")
                retval = JsonConvert.DeserializeObject<List<states>>(holder.value.ToString(), jsonsettingsin);
            else
            {
                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                throw new Exception(newerror.Message);
            }
            return retval;
        }


        #endregion
    }
}