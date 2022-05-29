using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class StatDefOf_Custom
    {
        public static StatDef FieldResearchSpeedMultiplier;

        static StatDefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf_Custom));
        }
    }
}
