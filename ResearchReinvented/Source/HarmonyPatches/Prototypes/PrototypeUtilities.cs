using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.AccessTools;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    public static class PrototypeUtilities
    {
        public static float EXPERIMENTAL_SURGERY_SUCCESS_MODIFIER = 0.65f;
        public static float EXPERIMENTAL_SURGERY_MAX_SUCCESS_CHANCE = 0.9f;

        private static float PROTOTYPE_QUALITY_MULTIPLIER_MIN = 0.3f;
        private static float PROTOTYPE_QUALITY_MULTIPLIER_MAX = 0.8f;

        private static float PROTOTYPE_HEALTH_MULTIPLIER_MIN = 0.4f;
        private static float PROTOTYPE_HEALTH_MULTIPLIER_MAX = 0.9f;

        public static float PROTOTYPE_WORK_MULTIPLIER = 2.0f;

        private static float PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER_MIN = 0.4f;
        private static float PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER_MAX = 0.8f;

        private static float PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MIN = 0.1f;
        private static float PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MAX = 0.5f;

        private static ResearchOpportunity[] _prototypeOpportunitiesCache = null;

        private static FieldRef<CompApparelVerbOwner_Charged, int> CompApparelVerbOwner_Charged_remainingCharges;

        static PrototypeUtilities()
        {
            CompApparelVerbOwner_Charged_remainingCharges = AccessTools.FieldRefAccess<int>(typeof(CompApparelVerbOwner_Charged), "remainingCharges");
        }

        public static IEnumerable<ResearchOpportunity> PrototypeOpportunities
        {
            get
            {
                if (_prototypeOpportunitiesCache == null)
                {
                    _prototypeOpportunitiesCache = ResearchOpportunityManager.Instance
                        .GetFilteredOpportunitiesOfAll(null, HandlingMode.Special_Prototype).ToArray();
                }
                return _prototypeOpportunitiesCache;
			}
		}

        public static void ClearPrototypeOpportunityCache()
        {
            _prototypeOpportunitiesCache = null;
            RecipeDefExtensions.ClearPrototypeOpportunityCache();
            BuildableDefExtensions.ClearPrototypeOpportunityCache();
            ThingDefExtensions.ClearPrototypeOpportunityCache();
        }


        public static void DoPrototypeHealthDecrease(Thing product, RecipeDef usedRecipe)
        {
            var mult = Rand.Range(PROTOTYPE_HEALTH_MULTIPLIER_MIN, PROTOTYPE_HEALTH_MULTIPLIER_MAX);
            if (ResearchReinvented_Debug.debugPrintouts)
                Log.Message($"Prototype health multiplier: {mult}");
            product.HitPoints = (int)Math.Max(1, (product.HitPoints * mult));
        }

        public static void DoPrototypeBadComps(Thing product, RecipeDef usedRecipe)
        {
            {
                var breakDownComp = product.TryGetComp<CompBreakdownable>();
                if (breakDownComp != null)
                {
                    breakDownComp.DoBreakdown();
                }
            }
            {
                var reloadComp1 = product.TryGetComp<CompApparelVerbOwner_Charged>();
                if(reloadComp1 != null)
                {
                    if (reloadComp1.Props.destroyOnEmpty)
                    {
                        var mult = Rand.Range(PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER_MIN, PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER_MAX);
                        if (ResearchReinvented_Debug.debugPrintouts)
                            Log.Message($"Prototype disposable verbOwner fuel multiplier: {mult}");
                        CompApparelVerbOwner_Charged_remainingCharges(reloadComp1) = Math.Max(1, (int)Math.Round(CompApparelVerbOwner_Charged_remainingCharges(reloadComp1) * mult));
                    }
                    else
                    {
                        var mult = Rand.Range(PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MIN, PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MAX);
                        if (ResearchReinvented_Debug.debugPrintouts)
                            Log.Message($"Prototype refuelable verbOwner fuel multiplier: {mult}");
                        CompApparelVerbOwner_Charged_remainingCharges(reloadComp1) = Math.Max(1, (int)Math.Round(CompApparelVerbOwner_Charged_remainingCharges(reloadComp1) * mult));
                    }
                }
            }
            {
                var reloadComp2 = product.TryGetComp<CompEquippableAbilityReloadable>();
                if(reloadComp2 != null)
                {
                    var mult = Rand.Range(PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MIN, PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MAX);
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"Prototype refuelable apparel fuel multiplier: {mult}");
                    reloadComp2.RemainingCharges = Math.Max(1, (int)Math.Round(reloadComp2.RemainingCharges * mult));
                }
            }
            {
                var refuelComp = product.TryGetComp<CompRefuelable>();
                if (refuelComp != null)
                {
                    if (refuelComp.Props.destroyOnNoFuel)
                    {
                        var mult = Rand.Range(PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER_MIN, PROTOTYPE_FUEL_DISPOSABLE_MULTIPLIER_MAX);
                        if (ResearchReinvented_Debug.debugPrintouts)
                            Log.Message($"Prototype disposable fuel multiplier: {mult}");
                        refuelComp.ConsumeFuel(refuelComp.Fuel * (1f - mult));
                    }
                    else
                    {
                        var mult = Rand.Range(PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MIN, PROTOTYPE_FUEL_REFUELABLE_MULTIPLIER_MAX);
                        if (ResearchReinvented_Debug.debugPrintouts)
                            Log.Message($"Prototype refuelable fuel multiplier: {mult}");
                        refuelComp.ConsumeFuel(refuelComp.Fuel * (1f - mult));
                    }
                }
            }
                
        }

        public static QualityCategory DoPrototypeQualityDecreaseThing(QualityCategory category, Pawn worker, Thing product, RecipeDef usedRecipe)
        {
            bool isPrototype = product.def.IsAvailableOnlyForPrototyping() || (usedRecipe != null && usedRecipe.IsAvailableOnlyForPrototyping());
            if (isPrototype)
            {
                byte asByte = (byte)category;
                var mult = Rand.Range(PROTOTYPE_QUALITY_MULTIPLIER_MIN, PROTOTYPE_QUALITY_MULTIPLIER_MAX);
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"Prototype quality multiplier: {mult}");
                var adjusted = (QualityCategory)Math.Max((byte)0, (byte)Math.Round((float)asByte * mult));
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
                var mult = Rand.Range(PROTOTYPE_QUALITY_MULTIPLIER_MIN, PROTOTYPE_QUALITY_MULTIPLIER_MAX);
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"Prototype quality multiplier: {mult}");
                var adjusted = (QualityCategory)Math.Max((byte)0, (byte)Math.Round((float)asByte * mult));
                //Log.Message($"adjusted quality for product {product} (recipe: {recipe} worker {worker}): {category} to {adjusted}");
                return adjusted;
            }
            return category;
        }

        public static void DoPostFailToFinishThingResearch(Pawn worker, float totalWork, float doneWork, ThingDef productDef, RecipeDef usedRecipe)
        {
            if (!worker.CanNowDoResearch())
                return;

            var opportunity = PrototypeOpportunities.Where(op => op.requirement.MetBy(usedRecipe) || op.requirement.MetBy(productDef)).FirstOrDefault();
            //var opportunity = ResearchOpportunityManager.Instance.GetFilteredOpportunitiesOfAll(OpportunityAvailability.Available, HandlingMode.Special_Prototype, (op) => op.requirement.MetBy(usedRecipe) || op.requirement.MetBy(productDef));
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (o.requirement.MetBy(usedRecipe) || o.requirement.MetBy(productDef)))
                //.FirstOrDefault();

            if (opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {worker.LabelCap} finished (failure) thing {productDef.LabelCap} {(usedRecipe != null ? usedRecipe.LabelCap.ToString() : "")} and there's an opportunity {opportunity.ShortDesc}");

                //make sure the number isnt completely insigificant
                doneWork = Math.Max(doneWork, totalWork / 5);
                doneWork = Math.Max(doneWork, 20 * 60);
                float amount = (float)(doneWork * BaseResearchAmounts.DoneWorkMultiplier);
                var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = doneWork * ResearchXPAmounts.DoneWorkMultiplier;
                opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: usedRecipe != null ? usedRecipe.LabelCap.ToString() : productDef.LabelCap.ToString());
            }
        }

        public static void DoPostFailToFinishTerrainResearch(Pawn worker, float totalWork, float doneWork, TerrainDef terrainDef)
        {
            if (!worker.CanNowDoResearch())
                return;

            var opportunity = PrototypeOpportunities.Where(op => op.requirement.MetBy(terrainDef)).FirstOrDefault();
            //var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, terrainDef);
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(terrainDef))
                //.FirstOrDefault();

            if (opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {worker.LabelCap} finished (failure) terrain {terrainDef.LabelCap} and there's an opportunity {opportunity.ShortDesc}");
                
                //make sure the number isnt completely insigificant
                doneWork = Math.Max(doneWork, totalWork / 5);
                doneWork = Math.Max(doneWork, 20 * 60);
                float amount = (float)(doneWork * BaseResearchAmounts.DoneWorkMultiplier);
                var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = doneWork * ResearchXPAmounts.DoneWorkMultiplier;
                opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: terrainDef.LabelCap);
            }
        }

        public static void DoPostFinishThingResearch(Pawn worker, float totalWork, Thing product, RecipeDef usedRecipe)
        {
            if (!worker.CanNowDoResearch())
                return;

            var opportunity = PrototypeOpportunities.Where(op => op.requirement.MetBy(usedRecipe) || op.requirement.MetBy(product.def)).FirstOrDefault();
            //var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, (op) => op.requirement.MetBy(usedRecipe) || op.requirement.MetBy(product.def));
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (o.requirement.MetBy(usedRecipe) || o.requirement.MetBy(product.def)))
                //.FirstOrDefault();

            if (opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {worker.LabelCap} finished thing {product.LabelCap} {(usedRecipe != null ? usedRecipe.LabelCap.ToString() : "")} and there's an opportunity {opportunity.ShortDesc}");
                
                //make sure the number isnt completely insigificant
                totalWork = Math.Max(totalWork, 20 * 60);
                float amount = (float)(totalWork * BaseResearchAmounts.DoneWorkMultiplier);
                var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = totalWork * ResearchXPAmounts.DoneWorkMultiplier;
                opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: usedRecipe != null ? usedRecipe.LabelCap.ToString() : product.LabelCapNoCount); 
            }
        }

        public static void DoPostFinishTerrainResearch(Pawn worker, float totalWork, TerrainDef terrainDef)
        {
            if (!worker.CanNowDoResearch())
                return;
            
            var opportunity = PrototypeOpportunities.Where(op => op.requirement.MetBy(terrainDef)).FirstOrDefault();
            //var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, terrainDef);
                //.GetCurrentlyAvailableOpportunities()
                //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(terrainDef))
                //.FirstOrDefault();

            if (opportunity != null)
            {
                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"pawn {worker.LabelCap} finished terrain {terrainDef.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                //make sure the number isnt completely insigificant
                totalWork = Math.Max(totalWork, 20 * 60);
                float amount = (float)(totalWork * BaseResearchAmounts.DoneWorkMultiplier);
                var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                var xp = totalWork * ResearchXPAmounts.DoneWorkMultiplier;
                opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: terrainDef.LabelCap);
            }
        }

        public static void DoPostFinishSurgeryResearch(Pawn target, Pawn worker, float totalWork, RecipeDef usedRecipe)
        {
            if (!worker.CanNowDoResearch())
                return;

            if (usedRecipe != null)
            {
                var opportunity = PrototypeOpportunities.Where(op => op.requirement.MetBy(usedRecipe)).FirstOrDefault();
                //var opportunity = ResearchOpportunityManager.Instance.GetFirstFilteredOpportunity(OpportunityAvailability.Available, HandlingMode.Special_Prototype, usedRecipe);
                    //.GetCurrentlyAvailableOpportunities()
                    //.Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (usedRecipe != null && o.requirement.MetBy(usedRecipe)))
                    //.FirstOrDefault();

                if (opportunity != null)
                {
                    if (ResearchReinvented_Debug.debugPrintouts)
                        Log.Message($"pawn {worker.LabelCap} finished surgery {usedRecipe.LabelCap} and there's an opportunity {opportunity.ShortDesc}");

                    //make sure the number isnt completely insigificant
                    totalWork = Math.Max(totalWork, 20 * 60);
                    float amount = (float)(totalWork * BaseResearchAmounts.DoneWorkMultiplier);
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    var xp = totalWork * ResearchXPAmounts.DoneWorkMultiplier;
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, xp, moteSubjectName: usedRecipe.LabelCap.ToString());
                }
            }
        }
    }
}
