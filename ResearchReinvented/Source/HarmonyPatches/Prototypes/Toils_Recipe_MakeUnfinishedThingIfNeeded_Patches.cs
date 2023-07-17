using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
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
    [HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.MakeUnfinishedThingIfNeeded))]
    public static class Toils_Recipe_MakeUnfinishedThingIfNeeded_Patches
    {
        [HarmonyPostfix]
        public static void Toils_Recipe_MakeUnfinishedThingIfNeeded(Toil __result)
        {
            __result.AddPreInitAction(() =>
            {
                var toil = __result;
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var recipeDef = curJob.RecipeDef;
                var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                if (unfinishedThing == null)
                    return;

                bool isPrototype = ((recipeDef != null && recipeDef.IsAvailableOnlyForPrototyping()) || (recipeDef.ProducedThingDef != null && recipeDef.ProducedThingDef.IsAvailableOnlyForPrototyping()));
                if (isPrototype)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"Marking {unfinishedThing} as prototype");
                    PrototypeKeeper.Instance.MarkAsPrototype(unfinishedThing);
                }
            });
        }
    }
}
