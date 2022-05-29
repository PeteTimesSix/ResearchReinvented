using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    /*public static class ResearchOpportunityFactory_Old
    {

        public enum UnlockLevel 
        {
            Ancestor,
            Researching,
            Descendant
        }


        internal static IEnumerable<ResearchOpportunity> MakeFromIngredients(ResearchProjectDef project, List<Def> unlockedDefs, UnlockLevel researching)
        {
            if (researching != UnlockLevel.Researching)
                yield break;

            var ingredientCounts = new List<IngredientCount>();
            foreach(var unlock in unlockedDefs)
            {
                if (unlock is RecipeDef)
                {
                    var recipe = unlock as RecipeDef;
                    ingredientCounts.AddRange(recipe.ingredients);
                }
                else if (unlock is ThingDef)
                {
                    var thing = unlock as ThingDef;
                    if (!typeof(Plant).IsAssignableFrom(thing.thingClass))
                    {
                        if (thing.CostList != null && thing.CostList.Count > 0)
                        {
                            ingredientCounts.AddRange(thing.CostList.Select(c => c.ToIngredientCount()));
                        }
                        if (thing.CostStuffCount > 0)
                        {
                            var stuffs = GenStuff.AllowedStuffsFor(thing);
                            ingredientCounts.AddRange(stuffs.Select(s => new ThingDefCountClass(s, thing.costStuffCount).ToIngredientCount()));
                        }
                    }
                }
                else if (unlock is TerrainDef)
                {
                    var terrain = unlock as TerrainDef;
                    if (terrain.CostList != null && terrain.CostList.Count > 0)
                    {
                        ingredientCounts.AddRange(terrain.CostList.Select(c => c.ToIngredientCount()));
                    }
                }
                else
                {
                    Debug.LogMessage($"{project.defName} unlocks {unlock.defName} which is not a handled type of def");
                }
            }

            var rawIngredientCounts = new List<ThingDefCount>();
            foreach(var ingredientCount in ingredientCounts) 
            {
                if (ingredientCount.IsFixedIngredient)
                    rawIngredientCounts.Add(new ThingDefCount(ingredientCount.FixedIngredient, (int)ingredientCount.GetBaseCount()));
                else
                {
                    var possibleIngredients = ingredientCount.filter.AllowedThingDefs.Select(a => new ThingDefCount(a, (int)ingredientCount.GetBaseCount()));
                    rawIngredientCounts.AddRange(possibleIngredients);
                }
            }
            var things = rawIngredientCounts.Select(c => c.ThingDef).Distinct();
            //var thingsWithCounts = things.Select(t => new ThingDefCount(t, rawIngredientCounts.Where(c => c.ThingDef == t).Sum(c => c.Count)));

            foreach(var thing in things)
            {
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseMaterials, new ROComp_RequiresThing(thing));
            }
            yield break;
        }

        public static IEnumerable<ResearchOpportunity> MakeFromUnlock(ResearchProjectDef project, Def unlock, UnlockLevel unlockLevel)
        {
            if (unlock is RecipeDef)
            {
                var recipe = unlock as RecipeDef;
                foreach (var opportunity in MakeFromRecipe(project, recipe, unlockLevel, true))
                    yield return opportunity;
            }
            else if (unlock is ThingDef)
            {
                var thing = unlock as ThingDef;
                if (typeof(Plant).IsAssignableFrom(thing.thingClass))
                {
                    foreach (var opportunity in MakeFromPlant(project, thing, unlockLevel))
                        yield return opportunity;
                }
                else
                {
                    foreach (var opportunity in MakeFromThing(project, thing, unlockLevel))
                        yield return opportunity;
                }
                foreach(var recipe in thing.AllRecipes) 
                {
                    foreach (var opportunity in MakeFromRecipe(project, recipe, unlockLevel, false))
                        yield return opportunity;
                }
            }
            else if (unlock is TerrainDef)
            {
                var terrain = unlock as TerrainDef;
                foreach (var opportunity in MakeFromTerrain(project, terrain, unlockLevel))
                    yield return opportunity;
            }
            else
            {
                Debug.LogMessage($"{project.defName} unlocks {unlock.defName} which is not a handled type of def");
            }
            yield break;
        }

        public static IEnumerable<ResearchOpportunity> MakeFromRecipe(ResearchProjectDef project, RecipeDef recipe, UnlockLevel unlockLevel, bool includeProducts)
        {
            if (recipe.products != null && includeProducts)
            {
                foreach (var product in recipe.products)
                {
                    foreach (var opportunity in MakeFromThing(project, product.thingDef, unlockLevel))
                        yield return opportunity;
                }
            }
            if(recipe.recipeUsers != null)
            {
                foreach (var bench in recipe.recipeUsers) 
                {
                    switch (unlockLevel)
                    {
                        case UnlockLevel.Researching:
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseProductionFacility, new ROComp_RequiresThing(bench));
                            break;
                        case UnlockLevel.Ancestor:
                        case UnlockLevel.Descendant:
                            break;
                        default:
                            throw new InvalidOperationException("Unknown unlockLevel passed to research opportunity generator function");
                    }
                }
            }
        }

        public static IEnumerable<ResearchOpportunity> MakeFromPlant(ResearchProjectDef project, ThingDef thing, UnlockLevel unlockLevel)
        {
            switch (unlockLevel)
            {
                case UnlockLevel.Researching:
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePlant, new ROComp_RequiresThing(thing));
                    var product = thing.plant?.harvestedThingDef;
                    if(product != null)
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseHarvestProduct, new ROComp_RequiresThing(thing));
                    }
                    break;
                case UnlockLevel.Ancestor:
                case UnlockLevel.Descendant:
                    break;
                default:
                    throw new InvalidOperationException("Unknown unlockLevel passed to research opportunity generator function");
            }
        }

        public static IEnumerable<ResearchOpportunity> MakeFromThing(ResearchProjectDef project, ThingDef thing, UnlockLevel unlockLevel)
        {
            switch (unlockLevel) 
            {
                case UnlockLevel.Ancestor:
                    if (thing.EverHaulable)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.ForwardEngineer, new ROComp_RequiresThing(thing));
                    else
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.ForwardEngineerInPlace, new ROComp_RequiresThing(thing));
                    break;
                case UnlockLevel.Researching:
                case UnlockLevel.Descendant:
                    if (thing.EverHaulable)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.ReverseEngineer, new ROComp_RequiresThing(thing));
                    else
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.ReverseEngineerInPlace, new ROComp_RequiresThing(thing));
                    break;
                default: 
                    throw new InvalidOperationException("Unknown unlockLevel passed to research opportunity generator function");
            }
        }

        public static IEnumerable<ResearchOpportunity> MakeFromTerrain(ResearchProjectDef project, TerrainDef terrain, UnlockLevel unlockLevel)
        {
            switch (unlockLevel)
            {
                case UnlockLevel.Ancestor:
                    break;
                case UnlockLevel.Researching:
                    if (terrain.IsSoil)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseSoil, new ROComp_RequiresTerrain(terrain));
                    else if(terrain.BuildableByPlayer)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.ReverseEngineerFloor, new ROComp_RequiresTerrain(terrain));
                    else
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseTerrain, new ROComp_RequiresTerrain(terrain));
                    break;
                case UnlockLevel.Descendant:
                    break;
                default:
                    throw new InvalidOperationException("Unknown unlockLevel passed to research opportunity generator function");
            }
        }

        public static IEnumerable<ResearchOpportunity> MakeFromProject(ResearchProjectDef project)
        {
            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.BasicResearch, new ROComp_RequiresNothing());

            foreach (var specialOpportunity in DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(o => o.researchProject == project))
            {
                ResearchOpportunityComp requirementComp = null;
                switch (specialOpportunity.requirement)
                {
                    case OppportunityRequirement.Nothing:
                        requirementComp = new ROComp_RequiresNothing(); break;
                    case OppportunityRequirement.Thing:
                        requirementComp = new ROComp_RequiresThing(specialOpportunity.targetThing); break;
                    case OppportunityRequirement.Ingredients:
                        requirementComp = new ROComp_RequiresIngredients(specialOpportunity.targetIngredients); break;
                    case OppportunityRequirement.Terrain:
                        requirementComp = new ROComp_RequiresTerrain(specialOpportunity.targetTerrain); break;
                }
                if(requirementComp == null)
                {
                    Log.Warning($"specialOpportunity {specialOpportunity.defName} has unknown requirement type");
                    continue;
                }
                if (requirementComp.TargetIsNull)
                {
                    Log.Warning($"specialOpportunity {specialOpportunity.defName} has missing requirement target");
                    continue;
                }

                yield return new ResearchOpportunity(project, specialOpportunity.opportunityType, requirementComp);
            }
        }

    }*/
}
