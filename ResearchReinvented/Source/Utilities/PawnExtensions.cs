using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class PawnExtensions
    {

        public static bool CanEverDoResearch(this Pawn pawn)
        {
            return
                pawn != null &&
                pawn.skills != null &&
                !pawn.WorkTypeIsDisabled(WorkTypeDefOf.Research);
        }

        public static bool CanNowDoResearch(this Pawn pawn, bool checkFaction = true)
        {
            return
                pawn.CanEverDoResearch() &&
                pawn.Awake() &&
                (!checkFaction || pawn.Faction == Faction.OfPlayer);
        }
    }
}
