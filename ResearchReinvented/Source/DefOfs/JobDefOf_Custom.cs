using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class JobDefOf_Custom
    {
        public static JobDef RR_Analyse;
        public static JobDef RR_AnalyseInPlace;
        public static JobDef RR_AnalyseTerrain;
        public static JobDef RR_Research;


        static JobDefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Custom));
        }
    }
}
