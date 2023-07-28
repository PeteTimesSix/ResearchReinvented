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
using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Utilities;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Ingestibles
{
    [HarmonyPatch(typeof(Thing), "PrePostIngested")]
    public static class Thing_PrePostIngested_Patches
    {

        [HarmonyPostfix]
        public static void Thing_PrePostIngested_Postfix(Thing __instance, Pawn ingester)
        {
            var ingestible = __instance;

            if (!ingester.CanNowDoResearch())
                return;
            
            var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_OnIngest, ingestible);
            if (opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {ingester.LabelCap} ingested {ingestible.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                var amount = BaseResearchAmounts.OnIngestIngester;
                var modifier = ingester.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = ResearchXPAmounts.OnIngestIngester;
                opportunity.ResearchChunkPerformed(ingester, HandlingMode.Special_OnIngest, amount, modifier, xp, moteSubjectName: ingestible.LabelCapNoCount);
            }
        }
    }
}
