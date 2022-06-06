using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories;
using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public static class ResearchOpportunityPrefabs
    {
        public static Dictionary<ResearchProjectDef, List<ResearchOpportunity>> Opportunities { get; private set; }
        public static Dictionary<ResearchProjectDef, Dictionary<ResearchOpportunityCategoryDef, float>> OpportunityTotals { get; private set; }

        public static void GenerateAllImplicitOpportunities()
        {
            if (Opportunities != null)
                return;

            Opportunities = new Dictionary<ResearchProjectDef, List<ResearchOpportunity>>();
            OpportunityTotals = new Dictionary<ResearchProjectDef, Dictionary<ResearchOpportunityCategoryDef, float>>();

            foreach (var project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                if (!Opportunities.ContainsKey(project))
                    Opportunities[project] = new List<ResearchOpportunity>();
                if (!OpportunityTotals.ContainsKey(project))
                    OpportunityTotals[project] = new Dictionary<ResearchOpportunityCategoryDef, float>();

                var projectOpportunities = Opportunities[project];

                var factory = new MasterFactory();
                projectOpportunities.AddRange(factory.GenerateOpportunities(project));

                HashSet<ResearchOpportunityCategoryDef> categories = new HashSet<ResearchOpportunityCategoryDef>();
                foreach (var opportunity in projectOpportunities)
                {
                    foreach(var category in opportunity.def.GetAllCategories())
                    {
                        if (!categories.Contains(category))
                            categories.Add(category);
                    }
                }
                //var categoriesTotalMultiplier = categories.Sum(c => c.TotalTargetFractionMultiplier);

                float totalMultiplier = 0;

                foreach (var category in categories)
                {
                    foreach (ResearchRelation relation in Enum.GetValues(typeof(ResearchRelation)))
                    {
                        var anyOpportunities = projectOpportunities.Any(o => o.def.GetCategory(relation) == category && o.relation == relation);
                        if (anyOpportunities)
                        {
                            totalMultiplier += category.targetFractionMultiplier;
                        }
                    }
                }

                foreach (var category in categories)
                {
                    foreach (ResearchRelation relation in Enum.GetValues(typeof(ResearchRelation))) 
                    {
                        var matchingOpportunities = projectOpportunities.Where(o => o.def.GetCategory(relation) == category && o.relation == relation);
                        foreach (var opportunity in matchingOpportunities)
                        {
                            opportunity.AdjustMaxProgress(totalMultiplier, matchingOpportunities.Sum(o => o.importance));
                        }
                    }
                }
            }
        }
    }
}
