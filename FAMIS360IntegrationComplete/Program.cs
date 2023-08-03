using Newtonsoft.Json;
using System.Data;
using System.Linq.Expressions;
using static FAMIS360IntegrationComplete.companies;

namespace FAMIS360IntegrationComplete
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
            foreach(var record in workdayList)
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

                if(record.Supplier_Status == "Active")
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
                if(exsiting != null)
                {
                    if(exsiting.hasChanges(incoming))
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
            foreach(var upload in updatedCompanies)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Patching Company {upload.Name}");
                    patchcompanies(upload);
                }
                catch(Exception ex) { Console.WriteLine(ex.Message); }
            }

            foreach(var upload in newCompanies)
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
            List<company> retval = new List<company>();
            string serviceurl = url + companies.url;
            if (urlargs != null)
                serviceurl += urlargs;
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            string basestring = apifunctions.getobject(logincreds.Item.access_token, serviceurl, out int httpcode, proxyserver, timeout);
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(basestring);

            if (httpcode.ToString() == "200")
            {
                string tmpstrng = holder.value.ToString();
                retval = JsonConvert.DeserializeObject<List<company>>(holder.value.ToString(), jsonsettingsin);
            }
            else
            {
                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(basestring);
                throw new Exception(newerror.Message);
            }
            bool hasmore = false;
            string newurl = "";
            //need to keep getting it until it is null, and deserialize the value and append it to the retval;
            if (holder.odatanextlink != null)
            {
                hasmore = true;
                newurl = holder.odatanextlink;
                while (hasmore)
                {
                    if (newurl.Substring(0, 5) != "https") //can you believe that when 
                        newurl = "https" + newurl.Substring(4);
                    if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                        refreshtoken();
                    baseobject holder2 = JsonConvert.DeserializeObject<baseobject>(apifunctions.getobject(logincreds.Item.access_token, newurl, out httpcode, proxyserver, timeout));
                    string tmpstrng2 = holder2.value.ToString();
                    if (httpcode.ToString() == "200")
                        retval.AddRange(JsonConvert.DeserializeObject<List<company>>(holder2.value.ToString(), jsonsettingsin));
                    else
                    {
                        error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder2.tojsonstring());
                        throw new Exception(newerror.Message);
                    }
                    if (holder2.odatanextlink == null)
                        hasmore = false;
                    else
                        newurl = holder2.odatanextlink;
                }
            }
            return retval;
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


        public static company patchcompanies(company thing)
        {
            try
            {
                company lt = new company();
                string updatetag = string.Format("?key={0}", thing.Id);
                if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                    refreshtoken();
                string result = apifunctions.patchobject(logincreds.Item.access_token, url + companies.url + updatetag, thing.toJsonString(), out int httpcode, proxyserver, timeout, jsonsettingsout); ;

                if (httpcode.ToString() == "200")
                    lt = JsonConvert.DeserializeObject<company>(result);
                else
                {
                    error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(result);
                    throw new Exception(newerror.Message);
                }
                return lt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static company postcompanies(company thing)
        {
            try
            {
                company lt = new company();

                string temp = apifunctions.postobject(logincreds.Item.access_token, url + companies.url, thing.toJsonString(), out int httpcode, proxyserver, timeout, jsonsettingsout);
                lt = JsonConvert.DeserializeObject<company>(temp);
                //if this isn't a companies, it errored, so throw it as an error.
                if (httpcode.ToString() == "200")
                    lt = JsonConvert.DeserializeObject<company>(temp);
                else
                {
                    error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(temp);
                    throw new Exception(newerror.Message);
                }
                return lt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        #endregion

        #region Async API Calls

        public async static Task getFamisDataAsync()
        {
            List<Task> tasks = new List<Task>();

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Company Data");
            Task<List<company>> companyTask = getcompaniesasync();
            Task companyComplete = companyTask.ContinueWith(x =>
            {
                companyList = x.Result;
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Company Data");
            });
            tasks.Add(companyComplete);

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Countries Data");
            Task<List<countries>> countriesTask = getcountriesasync();
            Task countriesComplete = countriesTask.ContinueWith(x =>
            {
                countriesList = x.Result;
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Countries Data");
            });
            tasks.Add(countriesComplete);


            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting State Data");
            Task<List<states>> statesTask = getStatesAsync();
            Task statesComplete = statesTask.ContinueWith(x =>
            {
                statesList = x.Result;
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting State Data");
            });
            tasks.Add(statesComplete);


            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Payment Term Data");
            Task<List<paymentterm>> paymenttermTask = getpaymenttermasync();
            Task paymenttermComplete = paymenttermTask.ContinueWith(x =>
            {
                paymenttermList = x.Result;
                Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Payment Term Data");
            });
            tasks.Add(paymenttermComplete);

            await Task.WhenAll( tasks );

        }

        /// <summary>
        /// Gets all States from FAMIS 360
        /// </summary>
        /// <param name="urlargs">OData</param>
        /// <returns></returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public async static Task<List<states>> getStatesAsync(string urlargs = null)
        {
            List<states> retval = new List<states>();
            string serviceurl = url + states.url;
            if (urlargs != null)
                serviceurl += urlargs;
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(await apifunctions.getobjectasync(logincreds.Item.access_token, serviceurl, proxyserver, timeout));
            //string tmpstrng = holder.value.ToString();
            try
            {
                retval = JsonConvert.DeserializeObject<List<states>>(holder.value.ToString(), jsonsettingsin);
            }
            catch (Exception e)
            {
                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                throw new Exception(newerror.Message);
            }

            return retval;
        }

        /// <summary>
        /// Gets all Companies from FAMIS360
        /// </summary>
        /// <param name="urlargs">OData</param>
        /// <returns>List of companies</returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public async static Task<List<company>> getcompaniesasync(string urlargs = null)
        {
            List<company> retval = new List<company>();

            string serviceurl = url + companies.url;
            if (urlargs != null)
                serviceurl += urlargs;
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(await apifunctions.getobjectasync(logincreds.Item.access_token, serviceurl, proxyserver, timeout));
            //string tmpstrng = holder.value.ToString();
            try
            {
                retval = JsonConvert.DeserializeObject<List<company>>(holder.value.ToString());
            }
            catch (Exception)
            {

                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                throw new Exception(newerror.Message);
            }
            bool hasmore = false;
            string newurl = "";
            //need to keep getting it until it is null, and deserialize the value and append it to the retval;
            if (holder.odatanextlink != null)
            {
                hasmore = true;
                newurl = holder.odatanextlink;
                while (hasmore)
                {
                    if (newurl.Substring(0, 5) != "https") //can you believe that when 
                        newurl = "https" + newurl.Substring(4);

                    baseobject holder2 = JsonConvert.DeserializeObject<baseobject>(await apifunctions.getobjectasync(logincreds.Item.access_token, newurl, proxyserver, timeout));
                    //string tmpstrng2 = holder2.value.ToString();
                    try
                    {

                        retval.AddRange(JsonConvert.DeserializeObject<List<company>>(holder2.value.ToString()));

                    }
                    catch (Exception)
                    {

                        error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                        throw new Exception(newerror.Message);
                    }
                    if (holder2.odatanextlink == null)
                        hasmore = false;
                    else
                        newurl = holder2.odatanextlink;
                }
            }

            return retval;
        }

        /// <summary>
        /// Gets all Countries from FAMIS 360
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Errors from FAMIS360</exception>
        public async static Task<List<countries>> getcountriesasync()
        {
            List<countries> retval = new List<countries>();
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(await apifunctions.getobjectasync(logincreds.Item.access_token, url + countries.url, proxyserver, timeout));
            string tmpstrng = holder.value.ToString();
            try
            {
                retval = JsonConvert.DeserializeObject<List<countries>>(holder.value.ToString(), jsonsettingsin);
            }
            catch (Exception)
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
        public async static Task<List<paymentterm>> getpaymenttermasync()
        {

            List<paymentterm> retval = new List<paymentterm>();
            if (logincreds.Item.expires.CompareTo(DateTime.UtcNow) <= 0)
                refreshtoken();
            baseobject holder = JsonConvert.DeserializeObject<baseobject>(await apifunctions.getobjectasync(logincreds.Item.access_token, url + paymentterm.url, proxyserver, timeout));
            //string tmpstrng = holder.value.ToString();
            try
            {
                retval = JsonConvert.DeserializeObject<List<paymentterm>>(holder.value.ToString(), jsonsettingsin);
            }
            catch (Exception)
            {
                error.httperror newerror = JsonConvert.DeserializeObject<error.httperror>(holder.tojsonstring());
                throw new Exception(newerror.Message);
            }
            return retval;
        }

        #endregion

    }
}