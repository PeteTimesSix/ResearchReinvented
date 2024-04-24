using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Utilities;
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
            if (medicine == null || !observer.CanNowDoResearch())
                return;

            var currentProject = Find.ResearchManager.GetProject();
            if (currentProject == null)
                return;

            var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfProject(currentProject, OpportunityAvailability.Available, HandlingMode.Special_Medicine, medicine).FirstOrDefault();

            if (opportunity != null)
            {
                var amount = BaseResearchAmounts.OnTendObserver;
                var modifier = observer.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = ResearchXPAmounts.OnTendObserver;
                opportunity.ResearchChunkPerformed(observer, HandlingMode.Special_Medicine, amount, modifier, xp, moteSubjectName: medicine.LabelCap);
            }
        }
    }
}
