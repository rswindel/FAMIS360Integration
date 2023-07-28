using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FAMIS360IntegrationComplete
{
    public class apifunctions
    {
        /// <summary>
        /// gets a login token from famis360. 
        /// </summary>
        /// <param name="url">url for famis360 instance</param>
        /// <param name="logininfo">login payload with username/password</param>
        /// <param name="proxyserver">[nullable]string proxyserver (host:port)</param>
        /// <param name="timeout">[nullable]int timeout in milliseconds</param>
        /// <returns>(object) login data</returns>
        public static string getlogin(string url, login.loginobject logininfo, string proxyserver = null, int? timeout = null)
        {
            string serviceURL = url;
            string bodystring = JsonConvert.SerializeObject(logininfo);
            byte[] bodydata = System.Text.Encoding.ASCII.GetBytes(bodystring);
            string dgresult;
            dgresult = apifunctions.PostjsonRequest(serviceURL, "application/json", bodydata, out int a, null, proxyserver, timeout);
            return dgresult;
        }

        /// <summary>
        /// gets something from the API
        /// </summary>
        /// <param name="bearer">(string) access_token from logininfo</param>
        /// <param name="url">url for famis360 instance</param>
        /// <param name="proxyserver">[nullable]string proxyserver (host:port)</param>
        /// <param name="timeout">[nullable]int timeout in milliseconds</param>
        /// <returns></returns>
        public static string getobject(string bearer, string url, out int httpcode, string proxyserver = null, int? timeout = 1000000)
        {
            string dgresult;
            dgresult = apifunctions.GetRequestjson(url, "application/json", out httpcode, bearer, proxyserver, timeout);
            return dgresult;
        }


        /// <summary>
        /// gets something from the API
        /// </summary>
        /// <param name="bearer">(string) access_token from logininfo</param>
        /// <param name="url">url for famis360 instance</param>
        /// <param name="proxyserver">[nullable]string proxyserver (host:port)</param>
        /// <param name="timeout">[nullable]int timeout in milliseconds</param>
        /// <returns></returns>
        public static async Task<string> getobjectasync(string bearer, string url, string proxyserver = null, int? timeout = 1000000)
        {
            return await apifunctions.GetRequestjsonasync(url, "application/json", bearer, proxyserver, timeout);

        }


        //post

        /// <summary>
        /// serializes an object as JSON and POSTs it to the URL.  if the object is a string, it just takes the string instead of trying to serialize it so you can send your own json serialized string.
        /// </summary>
        /// <param name="bearer"></param>
        /// <param name="url"></param>
        /// <param name="uploaddata"></param>
        /// <param name="httpcode"></param>
        /// <param name="proxyserver"></param>
        /// <param name="timeout"></param>
        /// <param name="jsonsettings"></param>
        /// <returns></returns>
        public static string postobject(string bearer, string url, object uploaddata, out int httpcode, string proxyserver = null, int? timeout = 1000000, JsonSerializerSettings jsonsettings = null)
        {
            string bodystring = "";
            if (uploaddata is string)
                bodystring = (string)uploaddata;
            else
                bodystring = JsonConvert.SerializeObject(uploaddata, jsonsettings);
            byte[] bodydata = System.Text.Encoding.ASCII.GetBytes(bodystring);
            string dgresult;
            dgresult = apifunctions.PostjsonRequest(url, "application/json", bodydata, out httpcode, bearer, proxyserver, timeout);

            return dgresult;
        }

        //patch
        public static string patchobject(string bearer, string url, object uploaddata, out int httpcode, string proxyserver = null, int? timeout = 1000000, JsonSerializerSettings jsonsettings = null)
        {
            string bodystring = "";
            if (uploaddata is string)
                bodystring = (string)uploaddata;
            else
                bodystring = JsonConvert.SerializeObject(uploaddata, jsonsettings);
            byte[] bodydata = System.Text.Encoding.ASCII.GetBytes(bodystring);
            string dgresult;
            dgresult = apifunctions.PutJsonRequest(url, "application/json", bodydata, out httpcode, bearer, proxyserver, timeout);
            return dgresult;
        }

        //delete
        public static string deleteobject(string bearer, string url, out int httpcode, string proxyserver = null, int? timeout = 1000000, JsonSerializerSettings jsonsettings = null)
        {
            string dgresult;
            dgresult = apifunctions.DeletejsonRequest(url, "application/json", null, out httpcode, bearer, proxyserver, timeout);
            return dgresult;
        }



        #region privatefunctions
        /// <summary>
        /// Makes HTTP GET request as specified user using Basic Authentication
        /// </summary>
        /// <param name="requestUrl">URL of request</param>
        /// <param name="contentType">Type of content to return (application/json as an example)</param>
        /// <param name="username">(string) username</param>
        /// <param name="password">(string) password</param>
        /// <param name="proxyserver">name of proxy server.  use server:port as format.</param>
        /// <returns>(string) data returned from api call</returns>
        private static string GetRequestjson(string requestUrl, string contentType, out int httpcode, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            return _MakeRequestjson(requestUrl, contentType, "GET", null, out httpcode, bearertoken, proxyserver, timeout_ms);
        }

        /// <summary>
        /// Makes HTTP GET request as specified user using Basic Authentication async
        /// </summary>
        /// <param name="requestUrl">URL of request</param>
        /// <param name="contentType">Type of content to return (application/json as an example)</param>
        /// <param name="username">(string) username</param>
        /// <param name="password">(string) password</param>
        /// <param name="proxyserver">name of proxy server.  use server:port as format.</param>
        /// <returns>(string) data returned from api call</returns>
        private static async Task<string> GetRequestjsonasync(string requestUrl, string contentType, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            return await _MakeRequestjsonAsync(requestUrl, contentType, "GET", null, bearertoken, proxyserver, timeout_ms);
        }

        /// <summary>
        /// Makes HTTP PUT request as current user
        /// </summary>
        /// <param name="requestUrl">URL of request</param>
        /// <param name="contentType">Type of content to return (application/json as an example)</param>
        /// <param name="bodydata">byte[] byte array of data to pass to request.  use UTF8 encoding if string.</param>
        /// <param name="proxyserver">name of proxy server.  use server:port as format.</param>
        /// <returns>(string) data returned from api call</returns>
        private static string PutJsonRequest(string requestUrl, string contentType, byte[] bodydata, out int httpcode, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            return _MakeRequestjson(requestUrl, contentType, "PATCH", bodydata, out httpcode, bearertoken, proxyserver, timeout_ms);
        }

        /// <summary>
        /// makes HTTP POST request as specified user using Basic Authentication
        /// </summary>
        /// <param name="requestUrl">URL of request</param>
        /// <param name="contentType">Type of content to return (application/json as an example)</param>
        /// <param name="bodydata">byte[] byte array of data to pass to request.  use UTF8 encoding if string.</param>
        /// <param name="username">(string) username</param>
        /// <param name="password">(string) password</param>
        /// <param name="proxyserver">name of proxy server.  use server:port as format.</param>
        /// <returns>(string) data returned from api call</returns>
        private static string PostjsonRequest(string requestUrl, string contentType, byte[] bodydata, out int httpcode, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            return _MakeRequestjson(requestUrl, contentType, "POST", bodydata, out httpcode, bearertoken, proxyserver, timeout_ms);
        }

        /// <summary>
        /// Makes HTTP DELETE request as current user
        /// </summary>
        /// <param name="requestUrl">URL of request</param>
        /// <param name="contentType">Type of content to return (application/json as an example)</param>
        /// <param name="bodydata">byte[] byte array of data to pass to request.  use UTF8 encoding if string.</param>
        /// <param name="proxyserver">name of proxy server.  use server:port as format.</param>
        /// <returns>(string) data returned from api call</returns>
        private static string DeletejsonRequest(string requestUrl, string contentType, byte[] bodydata, out int httpcode, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            return _MakeRequestjson(requestUrl, contentType, "DELETE", bodydata, out httpcode, bearertoken, proxyserver, timeout_ms);
        }

        /// <summary>
        /// process http request
        /// </summary>
        /// <param name="_requestUrl">URL to request</param>
        /// <param name="_contentType">Content type for return</param>
        /// <param name="_method">HTTP Method</param>
        /// <param name="_bodydata">byte[] body data as byte array</param>
        /// <param name="_username">(string) username</param>
        /// <param name="_password">(string) password</param>
        /// <param name="useproxy">set to true if using a proxy server</param>
        /// <param name="proxyserver">proxy server and port "server:port"</param>
        /// <param name="timeout_ms">timeout in milliseconds.  if null, uses default.</param>
        /// <returns>(string) json return data</returns>
        private static string _MakeRequestjson(string _requestUrl, string _contentType, string _method, byte[] _bodydata, out int httpcode, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = WebRequest.Create(_requestUrl) as HttpWebRequest;
                request.Method = _method.ToUpper();
                request.ContentType = _contentType;
                request.Accept = _contentType;
                if (timeout_ms != null)
                    request.Timeout = timeout_ms.Value;
                if (bearertoken != null)
                {
                    request.Headers.Add("Authorization", "Bearer " + bearertoken);
                }
                if (proxyserver != null)
                {
                    string server = proxyserver.Substring(0, proxyserver.IndexOf(':'));
                    int port = int.Parse(proxyserver.Substring(proxyserver.IndexOf(':') + 1));
                    WebProxy newproxy = new WebProxy(server, port);
                    request.Proxy = newproxy;
                }

                if (_method.ToUpper() != "GET")
                {
                    //determine if datastream has content, add if it it does, otherwise no body, just do the method.
                    if (_bodydata != null && _bodydata.Length > 0)
                    {
                        request.ContentLength = _bodydata.Length;
                        Stream bodystream = request.GetRequestStream();
                        bodystream.Write(_bodydata, 0, _bodydata.Length);
                        bodystream.Close();
                    }
                    else
                    {
                        request.ContentLength = 0;
                        request.ContentType = null;
                    }
                }

                HttpWebResponse response;
                try
                {
                    response = request.GetResponse() as HttpWebResponse;
                }
                catch (WebException s)
                {
                    response = (HttpWebResponse)s.Response;
                    //throw;
                }


                string returnval = "";
                if (_method.ToUpper() != "DELETE")
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        if (response.ContentLength != 0)
                        {
                            StreamReader streaminfo = new StreamReader(response.GetResponseStream());
                            returnval = streaminfo.ReadToEnd();
                        }
                    }
                    else
                    {
                        if (response.ContentLength != 0)
                        {
                            StreamReader streaminfo = new StreamReader(response.GetResponseStream());
                            returnval = streaminfo.ReadToEnd();
                        }
                        else
                            throw new WebException(response.StatusCode + " | " + response.StatusDescription);
                    }
                    httpcode = (int)response.StatusCode;
                    return (returnval);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (response.ContentLength != 0)
                        {
                            StreamReader streaminfo = new StreamReader(response.GetResponseStream());
                            returnval = streaminfo.ReadToEnd();
                        }
                        httpcode = (int)response.StatusCode;
                        return returnval;
                    }
                    else
                        throw new WebException(response.StatusCode + " | " + response.StatusDescription);
                }

            }
            catch (WebException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// process http request
        /// </summary>
        /// <param name="_requestUrl">URL to request</param>
        /// <param name="_contentType">Content type for return</param>
        /// <param name="_method">HTTP Method</param>
        /// <param name="_bodydata">byte[] body data as byte array</param>
        /// <param name="_username">(string) username</param>
        /// <param name="_password">(string) password</param>
        /// <param name="useproxy">set to true if using a proxy server</param>
        /// <param name="proxyserver">proxy server and port "server:port"</param>
        /// <param name="timeout_ms">timeout in milliseconds.  if null, uses default.</param>
        /// <returns>(string) json return data</returns>
        private static async Task<string> _MakeRequestjsonAsync(string _requestUrl, string _contentType, string _method, byte[] _bodydata, string bearertoken = null, string proxyserver = null, int? timeout_ms = null)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = WebRequest.Create(_requestUrl) as HttpWebRequest;
                request.Method = _method.ToUpper();
                request.ContentType = _contentType;
                request.Accept = _contentType;
                if (timeout_ms != null)
                    request.Timeout = timeout_ms.Value;
                if (bearertoken != null)
                {
                    request.Headers.Add("Authorization", "Bearer " + bearertoken);
                }
                if (proxyserver != null)
                {
                    string server = proxyserver.Substring(0, proxyserver.IndexOf(':'));
                    int port = int.Parse(proxyserver.Substring(proxyserver.IndexOf(':') + 1));
                    WebProxy newproxy = new WebProxy(server, port);
                    request.Proxy = newproxy;
                }

                if (_method.ToUpper() != "GET")
                {
                    //determine if datastream has content, add if it it does, otherwise no body, just do the method.
                    if (_bodydata != null && _bodydata.Length > 0)
                    {
                        request.ContentLength = _bodydata.Length;
                        Stream bodystream = request.GetRequestStream();
                        bodystream.Write(_bodydata, 0, _bodydata.Length);
                        bodystream.Close();
                    }
                    else
                    {
                        request.ContentLength = 0;
                        request.ContentType = null;
                    }
                }

                HttpWebResponse response;

                try
                {
                    string retval;
                    response = (HttpWebResponse)await request.GetResponseAsync();

                    if (_method.ToUpper() != "DELETE")
                    {
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Accepted)
                        {
                            using (Stream streamResponse = response.GetResponseStream())
                            {
                                retval = new StreamReader(streamResponse).ReadToEnd();
                            }
                        }
                        else
                        {
                            if (response.ContentLength != 0)
                            {
                                using (Stream streamResponse = response.GetResponseStream())
                                {
                                    retval = new StreamReader(streamResponse).ReadToEnd();
                                }
                            }
                            else
                                throw new WebException(response.StatusCode + " | " + response.StatusDescription);
                        }

                        return (retval);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream streamResponse = response.GetResponseStream())
                            {
                                retval = new StreamReader(streamResponse).ReadToEnd();
                            }
                            return retval;
                        }
                        else
                            throw new WebException(response.StatusCode + " | " + response.StatusDescription);
                    }
                }

                catch (Exception ee)
                {
                    //_logger.Error(new HttpErrorLog { Request = webRequest, Url = webRequest.RequestUri.ToString(), ExceptionMessage = ee.ToString(), PostBody = post });
                    //return null;
                    throw new WebException($"Request = {request}, URL = {request.RequestUri.ToString()}, ExceptionMessage = {ee.ToString()}");
                }

            }
            catch (WebException e)
            {
                throw e;
            }
        }



        #endregion
    }
}

