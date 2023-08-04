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
            

            //Now we can get all our data need for posting/patching Companies in FAMIS 360
            try
            {
                
                workdayList = getWorkdayData();
            }
            catch (Exception e)
            {

                throw e;
            }

            //Next steps are to look over all your incoming data to see what needs to go to FAMIS360
            

            //last step is to upload new records and patch any changes
            



        }

        


        public static List<workday> getWorkdayData()
        {
            string jsonFile = File.ReadAllText("workday.json");
            return JsonConvert.DeserializeObject<List<workday>>(jsonFile);
        }

        #region api calls


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