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
    public static class EffecterDefOf_Custom
    {
        public static EffecterDef NoResearchKitEffect;

        static EffecterDefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EffecterDefOf_Custom));
        }
    }
}
