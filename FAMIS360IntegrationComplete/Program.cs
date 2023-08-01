using Newtonsoft.Json;
using static FAMIS360IntegrationComplete.companies;

namespace FAMIS360IntegrationComplete
{
    internal class Program
    {
        internal static JsonSerializerSettings jsonsettingsin = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };
        internal static JsonSerializerSettings jsonsettingsout = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        private static login.loginobject f360Login;  //Login infrmation if we need to get a refresh;
        private static login.loginresponse.Rootobject logincreds = new login.loginresponse.Rootobject(); //Response data from getting a login token.

        static void Main(string[] args)
        {
            string url = "https://isu.stage.famis360.com/MobileWebServices"; // URL for the famis enviroment you are connecting to.
            string UserName = "";//TODO Need to clear this to resourse file once on laptop
            string Password = ""; //password for connecting to FAMIS360
            string proxyserver = null; //If proxy server is need to get to FAMIS360
            int timeout = 300000; //Timeout for API functions.  This is probalby need as some endpoint take longer to return.

        }


        #region api calls

        /// <summary>
        /// assumes you didn't set anything and you want to set it now.
        /// </summary>
        /// <param name="username">username to use for login</param>
        /// <param name="password">password to use for login</param>
        /// <param name="url">url for service instance</param>
        /// <param name="proxyserver">proxyserver info (host:port), (can be null)</param>
        /// <param name="timeout">timeout in milliseconds (can be null)</param>
        public static void getlogintoken(string username, string password, string url, string proxyserver = null, int? timeout = null)
        {
            
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



        #endregion


    }
}