using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        public FloatRange availableAtOverallProgress;

        public int priority;

        public Color color;

        public OpportunityAvailability GetCurrentAvailability(ResearchOpportunity asker){ 
            if (asker.project.ProgressPercent < availableAtOverallProgress.min)
                return OpportunityAvailability.ResearchTooLow;
            if (asker.project.ProgressPercent > availableAtOverallProgress.max)
                return OpportunityAvailability.ResearchTooHigh;
            var totalsStore = ResearchOpportunityManager.instance.GetTotalsStore(asker.project, this);
            if (!infiniteOverflow && GetCurrentTotal() >= totalsStore.allResearchPoints)
                return OpportunityAvailability.CategoryFinished;
            return OpportunityAvailability.Available;
        }

        public float GetCurrentTotal() 
        {
            var matchingOpportunities = ResearchOpportunityManager.instance.AllCurrentOpportunities.Where(o => o.def.GetCategory(o.relation) == this);
            var totalProgress = matchingOpportunities.Sum(o => o.Progress);
            return totalProgress;
        }
    }
}
