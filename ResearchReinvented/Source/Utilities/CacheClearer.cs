using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class CacheClearer
    {
        public static void ClearCaches() 
        {
            WorkGiver_Analyse.ClearMatchingOpportunityCache();
            WorkGiver_AnalyseInPlace.ClearMatchingOpportunityCache();
            WorkGiver_AnalyseTerrain.ClearMatchingOpportunityCache();
            WorkGiver_LearnRemotely.ClearMatchingOpportunityCache();   
            WorkGiver_ResearcherRR.ClearMatchingOpportunityCache();
            WorkGiver_Warden_Interrogate.ClearMatchingOpportunityCache();
        }
    }
}
