using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
    public class ResearchOpportunityRelationValues
    {
    }


    public class ResearchOpportunityCategoryDef : Def
    {
        public string name;

        public float targetFractionMultiplier;
        public float targetIterations;
        public float overflowMultiplier;
        public bool infiniteOverflow;
        public float researchSpeedMultiplier;

        public int priority;
    }
}
