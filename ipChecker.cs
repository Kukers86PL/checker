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
    class ipChecker : ICheck
    {
        private String label  = "";
        private String ip = "";
        private String ipURL = "https://v4.ipv6-test.com/api/myip.php";
        private int forceCheck = 0;
        private Boolean match = false;

        public String getConfigString()
        {
            return "ip";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 3)
            {
                label  = subs[1];
                ip = subs[2];
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
            String myIPv4 = sendAndReceive(ipURL);

            match = (myIPv4 == ip);

            return match;
        }

        public String getLabel()
        {
            return label + "\nIP";
        }

        public String getLog(Char Separator)
        {
            return label + Separator + ip + Separator + (match ? "1" : "0") + Separator;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
