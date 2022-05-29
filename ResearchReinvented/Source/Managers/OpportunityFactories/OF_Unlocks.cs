using PeteTimesSix.ResearchReinvented.Opportunities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public static class OF_Unlocks
    {
        public static void MakeFromUnlocks(ResearchProjectDef project, ResearchRelation relation, OpportunityFactoryCollectionsSetForRelation collections)
        {
            if(project.UnlockedDefs != null)
            {
                HashSet<IngredientCount> ingredientCounts = new HashSet<IngredientCount>();

                foreach (var unlock in project.UnlockedDefs) 
                {
                    if (unlock is RecipeDef asRecipe)
                    {
                        ingredientCounts.AddRange(asRecipe.ingredients);
                    }
                    else if (unlock is ThingDef asThing)
                    {
                        collections.forDirectAnalysis.Add(asThing);
                        if (asThing.CostList != null && asThing.CostList.Count > 0)
                        {
                            ingredientCounts.AddRange(asThing.CostList.Select(c => c.ToIngredientCount()));
                        }
                        if (asThing.CostStuffCount > 0)
                        {
                            var stuffs = GenStuff.AllowedStuffsFor(asThing);
                            ingredientCounts.AddRange(stuffs.Select(s => new ThingDefCountClass(s, asThing.costStuffCount).ToIngredientCount()));
                        }

                    }
                    else if (unlock is TerrainDef asTerrain)
                    {
                        collections.forDirectAnalysis.Add(asTerrain);
                        if (asTerrain.CostList != null && asTerrain.CostList.Count > 0)
                        {
                            ingredientCounts.AddRange(asTerrain.CostList.Select(c => c.ToIngredientCount()));
                        }
                    }
                    else
                    {
                        Log.Warning($"{project.defName} unlocks {unlock.defName} which is not a handled type of def");
                    }
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
