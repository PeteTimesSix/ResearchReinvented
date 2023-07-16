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

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Blueprint), nameof(Blueprint.TryReplaceWithSolidThing))]
    public static class Blueprint_TryReplaceWithSolidThing_Patches
    {
        [HarmonyPostfix] public static void Postfix(Blueprint __instance, Pawn workerPawn, ref Thing createdThing, ref bool jobEnded) 
        {
            //Log.Message($"intercepting {__instance.LabelCap} turning into {createdThing.LabelCap} (involved pawn: {workerPawn.LabelCap})");
            if(createdThing is Frame frame)
            {
                var buildable = frame.def.entityDefToBuild;
                if(buildable.IsAvailableOnlyForPrototyping(true))
                {
                    PrototypeKeeper.Instance.MarkAsPrototype(createdThing);
                }
            }
        }
    }
}
