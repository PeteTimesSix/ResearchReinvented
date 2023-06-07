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

        public static ResearchOpportunityTypeDef BasicResearch;

        public static ResearchOpportunityTypeDef GainFactionKnowledge;

        public static ResearchOpportunityTypeDef Analyse;
        public static ResearchOpportunityTypeDef AnalysePawn;
        public static ResearchOpportunityTypeDef AnalysePawnNonFlesh;
        public static ResearchOpportunityTypeDef AnalyseDissect;
        public static ResearchOpportunityTypeDef AnalyseDissectNonFlesh;
        public static ResearchOpportunityTypeDef AnalyseRawFood;
        public static ResearchOpportunityTypeDef AnalyseFood;
        public static ResearchOpportunityTypeDef AnalyseMedicine;
        public static ResearchOpportunityTypeDef AnalyseDrug;
        public static ResearchOpportunityTypeDef AnalyseHarvestProduct;

        public static ResearchOpportunityTypeDef AnalyseIngredients;
        //public static ResearchOpportunityTypeDef AnalyseIngredientsDrug;
        public static ResearchOpportunityTypeDef AnalyseIngredientsFood;

        public static ResearchOpportunityTypeDef AnalyseFuelFood;
        public static ResearchOpportunityTypeDef AnalyseFuelDrug;
        public static ResearchOpportunityTypeDef AnalyseFuelFlammable;
        public static ResearchOpportunityTypeDef AnalyseFuel;

        public static ResearchOpportunityTypeDef AnalyseProductionFacility;
        public static ResearchOpportunityTypeDef AnalysePlant;

        public static ResearchOpportunityTypeDef AnalyseTerrain;
        public static ResearchOpportunityTypeDef AnalyseSoil;
        public static ResearchOpportunityTypeDef AnalyseFloor;

        public static ResearchOpportunityTypeDef PrototypeConstruction;
        public static ResearchOpportunityTypeDef PrototypeTerrainConstruction;
        public static ResearchOpportunityTypeDef PrototypeProduction;
		public static ResearchOpportunityTypeDef PrototypeSurgery;

		public static ResearchOpportunityTypeDef TrialDrug;
        static ResearchOpportunityTypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchOpportunityTypeDefOf));
        }

    }
}
