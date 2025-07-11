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

    [HarmonyPatch(typeof(SurgeryOutcomeComp_MedicineQuality), "XGetter")]
    public static class SurgeryOutcomeComp_MedicineQuality_Patches
    {
        [HarmonyPrefix]
        public static void Recipe_Surgery_CheckSurgeryFail_Prefix(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
        {
            if (bill is Bill_Medical medicalBill && medicalBill.consumedMedicine != null)
            {
                foreach ((ThingDef medicine, int count) in medicalBill.consumedMedicine)
                {
                    if(count > 0)
                    {
                        TendUtility_Patches.DoForObserver(surgeon, medicine, 0.5f/*avoid overlap with patient's mote*/);
                        TendUtility_Patches.DoForObserver(patient, medicine);
                    }
                }
            }
            else
            {
                foreach (Thing ingredient in ingredients)
                {
                    if (ingredient.def.IsMedicine)
                    {
                        TendUtility_Patches.DoForObserver(surgeon, ingredient.def, 1.0f/*avoid overlap with patient's mote*/);
                        TendUtility_Patches.DoForObserver(patient, ingredient.def);
                        return;
                    }
                }
            }
        }
    }
}
