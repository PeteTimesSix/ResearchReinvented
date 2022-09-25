using PeteTimesSix.ResearchReinvented.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Opportunities
{
    public class ResearchOpportunityCategoryTotalsStore : IExposable
    {
        public float baseResearchPoints;
        public float allResearchPoints;
        public ResearchProjectDef project;
        public ResearchOpportunityCategoryDef category;

        public void ExposeData()
        {
            Scribe_Values.Look(ref baseResearchPoints, "baseResearchPoints", forceSave: true);
            Scribe_Values.Look(ref allResearchPoints, "allResearchPoints", forceSave: true);
            Scribe_Defs.Look(ref project, "project");
            Scribe_Defs.Look(ref category, "category");
        }
    }
}
