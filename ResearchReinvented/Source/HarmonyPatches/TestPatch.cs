using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    /*class TestPatch
    {
        [HarmonyPatch(typeof(ThingDefGenerator_Meat), nameof(ThingDefGenerator_Meat.ImpliedMeatDefs))]
        public static class ThingDefGenerator_Meat_ThingDefGenerator_Meat_Postfix
        {
            [HarmonyPostfix]
            static void Postfix(ref IEnumerable<ThingDef> __result)
            {
                Log.Message("test1" + "\n");
                foreach (ThingDef meatDef in __result)
                {
                    Log.Message("match for " + meatDef.defName + "\n");
                }
                Log.Message("test2" + "\n");
            }
        }
    }*/
}
