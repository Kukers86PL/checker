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
        private String name = "";
        private String ipURL = "https://v4.ipv6-test.com/api/myip.php";
        private String ipInfoURL = "https://ipinfo.io/";
        private int forceCheck = 0;
        private Boolean match = false;
        private int count = 0;
        private String cachedIp = "";

        public String getConfigString()
        {
            return "isp";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 4)
            {
                label  = subs[1];
                name = subs[2];
                forceCheck = Int32.Parse(subs[3]);
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
            count++;
            String myIPv4 = sendAndReceive(ipURL);

            if ((cachedIp != myIPv4) || (count >= forceCheck))
            {
                cachedIp = myIPv4;
                count = 0;

                String myIpv4Info = sendAndReceive(ipInfoURL + myIPv4).ToLower();

                match = myIpv4Info.Contains(name.ToLower());
            }

            return match;
        }

        public String getLabel()
        {
            return label + "\nISP";
        }

        public String getLog(Char Separator)
        {
            return label + Separator + (match ? "1" : "0") + Separator;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
