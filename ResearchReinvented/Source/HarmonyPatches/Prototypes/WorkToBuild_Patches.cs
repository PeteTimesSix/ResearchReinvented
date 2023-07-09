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
    [HarmonyPatch(typeof(Frame), nameof(Frame.WorkToBuild), MethodType.Getter)]
    public static class Frame_WorkToBuild_Patches
    {
        [HarmonyPostfix]
        public static float Postfix(float __result, Frame __instance)
        {
            if (Current.ProgramState != ProgramState.Playing) //only modify during game
                return __result;

            if (PrototypeKeeper.Instance.IsPrototype(__instance))
            {
                return __result * PrototypeUtilities.PROTOTYPE_WORK_MULTIPLIER;
            }
            else 
            {
                return __result;
            }
        }
    }

    [HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.WorkAmountTotal))]
    public static class RecipeDef_WorkAmountTotal_Patches
    {
        [HarmonyPostfix]
        public static float Postfix(float __result, RecipeDef __instance)
        {
            if (Current.ProgramState != ProgramState.Playing) //only modify during game
                return __result;

            if (!__instance.IsAvailableOnlyForPrototyping())
                return __result;
            else
            {
                return __result * PrototypeUtilities.PROTOTYPE_WORK_MULTIPLIER;
            }
        }
    }
}
