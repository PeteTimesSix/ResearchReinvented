using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.ModCompat
{
    [StaticConstructorOnStartup]
    public static class ResearchData
    {
        public delegate float ConsumptionRatePerTickGetter(CompRefuelable instance);

        public static ConsumptionRatePerTickGetter getter_ConsumptionRatePerTick;

        public enum ResearchDataCompatMode
        {
            TheoryOnly,
            AllBenchResearch
        }

        public static bool active = false;

        static ResearchData()
        {
            active = ModLister.GetActiveModWithIdentifier("kongkim.ResearchData") != null;

            getter_ConsumptionRatePerTick = AccessTools.MethodDelegate<ConsumptionRatePerTickGetter>(AccessTools.PropertyGetter(typeof(CompRefuelable), "ConsumptionRatePerTick"));
        }

        public static void PatchDelayed(Harmony harmony)
        {
            //these patches generate refuel jobs
            var workGiverHasJobOnThingPrefix = new HarmonyMethod(AccessTools.TypeByName("ResearchData.WorkGiver_Researcher_HasJobOnThing_Patch").GetMethod("Prefix"));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_ResearcherRR), nameof(WorkGiver_ResearcherRR.HasJobOnThing)), prefix: workGiverHasJobOnThingPrefix);
            var workGiverJobOnThingPrefix = new HarmonyMethod(AccessTools.TypeByName("ResearchData.WorkGiver_Researcher_JobOnThing_Patch").GetMethod("Prefix"));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_ResearcherRR), nameof(WorkGiver_ResearcherRR.JobOnThing)), prefix: workGiverJobOnThingPrefix);
        }

        public static void ConsumeTickFuel(this CompRefuelable comp) 
        {
            comp.ConsumeFuel(getter_ConsumptionRatePerTick(comp));
        }
    }
}
