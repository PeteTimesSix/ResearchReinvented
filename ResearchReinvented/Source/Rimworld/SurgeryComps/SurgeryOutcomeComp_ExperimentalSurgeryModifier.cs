using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.SurgeryComps
{
    public class SurgeryOutcomeComp_ExperimentalSurgeryModifier : SurgeryOutcomeComp
    {
        public float modifier;

        public SurgeryOutcomeComp_ExperimentalSurgeryModifier()
        {
        }
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            if (recipe.IsAvailableOnlyForPrototyping())
            {
                //Log.Message($"RR: reducing surgery {bill.Label} from {quality} to {quality * modifier}");
                quality *= modifier;
            }
        }
    }
}
