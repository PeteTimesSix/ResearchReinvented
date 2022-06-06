using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public class OF_Recipes
    {
        public static void MakeFromRecipes(ResearchProjectDef project, OpportunityFactoryCollectionsSetForRelation collections)
        {
            HashSet<ThingDef> users = new HashSet<ThingDef>();
            HashSet<IngredientCount> ingredients = new HashSet<IngredientCount>();
            HashSet<ThingDef> products = new HashSet<ThingDef>();

            foreach (var recipe in GatherDirectRecipes(project)) 
            {
                users.AddRange(recipe.AllRecipeUsers);
                if (recipe.products != null)
                    products.AddRange(recipe.products.Select(p => p.thingDef));
                if (recipe.ingredients != null)
                    ingredients.AddRange(recipe.ingredients);
            }

            foreach (var recipe in GatherCreationRecipes(project))
            {
                users.AddRange(recipe.AllRecipeUsers);
            }

            foreach (var recipe in GatherNewImpliedRecipes(project).Where(r => r.products != null))
            {
                if (recipe.products != null)
                    products.AddRange(recipe.products.Select(p => p.thingDef));
                if (recipe.ingredients != null)
                    ingredients.AddRange(recipe.ingredients);
            }

            var rawIngredients = new List<ThingDefCount>();
            foreach (var ingredientCount in ingredients)
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

            collections.forProductionFacilityAnalysis.AddRange(users);
            collections.forDirectAnalysis.AddRange(products);
            collections.forIngredientsAnalysis.AddRange(ingredientThings);
        }

        private static HashSet<RecipeDef> GatherDirectRecipes(ResearchProjectDef project)
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

            recipes.AddRange(
                DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(r => r.researchPrerequisite == project || (r.researchPrerequisites != null && r.researchPrerequisites.Contains(project)))
                );

            if (project.UnlockedDefs != null)
            {
                recipes.AddRange(
                    project.UnlockedDefs.Where(u => u is RecipeDef).Cast<RecipeDef>()
                    );
            }

            return recipes;
        }
        private static HashSet<RecipeDef> GatherNewImpliedRecipes(ResearchProjectDef project)
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

            if (project.UnlockedDefs != null)
            {
                var unlockedThings = project.UnlockedDefs.Where(u => u is ThingDef).Cast<ThingDef>().ToList();
                foreach (var thing in unlockedThings)
                {
                    if (thing.AllRecipes != null)
                    {
                        /*foreach(var recipe in thing.AllRecipes) 
                        {
                            if (recipe.ProducedThingDef?.defName == "Chemfuel")
                            {
                                Log.Message($"checking recipe for " + (recipe.ProducedThingDef != null ? recipe.ProducedThingDef.defName : "Multiple things"));
                                Log.Message($"unlocked specifically by {project.defName}: {recipe.UnlockedSpecificallyBy(project)}");
                                Log.Message($"has no research prereqs: {recipe.HasNoResearchPrerequisites()}");
                                Log.Message($"unlocked ONLY by things in {project.defName}: {recipe.UnlocksOnlyWith(unlockedThings)}");
                                foreach(var user in recipe.AllRecipeUsers) 
                                {
                                    Log.Message($"has recipe user {user.defName}");
                                }
                            }
                            //if (recipe.UnlockedSpecificallyBy(project) || recipe.HasNoResearchPrerequisites() && recipe.UnlocksOnlyWith(unlockedThings))
                            //    recipes.Add(recipe);
                        }*/
                        recipes.AddRange(thing.AllRecipes.Where(r => r.UnlockedSpecificallyBy(project) || (r.HasNoResearchPrerequisites() && r.UnlocksOnlyWith(unlockedThings))));
                    }
                }
            }

            return recipes;
        }

        private static HashSet<RecipeDef> GatherCreationRecipes(ResearchProjectDef project) 
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

            if (project.UnlockedDefs != null)
            {
                foreach (var unlock in project.UnlockedDefs)
                {
                    if(unlock is ThingDef asThing)
                    {
                        recipes.AddRange(
                            DefDatabase<RecipeDef>.AllDefsListForReading
                                .Where(r => r.products.Any(tdc => tdc.thingDef == asThing))
                        );
                    }
                }
            }

            return recipes;
        }

    }
}
