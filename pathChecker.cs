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
    class pathChecker : ICheck
    {
        private String path  = "";
        private String label = "";
        private Boolean exists = false;
        private String STATUS_FILE = "status.txt";

        public String getConfigString()
        {
            return "path";
        }

        public Boolean pasreConfig(Char Separator, String ConfigText)
        {
            String[] subs = ConfigText.Split(Separator);
            if (subs.Length == 3)
            {
                label  = subs[1];
                path = subs[2];
                return true;
            }
            return false;
        }

        public Boolean check()
        {
            Random rnd = new Random();
            int rand = rnd.Next();

            StreamWriter write_file = new StreamWriter(path + "\\" + STATUS_FILE, false);
            write_file.WriteLine(rand.ToString());
            write_file.Close();

            System.IO.StreamReader read_file = new System.IO.StreamReader(path + "\\" + STATUS_FILE);
            String line = read_file.ReadLine();
            read_file.Close();

            if (line == rand.ToString())
            {
                exists = true;
                return true;
            }

            exists = false;
            return false;
        }

        public String getLabel()
        {
            return label;
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
