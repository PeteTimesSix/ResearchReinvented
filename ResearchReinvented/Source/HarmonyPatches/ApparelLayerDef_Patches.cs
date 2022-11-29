using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(ApparelLayerDef), nameof(ApparelLayerDef.IsUtilityLayer), MethodType.Getter)]
    public static class ApparelLayerDef_IsUtilityLayer_Patches
    {
        [HarmonyPostfix]
        public static bool ApparelLayerDef_IsUtilityLayer_Postfix(bool __result, ApparelLayerDef __instance) 
        {
            if (__result == true)
                return __result;
            else
            {
                return __instance == ApparelLayerDefOf_Custom.Satchel;
            }
        }
    }
}
