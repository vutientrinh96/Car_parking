using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace Do_an_tot_nghiep
{
    class GlobalVars
    {
        private static string serialData;
        public static string SerialData
        {
            get { return GlobalVars.serialData; }
            set { GlobalVars.serialData = value; }
        }
    }
}
