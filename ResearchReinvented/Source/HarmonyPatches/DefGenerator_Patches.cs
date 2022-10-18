using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefGenerators;
using PeteTimesSix.ResearchReinvented.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class DefGenerator_PreResolve_Patches
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            foreach (var alternateDef in AlternateResearchSubjectDefGenerator.AncientAlternateDefs()) 
            {
                DefGenerator.AddImpliedDef(alternateDef);
            }
        }
    }

    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
    public static class DefGenerator_PostResolve_Patches
    {
        [HarmonyPostfix]
        public static void Postfix()
        {

        }
    }
}
