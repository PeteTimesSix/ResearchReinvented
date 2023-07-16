using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    public static class BackCompatibility_BackCompatibleDefName_Patches
    {
        [HarmonyPostfix]
        public static void BackCompatibility_BackCompatibleDefName_Postfix(ref string __result, Type defType, string defName)
        {
            if (defType == typeof(ResearchOpportunityCategoryDef))
            {
                if (defName == "Medical")
                {
                    __result = "Clinical";
                }
            }
        }
    }
}
