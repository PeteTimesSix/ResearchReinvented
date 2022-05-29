using PeteTimesSix.ResearchReinvented.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class ResearchOpportunityTypeDefOf
    {

        public static ResearchOpportunityTypeDef Analyse;
        public static ResearchOpportunityTypeDef AnalyseRawFood;
        public static ResearchOpportunityTypeDef AnalyseFood;
        public static ResearchOpportunityTypeDef AnalyseDrug;
        public static ResearchOpportunityTypeDef AnalyseHarvestProduct;

        public static ResearchOpportunityTypeDef AnalyseIngredients;
        public static ResearchOpportunityTypeDef AnalyseIngredientsDrug;
        public static ResearchOpportunityTypeDef AnalyseIngredientsFood;

        public static ResearchOpportunityTypeDef AnalyseFuelFood;
        public static ResearchOpportunityTypeDef AnalyseFuelDrug;
        public static ResearchOpportunityTypeDef AnalyseFuelFlammable;
        public static ResearchOpportunityTypeDef AnalyseFuel;

        public static ResearchOpportunityTypeDef AnalyseInPlace;
        public static ResearchOpportunityTypeDef AnalyseProductionFacility;
        public static ResearchOpportunityTypeDef AnalysePlant;

        public static ResearchOpportunityTypeDef AnalyseTerrain;
        public static ResearchOpportunityTypeDef AnalyseSoil;
        public static ResearchOpportunityTypeDef AnalyseFloor;

        public static ResearchOpportunityTypeDef BasicResearch;


        static ResearchOpportunityTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchOpportunityTypeDefOf));
        }
    }
}
