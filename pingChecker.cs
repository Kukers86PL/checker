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
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(host);
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
