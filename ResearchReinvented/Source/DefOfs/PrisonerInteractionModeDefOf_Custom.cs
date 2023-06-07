using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class PrisonerInteractionModeDefOf_Custom
    {
        public static PrisonerInteractionModeDef RR_ScienceInterrogation;


        static PrisonerInteractionModeDefOf_Custom()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PrisonerInteractionModeDefOf_Custom));
        }
    }
}
