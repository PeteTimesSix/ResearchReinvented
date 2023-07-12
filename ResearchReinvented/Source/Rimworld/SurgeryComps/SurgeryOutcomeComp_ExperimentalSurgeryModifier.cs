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
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            if(recipe.IsAvailableOnlyForPrototyping())
                quality *= modifier;
        }

        public SurgeryOutcomeComp_ExperimentalSurgeryModifier()
        {
        }

        public float modifier;
    }
}
