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

            importanceMultiplier = changes.importanceMultiplier.HasValue ? changes.importanceMultiplier.Value : preset.importanceMultiplier;
            importanceMultiplierCounted = changes.importanceMultiplierCounted.HasValue ? changes.importanceMultiplierCounted.Value : preset.importanceMultiplierCounted;
            infiniteOverflow = changes.infiniteOverflow.HasValue ? changes.infiniteOverflow.Value : preset.infiniteOverflow;
            targetIterations = changes.targetIterations.HasValue ? changes.targetIterations.Value : preset.targetIterations;
            researchSpeedMultiplier = changes.researchSpeedMultiplier.HasValue ? changes.researchSpeedMultiplier.Value : preset.researchSpeedMultiplier;

            availableAtOverallProgress = changes.availableAtOverallProgress.HasValue ? changes.availableAtOverallProgress.Value : preset.availableAtOverallProgress;
        }
    }
}
