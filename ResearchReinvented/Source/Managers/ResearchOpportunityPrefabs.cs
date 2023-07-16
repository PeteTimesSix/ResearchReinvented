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
        public static readonly float MIN_RESEARCH_POINTS = 1f;

        //public static Dictionary<ResearchProjectDef, List<ResearchOpportunity>> Opportunities { get; private set; }

        /*public static void GenerateAllImplicitOpportunities()
        {
            if (Opportunities != null)
                return;

            Opportunities = new Dictionary<ResearchProjectDef, List<ResearchOpportunity>>();

            foreach (var project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                if (!Opportunities.ContainsKey(project))
                    Opportunities[project] = new List<ResearchOpportunity>();

                var opportunities = MakeOpportunitiesForProject(project);
                Opportunities[project] = opportunities;
            }
        }*/

        public static (List<ResearchOpportunity> opportunities, List<ResearchOpportunityCategoryTotalsStore> categoryStores) MakeOpportunitiesForProject(ResearchProjectDef project)
        {
            if (ResearchReinvented_Debug.debugPrintouts) 
                Log.Message($"Generating opportunities for project {project.label}...");

            var projectOpportunities = new List<ResearchOpportunity>();

            var factory = new MasterFactory();
            projectOpportunities.AddRange(factory.GenerateOpportunities(project));

            HashSet<ResearchOpportunityCategoryDef> categories = new HashSet<ResearchOpportunityCategoryDef>();
            foreach (var opportunity in projectOpportunities)
            {
                foreach (var category in opportunity.def.GetAllCategories())
                {
                    if (!categories.Contains(category))
                        categories.Add(category);
                }
            }

            float projectResearchPoints = project.baseCost;
            float totalMultiplier = 0;

            foreach (var category in categories)
            {
                var anyOpportunities = projectOpportunities.Any(o => o.def.GetCategory(o.relation) == category/* && o.relation == relation*/);
                if (anyOpportunities)
                {
                    totalMultiplier += category.Settings.importanceMultiplierCounted;
                }
            }
            if (totalMultiplier < 1f)
                totalMultiplier = 1f;

            List<ResearchOpportunityCategoryTotalsStore> totalStores = new List<ResearchOpportunityCategoryTotalsStore>();

            foreach (var category in categories)
            {
                var matchingOpportunities = projectOpportunities.Where(o => o.def.GetCategory(o.relation) == category);

                var totalsStore = new ResearchOpportunityCategoryTotalsStore() { project = project, category = category };
                totalsStore.researchPoints = ((projectResearchPoints / totalMultiplier) * category.Settings.importanceMultiplier);

                if (totalsStore.researchPoints < MIN_RESEARCH_POINTS)
                    totalsStore.researchPoints = MIN_RESEARCH_POINTS;

                totalStores.Add(totalsStore);

                if (!matchingOpportunities.Any())
                    continue;

                var categoryImportanceTotal = matchingOpportunities.Sum(o => o.importance);
                var matchingOpportunityTypes = matchingOpportunities.Select(o => (def: o.def, rel: o.relation)).ToHashSet();

                var minimumOpportunityResearchPoints = totalsStore.researchPoints / category.Settings.targetIterations;

                foreach (var type in matchingOpportunityTypes)
                {
                    float typeResearchPoints = totalsStore.researchPoints / matchingOpportunityTypes.Count();

                    var matchingOpportunitiesOfType = matchingOpportunities.Where(o => o.def == type.def && o.relation == type.rel);
                    var matchCount = matchingOpportunitiesOfType.Count();       //attempt to make rares as valuable for research as all other options combined
                    float typeImportanceTotal = matchingOpportunitiesOfType.Sum(o => o.IsAlternate ? 0 : (o.requirement.IsRare ? matchCount : o.importance));
                    if (typeImportanceTotal < 1) //just in case, dont want to divide by zero
                        typeImportanceTotal = 1;

                    if(typeImportanceTotal > category.Settings.targetIterations)
                        typeImportanceTotal = category.Settings.targetIterations;
                    float baseImportance = 1f / typeImportanceTotal;

                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"project {project} ({projectResearchPoints}) category {category.label} ({categoryImportanceTotal}) min: {minimumOpportunityResearchPoints} type.def {type.def.defName} type.rel {type.rel} (points: {typeResearchPoints} base imp.: {baseImportance} count:{matchCount}) points per: {(typeResearchPoints * baseImportance)}");

                    foreach (var opportunity in matchingOpportunitiesOfType)
                    {
                        float opportunityResearchPoints;
                        //if (category.Settings.infiniteOverflow)
                        //    opportunityResearchPoints = projectResearchPoints;
                        if (opportunity.requirement.IsRare)
                            opportunityResearchPoints = Math.Max(typeResearchPoints, minimumOpportunityResearchPoints);
                        else
                            opportunityResearchPoints = Math.Max(((typeResearchPoints * baseImportance) * opportunity.importance), (minimumOpportunityResearchPoints * opportunity.importance));

                        opportunity.SetMaxProgress(Math.Max(MIN_RESEARCH_POINTS, opportunityResearchPoints));
                    }
                }
            }

            return (projectOpportunities, totalStores);
        }
    }
}
