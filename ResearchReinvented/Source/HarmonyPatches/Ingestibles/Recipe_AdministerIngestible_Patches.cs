using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Data;
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

        [HarmonyPrefix]
        public static void Recipe_AdministerIngestible_ApplyOnPawn_Prefix(Recipe_AdministerIngestible __instance, Pawn pawn, Pawn billDoer, List<Thing> ingredients)
        {
            if (ingredients == null || ingredients.Count == 0)
                return;

            var ingestible = ingredients[0];
            var ingester = pawn;
            var observer = billDoer;

            if (observer.RaceProps == null || !observer.RaceProps.Humanlike || observer.Faction != Faction.OfPlayer || observer.skills == null || observer.WorkTypeIsDisabled(WorkTypeDefOf.Research))
                return;

            var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_OnIngest_Observable, ingestible);

            if(opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {observer.LabelCap} observed pawn {ingester.LabelCap} ingest {ingestible.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                var amount = BaseResearchAmounts.AdministerIngestibleObserver;
                var modifier = ingester.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = ResearchXPAmounts.AdministerIngestibleObserver;
                opportunity.ResearchChunkPerformed(observer, HandlingMode.Special_OnIngest_Observable, amount, modifier, xp, moteSubjectName: ingestible.LabelCapNoCount, moteOffsetHint: 0.5f/*avoid overlap with ingester's mote*/);
            }
        }
    }
}
