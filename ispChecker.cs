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
    class ispChecker : ICheck
    {
        private String label  = "";
        private String isp = "";
        private String ipURL = "https://v4.ipv6-test.com/api/myip.php";
        private String ipInfoURL = "https://ipinfo.io";
        private String hostToCheck = "8.8.8.8";
        private int forceCheck = 0;
        private Boolean match = false;
        private String cachedIP = "";
        private Boolean checkedIP = false;

        public String getConfigString()
        {
            return "isp";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 3)
            {
                label  = subs[1];
                isp = subs[2];
                return true;
            }
            return false;
        }

        private String sendAndReceive(String Url)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(Url),
                    Method = HttpMethod.Get,
                };

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;

                    // by calling .Result you are synchronously reading the result
                    string responseString = responseContent.ReadAsStringAsync().Result;

                    return responseString;
                }
            }
            return "";
        }

        public Boolean check()
        {
            if (checkedIP == false)
            {
                String myIPv4 = sendAndReceive(ipURL);
                if (cachedIP != myIPv4)
                {
                    String myInoIPv4 = sendAndReceive(ipInfoURL + "/" + myIPv4).ToLower();
                    if (myInoIPv4.Length > 0)
                    {
                        match = myInoIPv4.Contains(isp.ToLower());
                        cachedIP = myIPv4;
                        checkedIP = true;
                    }
                }
            }

            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(hostToCheck);
            if (reply.Status != IPStatus.Success)
            {
                checkedIP = false;
            }

            return match;
        }

        public String getLabel()
        {
            return label + "\nISP";
        }

        public String getLog(Char Separator)
        {
            return label + Separator + isp + Separator + (match ? "1" : "0") + Separator;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
