using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.ModCompat
{
    [StaticConstructorOnStartup]
    public static class HumanoidAlienRaces
    {
        public static bool active = false;

        static HumanoidAlienRaces()
        {
            active = ModLister.GetActiveModWithIdentifier("erdelf.HumanoidAlienRaces") != null;
        }

        public static void PatchDelayed(Harmony harmony)
        {
            var raceCheck = new HarmonyMethod(AccessTools.TypeByName("AlienRace.HarmonyPatches").GetMethod("ShouldSkipResearchPostfix"));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_Analyse), nameof(WorkGiver_Analyse.ShouldSkip)), postfix: raceCheck);
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_AnalyseInPlace), nameof(WorkGiver_AnalyseInPlace.ShouldSkip)), postfix: raceCheck);
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_AnalyseTerrain), nameof(WorkGiver_AnalyseTerrain.ShouldSkip)), postfix: raceCheck);
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_ResearcherRR), nameof(WorkGiver_ResearcherRR.ShouldSkip)), postfix: raceCheck);
        }
    }
}
