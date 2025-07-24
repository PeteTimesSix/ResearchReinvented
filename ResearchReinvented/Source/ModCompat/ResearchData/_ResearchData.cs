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
        public static bool success = true;

        static ResearchData()
        {
            active = ModLister.GetActiveModWithIdentifier("kongkim.ResearchData") != null;

            getter_ConsumptionRatePerTick = AccessTools.MethodDelegate<ConsumptionRatePerTickGetter>(AccessTools.PropertyGetter(typeof(CompRefuelable), "ConsumptionRatePerTick"));
        }

        public static void PatchDelayed(Harmony harmony)
        {
            try
            {
                //this checks for fuel before starting a research job
                var workGiverHasJobOnThingPrefix = AccessTools.Method(AccessTools.TypeByName("ResearchData.ResearchData_WorkGiver_Researcher_HasJobOnThing_Patch"), "Prefix");
                harmony.Patch(AccessTools.Method(typeof(WorkGiver_ResearcherRR), nameof(WorkGiver_ResearcherRR.HasJobOnThing)), prefix: workGiverHasJobOnThingPrefix);
                
                //Gone. Reduced to atoms. GPT'd into the void.
                //var workGiverJobOnThingPrefix = new HarmonyMethod(AccessTools.TypeByName("ResearchData.WorkGiver_Researcher_JobOnThing_Patch").GetMethod("Prefix"));
                //harmony.Patch(AccessTools.Method(typeof(WorkGiver_ResearcherRR), nameof(WorkGiver_ResearcherRR.JobOnThing)), prefix: workGiverJobOnThingPrefix);
            }
            catch (Exception e)
            {
                Log.Warning("RR: Failed to apply Research Data compatibility patch (critical: fuel consuption on research): " + e.ToString());
                success = false;
            }
        }

        public static void ConsumeTickIntervalFuel(this CompRefuelable comp, int delta) 
        {
            comp.ConsumeFuel(getter_ConsumptionRatePerTick(comp) * delta);
        }
    }
}
