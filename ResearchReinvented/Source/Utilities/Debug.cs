using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented
{
    public static class Debug
    {
        public static bool debugPrintouts = true;

        public static void LogMessage(string str) 
        {
            if (debugPrintouts)
                Log.Message("[RR]:" + str);
        }

    }
}
