using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class InteractionDefOf_Custom
    {
        public static InteractionDef RR_ScienceInterrogation;
        public static InteractionDef RR_ScienceInterrogationFinalize;

        static InteractionDefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InteractionDefOf_Custom));
        }
    }
}
