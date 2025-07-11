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
        [HarmonyPostfix]
        public static void Toils_Recipe_DoRecipeWork_Postfix(Toil __result)
        {
            __result.AddPreTickIntervalAction((int delta) =>
            {
                var toil = __result;
                var actor = toil.actor;
                var curJob = actor.CurJob;

                var bench = curJob.GetTarget(TargetIndex.A).Thing;
                if (bench != null)
                {
                    if (!actor.CanEverDoResearch())
                        return;

                    var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Tooling, bench);

                    if (opportunity != null)
                    {
                        float workStatMult = 1.0f;
                        if(actor.skills != null)
                            workStatMult = ((curJob.RecipeDef.workSpeedStat == null) ? 1f : actor.GetStatValue(curJob.RecipeDef.workSpeedStat));
                        float num = workStatMult * actor.GetStatValue(StatDefOf.ResearchSpeed, true) * 0.00825f;
                        opportunity.ResearchTickPerformed(num, actor, delta);
                    }
                }
            });
        }
    }
}
