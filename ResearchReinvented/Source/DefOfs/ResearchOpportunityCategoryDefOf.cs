using PeteTimesSix.ResearchReinvented.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class ResearchOpportunityCategoryDefOf
    {
        public static ResearchOpportunityCategoryDef Prototyping;

        static ResearchOpportunityCategoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchOpportunityCategoryDefOf));
        }
    }
}
