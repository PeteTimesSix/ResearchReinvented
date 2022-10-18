using PeteTimesSix.ResearchReinvented.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
    public class SettingsPresetDef : Def
    {
        public int priority = 0;

        public List<CategorySettingsPreset> categorySettingPresets = new List<CategorySettingsPreset>();

        public CategorySettingsPreset GetCategoryPreset(ResearchOpportunityCategoryDef category)
        {
            var preset = categorySettingPresets.FirstOrDefault(cs => cs.category == category);
            if (preset == null)
            {
                Log.Warning($"Did not find preset category values for category {category.defName} in preset {this.defName}");
                preset = new CategorySettingsPreset() { category = category };
                categorySettingPresets.Add(preset);
            }
            return preset;
        }
    }
}
