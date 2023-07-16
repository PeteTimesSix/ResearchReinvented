using PeteTimesSix.ResearchReinvented.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Data
{
    public class CategorySettingsPreset
    {
        public ResearchOpportunityCategoryDef category;

        public bool enabled = true;

        public float importanceMultiplier = 1f;
        public float importanceMultiplierCounted = 1f;
        public bool infiniteOverflow = false;
        public float targetIterations = 1f;
        public float researchSpeedMultiplier = 1f;

        public FloatRange availableAtOverallProgress = new FloatRange(0f, 1f);
    }
}
