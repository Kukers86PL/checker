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
using System.Net;

namespace checker
{
    class pingChecker : ICheck
    {
        private String host         = "";
        private String label        = "";
        private long   responseTime = -1;

        public String getConfigString()
        {
            return "ping";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 3)
            {
                label  = subs[1];
                host = subs[2];
                return true;
            }
            return false;
        }

        public Boolean check()
        {
            IPAddress ip;
            if (!IPAddress.TryParse(host, out ip))
            {
                IPHostEntry entry = Dns.GetHostEntry(host);
                if (entry.AddressList.Length > 0)
                {
                    ip = entry.AddressList[0];
                }
            }
            
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(ip);
            if (reply.Status == IPStatus.Success)
            {
                responseTime = reply.RoundtripTime;
                return true;
            }
            responseTime = -1;
            return false;
        }

        public String getLabel()
        {
            return label + "\n" + responseTime.ToString() + " ms";
        }

        public String getLog(Char Separator)
        {
            return label + Separator + host + Separator + responseTime.ToString() + Separator;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
