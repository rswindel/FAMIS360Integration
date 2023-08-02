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

        static async Task Main(string[] args)
        {
            url = f360Resource.url; // URL for the famis enviroment you are connecting to.
            username = f360Resource.user;//Username for connecting to FAMIS360
            password = f360Resource.password; //Password for connecting to FAMIS360
            timeout = 300000; //Timeout for API functions.  This is probalby need as some endpoint take longer to return.

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Getting Login Token");
            getlogintoken(null, null, null);
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} - Finished getting Login Token");

            getFamisData();
            //await getFamisDataAsync();




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