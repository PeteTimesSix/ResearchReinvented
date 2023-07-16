using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
   [HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.DoRecipeWork))]
    public static class Toils_Recipe_DoRecipeWork_Patches
    {
        [HarmonyPostfix]
        public static void Toils_Recipe_DoRecipeWork_Postfix(Toil __result)
        {
            __result.tickAction += () =>
            {
                //find tooling opportunity and do research
                var toil = __result;
                var actor = toil.actor;
                var bench = toil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                if (bench != null)
                {
                    var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunitiesFiltered(true, HandlingMode.Special_Tooling, bench)
                        //.GetCurrentlyAvailableOpportunities()
                        //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Tooling) && o.requirement.MetBy(bench))
                        .FirstOrDefault();
                    if(opportunity != null)
                    {
                        float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
                        num *= FieldResearchHelper.GetFieldResearchSpeedFactor(actor, opportunity.project);
                        opportunity.ResearchTickPerformed(num, actor, null);
                    }
                }
            };
        }
    }
}
