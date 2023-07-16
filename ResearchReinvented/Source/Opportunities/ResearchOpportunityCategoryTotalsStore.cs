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
        public float researchPoints;
        public ResearchProjectDef project;
        public ResearchOpportunityCategoryDef category;

        public ResearchOpportunityCategoryTotalsStore() { }

        public void ExposeData()
        {
            Scribe_Values.Look(ref researchPoints, "allResearchPoints", forceSave: true);
            Scribe_Defs.Look(ref project, "project");
            Scribe_Defs.Look(ref category, "category");
        }
    }
}
