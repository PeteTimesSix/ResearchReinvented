using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Medicine
{
    [HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
    public static class TendUtility_Patches
    {
        private static IEnumerable<ResearchOpportunity> MatchingOpportunities =>
            ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities()
            .Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Special_Medicine));

        [HarmonyPrefix]
        public static void TendUtility_DoTend_Prefix(Pawn doctor, Pawn patient, RimWorld.Medicine medicine)
        {
            if(medicine == null) 
                return;

            DoForObserver(doctor, medicine.def, 0.5f/*avoid overlap with patient's mote*/);
            DoForObserver(patient, medicine.def);
        }

        public static void DoForObserver(Pawn observer, ThingDef medicine, float offsetHint = 0f) 
        {
            if (!observer.RaceProps.Humanlike || observer.Faction != Faction.OfPlayer || observer.skills == null || !observer.Awake() || StatDefOf.ResearchSpeed.Worker.IsDisabledFor(observer))
                return;

            foreach (var opportunity in MatchingOpportunities)
            {
                if (opportunity.requirement.MetBy(medicine))
                {
                    opportunity.ResearchChunkPerformed(observer, medicine.LabelCap, HandlingMode.Special_Medicine, offsetHint);
                }
            }
        }
    }
}
