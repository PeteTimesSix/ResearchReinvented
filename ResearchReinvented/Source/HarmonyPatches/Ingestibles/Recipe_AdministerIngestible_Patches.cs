using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Ingestibles
{
    [HarmonyPatch(typeof(Recipe_AdministerIngestible), nameof(Recipe_AdministerIngestible.ApplyOnPawn))]
    public static class Recipe_AdministerIngestible_Patches
    {
        private static IEnumerable<ResearchOpportunity> MatchingOpportunities =>
            ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
            .Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Special_OnIngest_Observable));

        [HarmonyPrefix]
        public static void Recipe_AdministerIngestible_ApplyOnPawn_Prefix(Recipe_AdministerIngestible __instance, Pawn pawn, Pawn billDoer, List<Thing> ingredients)
        {
            var ingestible = ingredients[0];
            var ingester = pawn;
            var observer = billDoer;
            if (!observer.RaceProps.Humanlike || observer.Faction != Faction.OfPlayer || observer.skills == null || StatDefOf.ResearchSpeed.Worker.IsDisabledFor(observer))
                return;

            //Log.Message($"pawn {ingester.LabelCap} observed pawn {ingester.LabelCap} ingest {ingestible.LabelCap}, checking opportunities (count: {MatchingOpportunities.Count()})");
            foreach (var opportunity in MatchingOpportunities)
            {
                if (opportunity.requirement.MetBy(ingestible))
                {
                    opportunity.ResearchChunkPerformed(observer, ingestible.LabelCapNoCount, HandlingMode.Special_OnIngest_Observable, 0.5f/*avoid overlap with ingester's mote*/);
                }
            }
        }
    }
}
