using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefGenerators;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(TerrainDefGenerator_Carpet), nameof(TerrainDefGenerator_Carpet.CarpetFromBlueprint))]
    public static class TerrainDefGenerator_Carpet_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(TerrainDef __result, TerrainTemplateDef tp)
        {
            if(!AlternateResearchSubjectDefGenerator.carpets.ContainsKey(tp))
                AlternateResearchSubjectDefGenerator.carpets[tp] = new List<TerrainDef>();
            AlternateResearchSubjectDefGenerator.carpets[tp].Add(__result);
        }
    }
}
