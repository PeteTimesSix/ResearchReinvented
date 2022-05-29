using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefOfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    /*[HarmonyPatch(typeof(ApparelLayerDef), nameof(ApparelLayerDef.IsUtilityLayer), MethodType.Getter)]
    public static class ApparelLayerDef_IsUtilityLayer_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ApparelLayerDef __instance, ref bool __result)
        {
            if (__result == true)
                return;

            //if (__instance == ApparelLayerDefOf_Custom.Satchel)
            //    __result = true;

            return;
        }
    }*/
}
