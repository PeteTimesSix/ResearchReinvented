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

        public static List<ResearchOpportunity> MakeOpportunitiesForProject(ResearchProjectDef project)
        {
            if (ResearchReinventedMod.Settings.debugPrintouts)
            {
                Debug.LogMessage($"Generating opportunities for project {project.label}...");
            }

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
            //var categoriesTotalMultiplier = categories.Sum(c => c.TotalTargetFractionMultiplier);

            float projectResearchPoints = project.baseCost;

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
                var matchingOpportunities = projectOpportunities.Where(o => o.def.GetCategory(o.relation) == category);
                if (!matchingOpportunities.Any())
                    continue;

                float categoryResearchPoints = ((projectResearchPoints / totalMultiplier) * category.targetFractionMultiplier) * category.overflowMultiplier;
                var categoryImportanceTotal = matchingOpportunities.Sum(o => o.importance);
                var matchingOpportunityTypes = matchingOpportunities.Select(o => o.def).ToHashSet();

                foreach (var type in matchingOpportunityTypes)
                {
                    float typeResearchPoints = categoryResearchPoints / matchingOpportunityTypes.Count();

                    var matchingOpportunitiesOfType = matchingOpportunities.Where(o => o.def == type);
                    var matchCount = matchingOpportunitiesOfType.Count(); //attempt to make rares as valuable for research as all other options combined
                    float typeImportanceTotal = matchingOpportunitiesOfType.Sum(o => o.IsAlternate ? 0 : (o.requirement.IsRare ? matchCount : o.importance));
                    if (typeImportanceTotal < 1) //just in case, dont want to divide by zero
                        typeImportanceTotal = 1;

                    if(typeImportanceTotal > category.targetIterations)
                        typeImportanceTotal = category.targetIterations;
                    float baseImportance = 1f / typeImportanceTotal;

                    //if (project.defName == "PackagedSurvivalMeal" || project.defName == "Autodoors")
                    //Log.Message($"project {project} ({projectResearchPoints}) category {category.label} ({categoryResearchPoints}) type {type.defName} (points: {typeResearchPoints} base imp.: {baseImportance} count:{matchCount}) points per: {(typeResearchPoints / typeImportanceTotal)}");

                    foreach (var opportunity in matchingOpportunitiesOfType)
                    {
                        float opportunityResearchPoints;
                        if (category.infiniteOverflow)
                            opportunityResearchPoints = projectResearchPoints;
                        else if (opportunity.requirement.IsRare)
                            opportunityResearchPoints = (typeResearchPoints * baseImportance) * matchCount;
                        else
                            opportunityResearchPoints = (typeResearchPoints * baseImportance) * opportunity.importance;

                        opportunity.SetMaxProgress(opportunityResearchPoints);
                    }
                }
            }

            return projectOpportunities;
        }
    }
}
