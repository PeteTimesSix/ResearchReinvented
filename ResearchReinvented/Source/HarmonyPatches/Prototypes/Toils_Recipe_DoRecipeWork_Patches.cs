using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
using PeteTimesSix.ResearchReinvented.Utilities;
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
        public const int tickModulo = 20;

        [HarmonyPostfix]
        public static void Toils_Recipe_DoRecipeWork_Postfix(Toil __result)
        {
            __result.AddPreTickAction(() =>
            {
                var toil = __result;
                var actor = toil.actor;
                if (!actor.IsHashIntervalTick(tickModulo))
                    return;

                var bench = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                if (bench != null)
                {
                    if (!actor.CanEverDoResearch())
                        return;

                    var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfProject(Find.ResearchManager.GetProject(), OpportunityAvailability.Available, HandlingMode.Special_Tooling, bench).FirstOrDefault();

                    if (opportunity != null)
                    {
                        float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true) * 0.00825f;
                        num *= FieldResearchHelper.GetFieldResearchSpeedFactor(actor, opportunity.project);
                        opportunity.ResearchTickPerformed(num, actor, tickModulo);
                    }
                }
            });

            /*Action initAction = null;

            initAction = () =>
            {
                //find tooling opportunity and do research
                var toil = __result;
                var actor = toil.actor;
                var bench = toil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                if (bench != null)
                {
                    var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available | OpportunityAvailability.ResearchTooLow, HandlingMode.Special_Tooling, bench);
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Tooling) && o.requirement.MetBy(bench))
                    //.FirstOrDefault();
                    if (opportunity != null)
                    {
                        if (ResearchReinvented_Debug.debugPrintouts)
                            Log.Message($"pawn {actor.LabelCap} started work on {bench.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                        __result.AddPreTickAction(() =>
                        {
                            float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
                            num *= FieldResearchHelper.GetFieldResearchSpeedFactor(actor, opportunity.project);
                            opportunity.ResearchTickPerformed(num, actor);
                        });
                    }
                    __result.tickAction -= initAction;
                }
            };
            __result.tickAction += initAction;*/
        }
    }
}
