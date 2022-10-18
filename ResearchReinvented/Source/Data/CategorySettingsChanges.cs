using PeteTimesSix.ResearchReinvented.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Data
{
    public class CategorySettingsChanges : IExposable
    {
        public ResearchOpportunityCategoryDef category;

        public bool? enabled;

        public float? targetFractionMultiplier;
        public float? targetIterations;
        public float? extraFractionMultiplier;
        public bool? infiniteOverflow;
        public float? researchSpeedMultiplier;

        public FloatRange? availableAtOverallProgress;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref category, "category");

            Scribe_Values.Look(ref enabled, "enabled", null);

            Scribe_Values.Look(ref targetFractionMultiplier, "targetFractionMultiplier", null);
            Scribe_Values.Look(ref targetIterations, "targetIterations", null);
            Scribe_Values.Look(ref extraFractionMultiplier, "extraFractionMultiplier", null);
            Scribe_Values.Look(ref infiniteOverflow, "infiniteOverflow", null);
            Scribe_Values.Look(ref researchSpeedMultiplier, "researchSpeedMultiplier", null);

            Scribe_Values.Look(ref availableAtOverallProgress, "availableAtOverallProgress", null);
        }

        public void UpdateChanges(CategorySettingsPreset defaults, CategorySettingsFinal finals)
        {
            //set only if different, but dont un-set if not
            //this way, once a user sets a setting the value will be preserved
            if (enabled.HasValue || finals.enabled != defaults.enabled)
                enabled = finals.enabled;

            if (targetFractionMultiplier.HasValue || finals.targetFractionMultiplier != defaults.targetFractionMultiplier)
                targetFractionMultiplier = finals.targetFractionMultiplier;
            if (targetIterations.HasValue || finals.targetIterations != defaults.targetIterations)
                targetIterations = finals.targetIterations;
            if (extraFractionMultiplier.HasValue || finals.extraFractionMultiplier != defaults.extraFractionMultiplier)
                extraFractionMultiplier = finals.extraFractionMultiplier;
            if (infiniteOverflow.HasValue || finals.infiniteOverflow != defaults.infiniteOverflow)
                infiniteOverflow = finals.infiniteOverflow;
            if (researchSpeedMultiplier.HasValue || finals.researchSpeedMultiplier != defaults.researchSpeedMultiplier)
                researchSpeedMultiplier = finals.researchSpeedMultiplier;

            if (enabled.HasValue || finals.availableAtOverallProgress != defaults.availableAtOverallProgress)
                availableAtOverallProgress = finals.availableAtOverallProgress;
        }
    }
}
