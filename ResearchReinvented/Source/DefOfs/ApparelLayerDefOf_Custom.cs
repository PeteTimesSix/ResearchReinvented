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
    public static class ApparelLayerDefOf_Custom
    {
        public static ApparelLayerDef Satchel;

        static ApparelLayerDefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ApparelLayerDefOf_Custom));
        }
    }
}
