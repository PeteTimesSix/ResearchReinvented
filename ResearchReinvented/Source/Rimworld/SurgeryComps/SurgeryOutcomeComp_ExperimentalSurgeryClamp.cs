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
    public class SurgeryOutcomeComp_ExperimentalSurgeryClamp : SurgeryOutcomeComp_ClampToRange
    {
        public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
        {
            if(recipe.IsAvailableOnlyForPrototyping())
            {
                //Log.Message($"RR: clamping surgery {bill.Label} of {quality} to {this.range.ClampToRange(quality)} ({this.range.min} - {this.range.max})");
                quality = this.range.ClampToRange(quality);
            }
        }

        public SurgeryOutcomeComp_ExperimentalSurgeryClamp()
        {
        }
    }
}
