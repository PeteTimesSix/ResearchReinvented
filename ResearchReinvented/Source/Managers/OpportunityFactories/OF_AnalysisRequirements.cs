using PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public static class OF_AnalysisRequirements
    {
        public static void MakeFromAnalysisRequirements(ResearchProjectDef project, OpportunityFactoryCollectionsSetForRelation collections)
        {
            if (project.requiredAnalyzed != null)
            {
                HashSet<IngredientCount> ingredientCounts = new HashSet<IngredientCount>();

                foreach (var analysisThing in project.requiredAnalyzed)
                {
                    collections.forDirectAnalysis.Add(analysisThing);
                    if (analysisThing.CostList != null && analysisThing.CostList.Count > 0)
                    {
                        ingredientCounts.AddRange(analysisThing.CostList.Select(c => c.ToIngredientCount()));
                    }
                    if (analysisThing.CostStuffCount > 0)
                    {
                        var stuffs = GenStuff.AllowedStuffsFor(analysisThing);
                        ingredientCounts.AddRange(stuffs.Select(s => new ThingDefCountClass(s, analysisThing.costStuffCount).ToIngredientCount()));
                    }

                    if (!(analysisThing.researchPrerequisites?.Except(project)?.Any() ?? false))
                        collections.forPrototyping.Add(analysisThing);
                    
                }

                var rawIngredients = new List<ThingDefCount>();
                foreach (var ingredientCount in ingredientCounts)
                {
                    if (ingredientCount.IsFixedIngredient)
                        rawIngredients.Add(new ThingDefCount(ingredientCount.FixedIngredient, (int)ingredientCount.GetBaseCount()));
                    else
                    {
                        var possibleIngredients = ingredientCount.filter.AllowedThingDefs.Select(a => new ThingDefCount(a, (int)ingredientCount.GetBaseCount()));
                        rawIngredients.AddRange(possibleIngredients);
                    }
                }
                var ingredientThings = rawIngredients.Select(c => c.ThingDef).ToHashSet();

                collections.forIngredientsAnalysis.AddRange(ingredientThings);
            }
        }
    }
}
