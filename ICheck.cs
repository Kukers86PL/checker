using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    interface ICheck : ICloneable
    {
        String getConfigString();

        Boolean pasreConfig(Char Separator, String ConfigText);

        Boolean check();

        String getLabel();

        String getLog(Char Separator);

        object Clone();
    }
}
