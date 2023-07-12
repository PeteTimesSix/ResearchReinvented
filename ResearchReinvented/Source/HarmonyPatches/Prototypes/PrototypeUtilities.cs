using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
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
                var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                    .Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(productDef))
                    .FirstOrDefault();

                if (opportunity != null)
                {
                    //Log.Message($"found matching opp. {opportunity.ShortDesc}");
                    var amount = doneWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, SkillDefOf.Intellectual, moteSubjectName: usedRecipe != null ? usedRecipe.LabelCap.ToString() : productDef.LabelCap.ToString());
                    if (worker?.skills != null)
                    {
                        var xp = 0.1f * totalWork;
                        worker.skills.Learn(SkillDefOf.Intellectual, xp, false);
                    }
                }
            }
        }

        public static void DoPostFailToFinishTerrainResearch(Pawn worker, float totalWork, float doneWork, TerrainDef terrainDef)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                    .Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(terrainDef))
                    .FirstOrDefault();

                if (opportunity != null)
                {
                    var amount = doneWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, SkillDefOf.Intellectual, moteSubjectName: terrainDef.LabelCap);
                    if (worker?.skills != null)
                    {
                        var xp = 0.1f * totalWork;
                        worker.skills.Learn(SkillDefOf.Intellectual, xp, false);
                    }
                }
            }
        }

        public static void DoPostFinishThingResearch(Pawn worker, float totalWork, Thing product, RecipeDef usedRecipe)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                    .Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(product.def))
                    .FirstOrDefault();

                if (opportunity != null)
                {
                    //Log.Message($"found matching opp. {opportunity.ShortDesc}");
                    var amount = totalWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, SkillDefOf.Intellectual, moteSubjectName: usedRecipe != null ? usedRecipe.LabelCap.ToString() : product.LabelCapNoCount); 
                    if (worker?.skills != null)
                    {
                        var xp = 0.1f * totalWork;
                        worker.skills.Learn(SkillDefOf.Intellectual, xp, false);
                    }
                }
            }
        }

        public static void DoPostFinishTerrainResearch(Pawn worker, float totalWork, TerrainDef terrainDef)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                    .Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && o.requirement.MetBy(terrainDef))
                    .FirstOrDefault();

                if (opportunity != null)
                {
                    var amount = totalWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, SkillDefOf.Intellectual, moteSubjectName: terrainDef.LabelCap);
                    if (worker?.skills != null)
                    {
                        var xp = 0.1f * totalWork;
                        worker.skills.Learn(SkillDefOf.Intellectual, xp, false);
                    }
                }
            }
        }

        public static void DoPostFinishSurgeryResearch(Pawn target, Pawn worker, float totalWork, RecipeDef usedRecipe)
        {
            {
                var opportunity = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities()
                    .Where(o => o.def.handledBy.HasFlag(HandlingMode.Special_Prototype) && (usedRecipe != null && o.requirement.MetBy(usedRecipe)))
                    .FirstOrDefault();

                if (opportunity != null)
                {
                    var amount = totalWork * BaseResearchAmounts.DoneWorkMultiplier;
                    var modifier = worker.GetStatValue(StatDefOf.ResearchSpeed, true);
                    opportunity.ResearchChunkPerformed(worker, HandlingMode.Special_Prototype, amount, modifier, SkillDefOf.Intellectual, moteSubjectName: usedRecipe.LabelCap.ToString());
                    if (worker?.skills != null)
                    {
                        var xp = 0.1f * totalWork;
                        worker.skills.Learn(SkillDefOf.Intellectual, xp, false);
                    }
                }
            }
        }
    }
}
