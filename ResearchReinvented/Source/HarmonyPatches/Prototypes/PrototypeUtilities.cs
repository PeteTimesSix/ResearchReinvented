using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    public static class PrototypeUtilities
    {
        public static float EXPERIMENTAL_SURGERY_SUCCESS_MODIFIER = 0.65f;
        public static float EXPERIMENTAL_SURGERY_MAX_SUCCESS_CHANCE = 0.85f;

        private static float PROTOTYPE_QUALITY_MULTIPLIER = 0.3f;
        private static float PROTOTYPE_HEALTH_MULTIPLIER = 0.3f;

        public static float PROTOTYPE_WORK_MULTIPLIER = 3.0f;
        public static float PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER = 0.75f;
        public static float PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER = 0.95f;

        private static ResearchProjectDef cacheBuiltForProject = null;
        private static ResearchOpportunity[] _prototypeOpportunitiesCache = Array.Empty<ResearchOpportunity>();

        public static IEnumerable<ResearchOpportunity> PrototypeOpportunities
        {
            get 
			{
				if (cacheBuiltForProject != Find.ResearchManager.currentProj)
				{
                    _prototypeOpportunitiesCache = ResearchOpportunityManager.Instance
                        .GetFilteredOpportunities(null, HandlingMode.Special_Prototype).ToArray();
					cacheBuiltForProject = Find.ResearchManager.currentProj;
				}
				return _prototypeOpportunitiesCache;
			}
		}

        public static void ClearPrototypeOpportunityCache()
        {
            cacheBuiltForProject = null;
            _prototypeOpportunitiesCache = Array.Empty<ResearchOpportunity>();
        }


        public static void DoPrototypeHealthDecrease(Thing product, RecipeDef usedRecipe)
        {
            product.HitPoints = (int)Math.Max(1, (product.HitPoints * PROTOTYPE_HEALTH_MULTIPLIER));
        }

        public static void DoPrototypeBadComps(Thing product, RecipeDef usedRecipe)
        {
            {
                var breakDownComp = product.TryGetComp<CompBreakdownable>();
                if (breakDownComp != null)
                {
                    breakDownComp.DoBreakdown();
                }
                var refuelComp = product.TryGetComp<CompRefuelable>();
                if (refuelComp != null)
                {
                    if (!(refuelComp.props as CompProperties_Refuelable).destroyOnNoFuel)
                        refuelComp.ConsumeFuel(refuelComp.Fuel * PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER);
                    else
                        refuelComp.ConsumeFuel(refuelComp.Fuel * PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER);
                }
            }
        }

        public static QualityCategory DoPrototypeQualityDecreaseThing(QualityCategory category, Pawn worker, Thing product, RecipeDef usedRecipe)
        {
            bool isPrototype = product.def.IsAvailableOnlyForPrototyping() || (usedRecipe != null && usedRecipe.IsAvailableOnlyForPrototyping());
            if (isPrototype)
            {
                byte asByte = (byte)category;
                var adjusted = (QualityCategory)Math.Max((byte)0, (byte)Math.Round((float)asByte * PROTOTYPE_QUALITY_MULTIPLIER));
                //Log.Message($"adjusted quality for product {product} (worker {worker}): {category} to {adjusted}");
                return adjusted;
            }
            return category;
        }

        public static QualityCategory DoPrototypeQualityDecreaseRecipe(QualityCategory category, Pawn worker, Thing product, RecipeDef usedRecipe)
        {
            bool isPrototype = product.def.IsAvailableOnlyForPrototyping() || (usedRecipe != null && usedRecipe.IsAvailableOnlyForPrototyping());
            if (isPrototype)
            {
                byte asByte = (byte)category;
                var adjusted = (QualityCategory)Math.Max((byte)0, (byte)Math.Round((float)asByte * PROTOTYPE_QUALITY_MULTIPLIER));
                //Log.Message($"adjusted quality for product {product} (recipe: {recipe} worker {worker}): {category} to {adjusted}");
                return adjusted;
            }
            return category;
        }

        public static void DoPostFailToFinishThingResearch(Pawn worker, float totalWork, float doneWork, ThingDef productDef, RecipeDef usedRecipe)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, (op) => op.requirement.MetBy(usedRecipe) || op.requirement.MetBy(productDef));
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (o.requirement.MetBy(usedRecipe) || o.requirement.MetBy(productDef)))
                    //.FirstOrDefault();

                if (opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {worker.LabelCap} finished (failure) thing {productDef.LabelCap} {(usedRecipe != null ? usedRecipe.LabelCap.ToString() : "")} and there's an opportunity {opportunity.ShortDesc}");

                    //Log.Message($"found matching opp. {opportunity.ShortDesc}");
                    var amount = doneWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    var xp = doneWork * ResearchXPAmounts.DoneWorkMultiplier;
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: usedRecipe != null ? usedRecipe.LabelCap.ToString() : productDef.LabelCap.ToString());
                }
            }
        }

        public static void DoPostFailToFinishTerrainResearch(Pawn worker, float totalWork, float doneWork, TerrainDef terrainDef)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, terrainDef);
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(terrainDef))
                    //.FirstOrDefault();

                if (opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {worker.LabelCap} finished (failure) terrain {terrainDef.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                    var amount = doneWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    var xp = doneWork * ResearchXPAmounts.DoneWorkMultiplier;
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: terrainDef.LabelCap);
                }
            }
        }

        public static void DoPostFinishThingResearch(Pawn worker, float totalWork, Thing product, RecipeDef usedRecipe)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, (op) => op.requirement.MetBy(usedRecipe) || op.requirement.MetBy(product.def));
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (o.requirement.MetBy(usedRecipe) || o.requirement.MetBy(product.def)))
                    //.FirstOrDefault();

                if (opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {worker.LabelCap} finished thing {product.LabelCap} {(usedRecipe != null ? usedRecipe.LabelCap.ToString() : "")} and there's an opportunity {opportunity.ShortDesc}");

                    //Log.Message($"found matching opp. {opportunity.ShortDesc}");
                    var amount = totalWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    var xp = totalWork * ResearchXPAmounts.DoneWorkMultiplier;
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: usedRecipe != null ? usedRecipe.LabelCap.ToString() : product.LabelCapNoCount); 
                }
            }
        }

        public static void DoPostFinishTerrainResearch(Pawn worker, float totalWork, TerrainDef terrainDef)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, terrainDef);
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(terrainDef))
                    //.FirstOrDefault();

                if (opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {worker.LabelCap} finished terrain {terrainDef.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                    var amount = totalWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    var xp = totalWork * ResearchXPAmounts.DoneWorkMultiplier;
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: terrainDef.LabelCap);
                }
            }
        }

        public static void DoPostFinishSurgeryResearch(Pawn target, Pawn worker, float totalWork, RecipeDef usedRecipe)
        {
            if(usedRecipe != null)
            {
                var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, usedRecipe);
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (usedRecipe != null && o.requirement.MetBy(usedRecipe)))
                    //.FirstOrDefault();

                if (opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {worker.LabelCap} finished surgery {usedRecipe.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                    var amount = totalWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    var xp = totalWork * ResearchXPAmounts.DoneWorkMultiplier;
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: usedRecipe.LabelCap.ToString());
                }
            }
        }
    }
}
