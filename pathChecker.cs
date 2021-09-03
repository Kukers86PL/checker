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

namespace checker
{
    class pathChecker : ICheck
    {
        private String path  = "";
        private String label = "";
        private Boolean exists = false;
        private String STATUS_FILE = "seed.txt";
        private String seed = "";

        public String getConfigString()
        {
            return "path";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 4)
            {
                label  = subs[1];
                path = subs[2];
                seed = subs[3];
                return true;
            }
            return false;
        }

        public Boolean check()
        {
            System.IO.StreamReader read_file = new System.IO.StreamReader(path + "\\" + STATUS_FILE);
            String line = read_file.ReadLine();
            read_file.Close();

            if (line == seed)
            {
                exists = true;
                return true;
            }

            exists = false;
            return false;
        }

        public String getLabel()
        {
            return label + "\n" + (exists ? "OK" : "NOK");
        }

        public String getLog(Char Separator)
        {
            return label + Separator + path + Separator + (exists ? "1" : "0") + Separator;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
