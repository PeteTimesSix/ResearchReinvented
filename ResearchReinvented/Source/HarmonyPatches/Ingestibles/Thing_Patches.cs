using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Ingestibles
{
    [HarmonyPatch(typeof(Thing), "PrePostIngested")]
    public static class Thing_PrePostIngested_Patches
    {
        private static IEnumerable<ResearchOpportunity> MatchingOpportunities =>
            ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities()
            .Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Special_OnIngest));

        [HarmonyPostfix]
        public static void Thing_PrePostIngested_Postfix(Thing __instance, Pawn ingester)
        {
            if (!ingester.RaceProps.Humanlike || ingester.Faction != Faction.OfPlayer || ingester.skills == null || !ingester.Awake() || StatDefOf.ResearchSpeed.Worker.IsDisabledFor(ingester))
                return;

            //Log.Message($"pawn {ingester.LabelCap} ingested {__instance.LabelCap}, checking opportunities (count: {MatchingOpportunities.Count()})");
            foreach(var opportunity in MatchingOpportunities)
            {
                if (opportunity.requirement.MetBy(__instance))
                {
                    opportunity.ResearchChunkPerformed(ingester, __instance.LabelCapNoCount, HandlingMode.Special_OnIngest);
                }
            }
        }
    }
}
