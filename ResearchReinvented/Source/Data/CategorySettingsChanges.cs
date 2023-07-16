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

        public float? importanceMultiplier;
        public float? importanceMultiplierCounted;
        public bool? infiniteOverflow;
        public float? targetIterations;
        public float? researchSpeedMultiplier;

        public FloatRange? availableAtOverallProgress;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref category, "category");

            Scribe_Values.Look(ref enabled, "enabled", null);

            Scribe_Values.Look(ref importanceMultiplier, "importanceMultiplier", null);
            Scribe_Values.Look(ref importanceMultiplierCounted, "importanceMultiplierCounted", null);
            Scribe_Values.Look(ref infiniteOverflow, "infiniteOverflow", null);
            Scribe_Values.Look(ref targetIterations, "targetIterations", null);
            Scribe_Values.Look(ref researchSpeedMultiplier, "researchSpeedMultiplier", null);

            Scribe_Values.Look(ref availableAtOverallProgress, "availableAtOverallProgress", null);
        }

        public void UpdateChanges(CategorySettingsPreset defaults, CategorySettingsFinal finals)
        {
            //set only if different, but dont un-set if not
            //this way, once a user sets a setting the value will be preserved
            if (enabled.HasValue || finals.enabled != defaults.enabled)
                enabled = finals.enabled;

            if (importanceMultiplier.HasValue || finals.importanceMultiplier != defaults.importanceMultiplier)
                importanceMultiplier = finals.importanceMultiplier;
            if (importanceMultiplierCounted.HasValue || finals.importanceMultiplierCounted != defaults.importanceMultiplierCounted)
                importanceMultiplierCounted = finals.importanceMultiplierCounted;
            if (infiniteOverflow.HasValue || finals.infiniteOverflow != defaults.infiniteOverflow)
                infiniteOverflow = finals.infiniteOverflow;
            if (targetIterations.HasValue || finals.targetIterations != defaults.targetIterations)
                targetIterations = finals.targetIterations;
            if (researchSpeedMultiplier.HasValue || finals.researchSpeedMultiplier != defaults.researchSpeedMultiplier)
                researchSpeedMultiplier = finals.researchSpeedMultiplier;

            if (enabled.HasValue || finals.availableAtOverallProgress != defaults.availableAtOverallProgress)
                availableAtOverallProgress = finals.availableAtOverallProgress;
        }
    }
}
