using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.Data
{
    public class CategorySettingsFinal : CategorySettingsPreset
    {
        public void Update(CategorySettingsPreset preset, CategorySettingsChanges changes)
        {
            enabled = changes.enabled.HasValue ? changes.enabled.Value : preset.enabled;

            targetFractionMultiplier = changes.targetFractionMultiplier.HasValue ? changes.targetFractionMultiplier.Value : preset.targetFractionMultiplier;
            targetIterations = changes.targetIterations.HasValue ? changes.targetIterations.Value : preset.targetIterations;
            extraFractionMultiplier = changes.extraFractionMultiplier.HasValue ? changes.extraFractionMultiplier.Value : preset.extraFractionMultiplier;
            infiniteOverflow = changes.infiniteOverflow.HasValue ? changes.infiniteOverflow.Value : preset.infiniteOverflow;
            researchSpeedMultiplier = changes.researchSpeedMultiplier.HasValue ? changes.researchSpeedMultiplier.Value : preset.researchSpeedMultiplier;

            availableAtOverallProgress = changes.availableAtOverallProgress.HasValue ? changes.availableAtOverallProgress.Value : preset.availableAtOverallProgress;
        }

        /*public void ApplyChanges(CategorySettingsChanges changes)
        {
            if(changes.enabled.HasValue)
                enabled = changes.enabled.Value;

            if (changes.targetFractionMultiplier.HasValue)
                targetFractionMultiplier = changes.targetFractionMultiplier.Value;
            if (changes.targetIterations.HasValue)
                targetIterations = changes.targetIterations.Value;
            if (changes.extraFractionMultiplier.HasValue)
                extraFractionMultiplier = changes.extraFractionMultiplier.Value;
            if (changes.infiniteOverflow.HasValue)
                infiniteOverflow = changes.infiniteOverflow.Value;
            if (changes.researchSpeedMultiplier.HasValue)
                researchSpeedMultiplier = changes.researchSpeedMultiplier.Value;

            if (changes.availableAtOverallProgress.HasValue)
                availableAtOverallProgress = changes.availableAtOverallProgress.Value;
        }*/
    }
}
