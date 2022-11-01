using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class ThingChecks
    {

        public static bool FactionAllowsAnalysis(this Thing thing)
        {
            if (thing.Faction == Faction.OfPlayer)
                return true;

            if (thing is Pawn pawn) 
            {
                if ((pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony) && !pawn.HostileTo(Faction.OfPlayer))
                    return true;
                else
                    return false;
            }
            else 
            {
                return thing.Faction == null;
            }
        }


        public static Thing UnwrapIfWrapped(this Thing outerThing)
		{
            if (outerThing is MinifiedThing minified)
            {
                return minified.InnerThing;
            }
            else if (outerThing is Corpse corpse)
            {
                return corpse.InnerPawn;
            }
            return outerThing;
        }
	}
}
