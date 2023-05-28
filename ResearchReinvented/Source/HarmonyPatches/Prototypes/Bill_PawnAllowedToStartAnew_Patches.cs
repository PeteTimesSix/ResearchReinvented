using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Extensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew))]
    public static class Bill_PawnAllowedToStartAnew_Patches
    {
        [HarmonyPostfix]
        public static bool Postfix(bool __result, Bill __instance, Pawn p) 
        {
            if (__result == false)
                return __result;

            if (__instance.recipe.IsAvailableOnlyForPrototyping()) 
            {
                if (p.WorkTypeIsDisabled(WorkTypeDefOf.Research))
                {
                    JobFailReason.Is(StringsCache.JobFail_IncapableOfResearch, null);
                    return false;
                }
                if (!p.workSettings?.WorkIsActive(WorkTypeDefOf.Research) ?? true)
                {
                    JobFailReason.Is("NotAssignedToWorkType".Translate(WorkTypeDefOf.Research.gerundLabel).CapitalizeFirst(), null);
                    return false;
                }
            }

            return __result;
        }
    }
}
