using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Data;
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
using static UnityEngine.UI.GridLayoutGroup;

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
            if (observer.RaceProps == null || !observer.RaceProps.Humanlike || observer.Faction != Faction.OfPlayer || observer.skills == null || !observer.Awake() || observer.WorkTypeIsDisabled(WorkTypeDefOf.Research))
                return;

            var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Medicine, medicine);

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
