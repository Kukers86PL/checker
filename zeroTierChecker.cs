using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;

namespace checker
{
    class zeroTierChecker : ICheck
    {
        private String label  = "";
        private String authToken = "";
        private String networkId = "";
        private String nodeId = "";
        private string URL = "https://my.zerotier.com";
        private Boolean online = false;

        public String getConfigString()
        {
            return "zerotier";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 5)
            {
                label  = subs[1];
                authToken = subs[2];
                networkId = subs[3];
                nodeId = subs[4];
                return true;
            }
            return false;
        }

        public Boolean check()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(URL + "/api/network/" + networkId + "/member/" + nodeId),
                    Method = HttpMethod.Get,
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authToken);

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;

                    // by calling .Result you are synchronously reading the result
                    string responseString = responseContent.ReadAsStringAsync().Result;

                    if (responseString.Contains(",\"online\":true,"))
                    {
                        online = true;
                        return true;
                    }
                }
            }

            online = false;
            return false;
        }

        public String getLabel()
        {
            return label;
        }

        public String getLog(Char Separator)
        {
            return "";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
