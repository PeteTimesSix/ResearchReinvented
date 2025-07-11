using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public class OpportunityFactoryCollectionsSetForRelation
    {
        internal HashSet<ThingDef> forProductionFacilityAnalysis = new HashSet<ThingDef>();
        internal HashSet<Def> forDirectAnalysis = new HashSet<Def>();
        internal HashSet<ThingDef> forIngredientsAnalysis = new HashSet<ThingDef>();
        internal HashSet<ThingDef> forHarvestProductAnalysis = new HashSet<ThingDef>();
        internal HashSet<ThingDef> forFuelAnalysis = new HashSet<ThingDef>();
        internal HashSet<Def> forPrototyping = new HashSet<Def>();
        internal HashSet<SpecialResearchOpportunityDef> specials = new HashSet<SpecialResearchOpportunityDef>();

        public ResearchRelation relation;

        public OpportunityFactoryCollectionsSetForRelation(ResearchRelation relation) 
        {
            this.relation = relation;
        }

        public void RemoveDuplicates(OpportunityFactoryCollectionsSetForRelation other)
        {
            forProductionFacilityAnalysis.ExceptWith(other.forProductionFacilityAnalysis);
            forDirectAnalysis.ExceptWith(other.forDirectAnalysis);
            forIngredientsAnalysis.ExceptWith(other.forIngredientsAnalysis);
            forHarvestProductAnalysis.ExceptWith(other.forHarvestProductAnalysis);
            forFuelAnalysis.ExceptWith(other.forFuelAnalysis);
            forPrototyping.ExceptWith(other.forPrototyping);

            specials.ExceptWith(other.specials);
        }

        public void RemoveBlacklisted()
        {
            forProductionFacilityAnalysis.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));
            forDirectAnalysis.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));
            forIngredientsAnalysis.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));
            forHarvestProductAnalysis.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));
            forFuelAnalysis.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));
            forPrototyping.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));

            specials.RemoveWhere(d => d.modExtensions != null && d.modExtensions.Any(e => e is Blacklisted));
        }

        public void RemoveAlternatesWhereAppropriate()
        {
            forProductionFacilityAnalysis.RemoveWhere(t => AsAlternateInCollection(t, forProductionFacilityAnalysis, AlternatesMode.EQUIVALENT));
            forDirectAnalysis.RemoveWhere(t => AsAlternateInCollection(t, forDirectAnalysis, relation == ResearchRelation.Direct ? AlternatesMode.SIMILAR : AlternatesMode.EQUIVALENT));
            forIngredientsAnalysis.RemoveWhere(t => AsAlternateInCollection(t, forIngredientsAnalysis, AlternatesMode.EQUIVALENT));
            forHarvestProductAnalysis.RemoveWhere(t => AsAlternateInCollection(t, forHarvestProductAnalysis, AlternatesMode.EQUIVALENT));
            forFuelAnalysis.RemoveWhere(t => AsAlternateInCollection(t, forFuelAnalysis, AlternatesMode.EQUIVALENT));
            forPrototyping.RemoveWhere(t => AsAlternateInCollection(t, forPrototyping, AlternatesMode.EQUIVALENT));
        }

        public bool AsAlternateInCollection(Def testee, IEnumerable<Def> collection, AlternatesMode mode)
        {
            switch(mode)
            {
                case AlternatesMode.NONE:
                    return false;
                case AlternatesMode.EQUIVALENT:
                    {
                        return collection.Any(o => 
                        (o is ThingDef tho && AlternatesKeeper.alternatesEquivalent.TryGetValue(tho, out ThingDef[] alternates) && alternates.Contains(testee)) ||
                        (o is TerrainDef teo && AlternatesKeeper.alternateEquivalentTerrains.TryGetValue(teo, out TerrainDef[] alternateTerrains) && alternateTerrains.Contains(testee)) ||
                        (o is RecipeDef reo && (AlternatesKeeper.alternateEquivalentRecipes.TryGetValue(reo, out RecipeDef[] alternateRecipes) && alternateRecipes.Contains(testee)))
                        );
                    }
                case AlternatesMode.SIMILAR:
                    {
                        return collection.Any(o =>
                        (o is ThingDef tho && AlternatesKeeper.alternatesSimilar.TryGetValue(tho, out ThingDef[] alternates) && alternates.Contains(testee)) ||
                        (o is TerrainDef teo && AlternatesKeeper.alternateSimilarTerrains.TryGetValue(teo, out TerrainDef[] alternateTerrains) && alternateTerrains.Contains(testee)) ||
                        (o is RecipeDef reo && (AlternatesKeeper.alternateSimilarRecipes.TryGetValue(reo, out RecipeDef[] alternateRecipes) && alternateRecipes.Contains(testee)))
                        );
                    }
            }
            return false;
        }
    }

    public class OpportunityFactoryCollectionsSet
    {
        OpportunityFactoryCollectionsSetForRelation collections_direct = new OpportunityFactoryCollectionsSetForRelation(ResearchRelation.Direct);
        OpportunityFactoryCollectionsSetForRelation collections_ancestor = new OpportunityFactoryCollectionsSetForRelation(ResearchRelation.Ancestor);
        OpportunityFactoryCollectionsSetForRelation collections_descendant = new OpportunityFactoryCollectionsSetForRelation(ResearchRelation.Descendant);

        public OpportunityFactoryCollectionsSetForRelation GetSet(ResearchRelation relation) 
        {
            switch (relation)
            {
                case ResearchRelation.Direct:
                    return collections_direct;
                case ResearchRelation.Ancestor:
                    return collections_ancestor;
                case ResearchRelation.Descendant:
                    return collections_descendant;
            }
            return null;
        }

        public OpportunityFactoryCollectionsSetForRelation[] GetSets() 
        {
            return new OpportunityFactoryCollectionsSetForRelation[3] { collections_direct, collections_ancestor, collections_descendant };
        }

        public void RemoveBlacklisted()
        {
            collections_direct.RemoveBlacklisted();
            collections_ancestor.RemoveBlacklisted();
            collections_descendant.RemoveBlacklisted();
        }

        public void RemoveDuplicates()
        {
            collections_ancestor.RemoveDuplicates(collections_direct);
            collections_descendant.RemoveDuplicates(collections_direct);
        }

        public void RemoveAlternatesWhereAppropriate()
        {
            collections_direct.RemoveAlternatesWhereAppropriate();
            collections_ancestor.RemoveAlternatesWhereAppropriate();
            collections_descendant.RemoveAlternatesWhereAppropriate();
        }
    }

    public class MasterFactory
    {
        public MasterFactory() 
        {
        }

        public IEnumerable<ResearchOpportunity> GenerateOpportunities(ResearchProjectDef project) 
        {
            if (project == null)
                return new List<ResearchOpportunity>();

            var collections = FillCollections(project);
            var opportunities = MakeOpportunities(project, collections);

            opportunities = ValidOpportunities(opportunities);

            return opportunities;
        }

        public IEnumerable<ResearchOpportunity> ValidOpportunities(IEnumerable<ResearchOpportunity> opportunities) 
        {
            foreach(var opportunity in opportunities) 
            {
                if(opportunity.IsValid()) 
                {
                    yield return opportunity;
                }
            }
            //yield break;
        }

        private IEnumerable<ResearchOpportunity> MakeOpportunities(ResearchProjectDef project, OpportunityFactoryCollectionsSet collections)
        {
            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.BasicResearch, ResearchRelation.Direct, new ROComp_RequiresNothing(), "Project");
            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.SchematicStudy, ResearchRelation.Direct, new ROComp_RequiresSchematicWithProject(project), "Schematic");
            if (ModsConfig.RoyaltyActive && project.Techprint != null)
            {
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseTechprint, ResearchRelation.Direct, new ROComp_RequiresThing(project.Techprint, AlternatesMode.NONE), "Techprint");
            }
            /*if (ModsConfig.OdysseyActive && project.requireGravEngineInspected)
            {
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Analyse, ResearchRelation.Direct, new ROComp_RequiresThing(ThingDefOf.GravEngine), "Grav engine", forceRare: true);
            }*/

            {
                {
                    var playerFaction = Faction.OfPlayer;
                    var playerTechLevelModifier = OF_Factions.Modifiers.TryGetValue((playerFaction.def.techLevel, project.techLevel), 0f);
                    if(playerTechLevelModifier > 0f)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Brainstorming, ResearchRelation.Direct, new ROComp_RequiresFaction(playerFaction), "player faction", importance: playerTechLevelModifier);
                }

                var factions = Find.FactionManager.GetFactions(/*not of player,*/ allowHidden: true, allowDefeated: true, allowNonHumanlike: false, minTechLevel: TechLevel.Neolithic, allowTemporary: false);

                foreach (var faction in factions) 
                {
                    var techLevelModifier = OF_Factions.Modifiers.TryGetValue((faction.def.techLevel, project.techLevel), 0f);
                    if(techLevelModifier > 0f)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.GainFactionKnowledge, ResearchRelation.Direct, new ROComp_RequiresFaction(faction), "faction", importance: techLevelModifier);
                }

                {
                    var techLevelModifier = 0.5f;
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.GainFactionlessKnowledge, ResearchRelation.Direct, new ROComp_RequiresFactionlessPawn(), "no faction", importance: techLevelModifier);
                }
            }

            foreach (var set in collections.GetSets())
            {
                foreach (var productionFacility in set.forProductionFacilityAnalysis.Where(facility => !project.UnlockedDefs.Contains(facility))) //dont include tables we're trying to invent
                {
                    if(typeof(Pawn).IsAssignableFrom(productionFacility.thingClass))
                    {
                        if (productionFacility.race.IsFleshModAware())
                        {
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePawn, set.relation, new ROComp_RequiresThing(productionFacility, AlternatesMode.EQUIVALENT), "set.production facility pawn");
                            if (productionFacility.race.corpseDef != null)
                                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissect, set.relation, new ROComp_RequiresThing(productionFacility.race.corpseDef, AlternatesMode.EQUIVALENT), "set.production facility pawn (corpse)");
                        }
                        else
                        {
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePawnNonFlesh, set.relation, new ROComp_RequiresThing(productionFacility, AlternatesMode.EQUIVALENT), "set.production facility pawn nonflesh");
                            if (productionFacility.race.corpseDef != null)
                                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissectNonFlesh, set.relation, new ROComp_RequiresThing(productionFacility.race.corpseDef, AlternatesMode.EQUIVALENT), "set.production facility pawn (corpse non-flesh)");
                        }
                    }
                    else
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseProductionFacility, set.relation, new ROComp_RequiresThing(productionFacility, AlternatesMode.EQUIVALENT), "set.production facility");
                    }
                }

                foreach (var analysable in set.forDirectAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromDirectAnalysis(project, analysable, set.relation))
                        yield return opportunity;
                }

                foreach (var material in set.forIngredientsAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromIngredientAnalysis(project, material, set.relation))
                        yield return opportunity;
                }

                foreach (var product in set.forHarvestProductAnalysis)
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseHarvestProduct, set.relation, new ROComp_RequiresThing(product, AlternatesMode.EQUIVALENT), "set.harvest product");
                }

                foreach (var fuel in set.forFuelAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromFuelAnalysis(project, fuel, set.relation))
                        yield return opportunity;
                }

                foreach (var prototypeable in set.forPrototyping)
                {
                    foreach (var opportunity in OpportunitiesFromPrototyping(project, prototypeable, set.relation))
                        yield return opportunity;
                }

                foreach (var special in set.specials)
                {
                    foreach (var opportunity in OpportunitiesFromSpecialOpportunityDef(project, special, set.relation))
                        yield return opportunity;
                }
            }

            //specials.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.originals != null && s.originals.Intersect(allThings).Any()));
            
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromSpecialOpportunityDef(ResearchProjectDef project, SpecialResearchOpportunityDef special, ResearchRelation relation)
        {
            var relationOverride = special.relationOverride.HasValue ? special.relationOverride.Value : relation;
            bool requiredSomething = false;
            if (special.things != null) 
            {
                requiredSomething = true;
                foreach (var thing in special.things)
                {
                    if (special.opportunityType != null)
                    { 
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, relationOverride, new ROComp_RequiresThing(thing, special.altsMode), "special (thing)", special.importanceMultiplier, special.rare, special.freebie);
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromDirectAnalysis(project, thing, relationOverride))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }

            if (special.terrains != null)
            {
                requiredSomething = true;
                foreach (var terrain in special.terrains)
                {
                    if (special.opportunityType != null)
                    {
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, relationOverride, new ROComp_RequiresTerrain(terrain, special.altsMode), "special (terrain)", special.importanceMultiplier, special.rare, special.freebie);
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromDirectAnalysis(project, terrain, relationOverride))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }

            if (special.recipes != null)
            {
                requiredSomething = true;
                foreach (var recipe in special.recipes)
                {
                    if (special.opportunityType != null)
                    {
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, relationOverride, new ROComp_RequiresRecipe(recipe, special.altsMode), "special (recipe)", special.importanceMultiplier, special.rare, special.freebie);
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromPrototyping(project, recipe, relationOverride))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }

            if (!requiredSomething)
                yield return new ResearchOpportunity(project, special.opportunityType, relation, new ROComp_RequiresNothing(), "special (nothing)", special.importanceMultiplier);
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromFuelAnalysis(ResearchProjectDef project, ThingDef fuel, ResearchRelation relation)
        {
            if (fuel.ingestible != null)
            {
                if(fuel.IsDrug)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelDrug, relation, new ROComp_RequiresThing(fuel, AlternatesMode.EQUIVALENT), "fuel analysis (drug)");
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFood, relation, new ROComp_RequiresThing(fuel, AlternatesMode.EQUIVALENT), "fuel analysis (ingestible)");
            }
            else
            {
                if (fuel.GetStatValueAbstract(StatDefOf.Flammability) >= 0.5f)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFlammable, relation, new ROComp_RequiresThing(fuel, AlternatesMode.EQUIVALENT), "fuel analysis (flammable)");
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuel, relation, new ROComp_RequiresThing(fuel, AlternatesMode.EQUIVALENT), "fuel analysis");
            }
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromIngredientAnalysis(ResearchProjectDef project, ThingDef material, ResearchRelation relation)
        {
            if (material.IsMedicine)
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseMedicine, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "ingre. analysis (medicine)");
            //food n' drugs
            else if (material.ingestible != null)
            {
                //corpses
                if (material.IsCorpse)
                {
                    var deadThing = material.ingestible?.sourceDef;
                    if (deadThing != null)
                    {
                        var deadThingRace = deadThing.race;
                        if (deadThingRace != null)
                        {
                            if (deadThingRace.IsFleshModAware())
                                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissect, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "direct analysis (corpse)");
                            else
                                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissectNonFlesh, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "direct analysis (corpse non-flesh)");

                        }
                    }
                }
                else if (material.IsDrug)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDrug, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "ingre. analysis (drug)");
                else if (material.IsTrulyRawFood())
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFood, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "ingre. analysis (raw food)");
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredientsFood, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "ingre. analysis (ingestible)");
            }
            //everything else
            else
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredients, relation, new ROComp_RequiresThing(material, AlternatesMode.EQUIVALENT), "ingre. analysis");
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromDirectAnalysis(ResearchProjectDef project, Def reverseEngineerable, ResearchRelation relation)
        {
            var altsMode = AlternatesMode.NONE;
            switch(relation)
            {
                case ResearchRelation.Ancestor:
                    altsMode = AlternatesMode.EQUIVALENT;
                    break;
                case ResearchRelation.Direct:
                    altsMode = AlternatesMode.SIMILAR;
                    break;
            }
            var allowAlternates = relation == ResearchRelation.Direct;
            if (reverseEngineerable is ThingDef asThing)
            {
                if (typeof(Pawn).IsAssignableFrom(asThing.thingClass) && asThing.race != null)
                {
                    if (asThing.race.IsFleshModAware())
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePawn, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis pawn");
                        if (asThing.race.corpseDef != null)
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissect, relation, new ROComp_RequiresThing(asThing.race.corpseDef, altsMode), "direct analysis pawn (corpse)");
                    }
                    else
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePawnNonFlesh, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis pawn nonflesh");
                        if (asThing.race.corpseDef != null)
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissectNonFlesh, relation, new ROComp_RequiresThing(asThing.race.corpseDef, altsMode), "direct analysis pawn (corpse non-flesh)");
                    }

                }
                else if (typeof(Plant).IsAssignableFrom(asThing.thingClass))
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePlant, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (plant)");
                else if (!asThing.EverHaulable)
                {
                    if (!asThing.IsInstantBuild())
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Analyse, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (unhaulable)");

                        //if (!isAlternate && relation == ResearchRelation.Direct && asThing.BuildableByPlayer)
                        //    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeConstruction, relation, new ROComp_RequiresThing(asThing), "direct analysis (unhaulable)");
                    }
                }
                else
                {
                    if (asThing.IsMedicine)
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseMedicine, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (medicine)");
                    }
                    //food n' drugs
                    else if (asThing.ingestible != null)
                    {
                        if (asThing.IsDrug)
                        {
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDrug, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (drug)");
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.TrialDrug, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (drug)");
                        }
                        else if(asThing.IsTrulyRawFood())
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseRawFood, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (raw food)");
                        else
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFood, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis (ingestible)");
                    }
                    //everything else
                    else 
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Analyse, relation, new ROComp_RequiresThing(asThing, altsMode), "direct analysis");

                    //if (!isAlternate && relation == ResearchRelation.Direct)
                    //    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeProduction, relation, new ROComp_RequiresThing(asThing), "direct analysis");
                }

            }
            else if (reverseEngineerable is TerrainDef asTerrain)
            {
                if (asTerrain.IsSoil)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseSoil, relation, new ROComp_RequiresTerrain(asTerrain, altsMode), "direct analysis tr. (soil)");
                else if (asTerrain.BuildableByPlayer)
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFloor, relation, new ROComp_RequiresTerrain(asTerrain, altsMode), "direct analysis tr. (builable)");
                    //if (!isAlternate && relation == ResearchRelation.Direct && asTerrain.BuildableByPlayer)
                    //    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeTerrainConstruction, relation, new ROComp_RequiresTerrain(asTerrain), "direct analysis tr. (builable)");
                }
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseTerrain, relation, new ROComp_RequiresTerrain(asTerrain, altsMode), "direct analysis tr.");
            } 
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromPrototyping(ResearchProjectDef project, Def prototypeable, ResearchRelation relation)
        {
            if (prototypeable is RecipeDef asRecipe)
            {
                if (asRecipe.IsSurgery)
				{
					if (relation == ResearchRelation.Direct)
						yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeSurgery, relation, new ROComp_RequiresRecipe(asRecipe, AlternatesMode.EQUIVALENT), "prototype surgery");
				}
				else if (asRecipe.ProducedThingDef != null) //only non-null if produces exactly one product
				{
					if (relation == ResearchRelation.Direct)
						yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeProduction, relation, new ROComp_RequiresRecipe(asRecipe, AlternatesMode.EQUIVALENT), "prototype");
				}
			}
            else if (prototypeable is ThingDef asThing)
            {
                if (typeof(Plant).IsAssignableFrom(asThing.thingClass))
                    yield break;
                else if (!asThing.EverHaulable)
                {
                    if (!asThing.IsInstantBuild())
                    {
                        if (relation == ResearchRelation.Direct && asThing.BuildableByPlayer)
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeConstruction, relation, new ROComp_RequiresThing(asThing, AlternatesMode.EQUIVALENT), "prototype (unhaulable)");
                    }
                }

            }
            else if (prototypeable is TerrainDef asTerrain)
            {
                if (asTerrain.BuildableByPlayer)
                {
                    if (relation == ResearchRelation.Direct && asTerrain.BuildableByPlayer)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeTerrainConstruction, relation, new ROComp_RequiresTerrain(asTerrain, AlternatesMode.EQUIVALENT), "prototype tr. (builable)");
                }
            }
        }

        public OpportunityFactoryCollectionsSet FillCollections(ResearchProjectDef project)
        {
            OpportunityFactoryCollectionsSet collections = new OpportunityFactoryCollectionsSet();

            OF_Recipes.MakeFromRecipes(project, collections.GetSet(ResearchRelation.Direct));
            OF_AnalysisRequirements.MakeFromAnalysisRequirements(project, collections.GetSet(ResearchRelation.Direct));
            OF_Unlocks.MakeFromUnlocks(project, collections.GetSet(ResearchRelation.Direct));
            OF_Plants.MakeFromPlants(project, collections.GetSet(ResearchRelation.Direct));
            OF_CompProps.MakeFromFuel(project, collections.GetSet(ResearchRelation.Direct));

            {
                var prerequisites = new List<ResearchProjectDef>();
                if (project.prerequisites != null)
                    prerequisites.AddRange(project.prerequisites);
                if (project.hiddenPrerequisites != null)
                    prerequisites.AddRange(project.hiddenPrerequisites);

                foreach (var prerequisite in prerequisites)
                {
                    if (prerequisite == project)
                        continue;

                    //OF_Recipes.MakeFromRecipes(prerequisite, ResearchRelation.Ancestor, collections.GetSet(ResearchRelation.Ancestor));
                    OF_Unlocks.MakeFromUnlocks(prerequisite, collections.GetSet(ResearchRelation.Ancestor));
                }
            }

            /*{
                var descendants = new List<ResearchProjectDef>();
                foreach(var candidate in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(p => p != project && (p.prerequisites?.Contains(project) ?? false) || p.hiddenPrerequisites?.Contains(project) ?? false)))

                foreach (var prerequisite in prerequisites)
                {
                    if (prerequisite == project)
                        continue;

                    //OF_Recipes.MakeFromRecipes(prerequisite, ResearchRelation.Ancestor, collections.GetSet(ResearchRelation.Ancestor));
                    OF_Unlocks.MakeFromUnlocks(prerequisite, collections.GetSet(ResearchRelation.Ancestor));
                }
            }*/

            /*discard what we dont want to generate from*/
            collections.RemoveBlacklisted();

            collections.GetSet(ResearchRelation.Ancestor).forIngredientsAnalysis.Clear();
            collections.GetSet(ResearchRelation.Descendant).forIngredientsAnalysis.Clear();

            collections.GetSet(ResearchRelation.Ancestor).forFuelAnalysis.Clear();
            collections.GetSet(ResearchRelation.Descendant).forFuelAnalysis.Clear();

            collections.GetSet(ResearchRelation.Ancestor).forPrototyping.Clear();
            collections.GetSet(ResearchRelation.Descendant).forPrototyping.Clear();

            /*add handcrafted opportunities*/
            OF_Specials.MakeFromSpecials(project, collections);

            collections.RemoveDuplicates();
            collections.RemoveAlternatesWhereAppropriate();

            return collections;
        }

        
    }
}
