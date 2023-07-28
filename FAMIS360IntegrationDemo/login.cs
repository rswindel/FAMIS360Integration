using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMIS360IntegrationDemo
{
    public class login
    { 
            public class loginobject
            {
                public string UserName { get; set; }
                public string Password { get; set; }

                public loginobject(string username, string password)
                { this.UserName = username; this.Password = password; }
            }
            public class loginresponse
            {

                public class Rootobject
                {
                    public Item Item { get; set; }
                    public bool Result { get; set; }
                    public int Context { get; set; }
                    public string Message { get; set; }

                    public Rootobject() { this.Item = new Item(); }
                }

                public class Item
                {
                    public string access_token { get; set; }
                    public string token_type { get; set; }
                    public int expires_in { get; set; }
                    public string refresh_token { get; set; }
                    public string user_id { get; set; }
                    public string first_name { get; set; }
                    public string last_name { get; set; }
                    public string installation_id { get; set; }
                    public string installation_name { get; set; }
                    [JsonProperty(".expires")]
                    public DateTime expires { get; set; }
                    [JsonProperty(".issued")]
                    public DateTime issued { get; set; }
                }

            }
            public class refreshobject
            {
                public string Refresh_Token { get; set; }

                public refreshobject(string token) { this.Refresh_Token = token; }
            }

            public login() { }

        }
    }

