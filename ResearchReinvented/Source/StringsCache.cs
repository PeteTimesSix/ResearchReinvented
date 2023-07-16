using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented
{
    [StaticConstructorOnStartup]
    public static class StringsCache
    {
        public static string JobFail_IncapableOfResearch;
        public static string JobFail_NeedResearchKit;
        public static string JobFail_NeedResearchBench;
        public static string JobFail_IsPrototype;

        static StringsCache()
        {
            JobFail_IncapableOfResearch = "RR_jobFail_incapableOfResearch".Translate();
            JobFail_NeedResearchKit = "RR_jobFail_needResearchKit".Translate();
            JobFail_NeedResearchBench = "RR_jobFail_needResearchBench".Translate();
            JobFail_IsPrototype = "RR_jobFail_isPrototype".Translate();
        }
    }
}
