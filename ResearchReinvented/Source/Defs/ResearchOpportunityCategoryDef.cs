using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
    public class ResearchOpportunityRelationValues
    {
    }


    public class ResearchOpportunityCategoryDef : Def
    {
        /*public bool enabled = true;

        public float targetFractionMultiplier;
        public float targetIterations;
        public float extraFractionMultiplier;
        public bool infiniteOverflow;
        public float researchSpeedMultiplier;

        public FloatRange availableAtOverallProgress;*/

        public bool maxAsRemaining = false;

        private CategorySettingsFinal _settingsCached;
        public CategorySettingsFinal Settings { get 
            {
                if (_settingsCached == null)
                    _settingsCached = ResearchReinventedMod.Settings.GetCategorySettings(this);
                return _settingsCached; 
            } 
        }

        private List<ResearchOpportunityTypeDef> _opportunityTypes;
        public List<ResearchOpportunityTypeDef> OpportunityTypes
        {
            get
            {
                if (_opportunityTypes == null)
                {
                    _opportunityTypes = DefDatabase<ResearchOpportunityTypeDef>.AllDefs.Where(t => t.GetAllCategories().Contains(this)).ToList();

                }
                return _opportunityTypes;
            }
        }

        private bool? _usesChunkedResearch;
        public bool UsesChunkedResearch 
        {
            get 
            {
                if(!_usesChunkedResearch.HasValue)
                {
                    _usesChunkedResearch = false;
                    foreach (var opportunityType in OpportunityTypes)
                    {
                        if(opportunityType.UsesChunkedResearch)
                        {
                            _usesChunkedResearch = true;
                            break;
                        }
                    }
                }
                return _usesChunkedResearch.Value;
            }
        }

        public int priority;

        public Color color;

        public OpportunityAvailability GetCurrentAvailability(ResearchOpportunity asker){
            if(asker?.project == null)
            {
                Log.Error($"research opportunity {asker} in category {this} has null research project");
                return OpportunityAvailability.UnavailableReasonUnknown;
            }
            return GetCurrentAvailability(asker?.project);
        }

        public OpportunityAvailability GetCurrentAvailability(ResearchProjectDef project)
        {
            if (project == null)
                return OpportunityAvailability.UnavailableReasonUnknown;
            if (!Settings.enabled)
                return OpportunityAvailability.Disabled;
            if (project.ProgressPercent < Settings.availableAtOverallProgress.min)
                return OpportunityAvailability.ResearchTooLow;
            if (project.ProgressPercent > Settings.availableAtOverallProgress.max)
                return OpportunityAvailability.ResearchTooHigh;
            var totalsStore = ResearchOpportunityManager.Instance.GetTotalsStore(project, this);
            if (totalsStore == null)
                return OpportunityAvailability.UnavailableReasonUnknown;
            if (!Settings.infiniteOverflow && GetCurrentTotal() >= totalsStore.researchPoints)
                return OpportunityAvailability.CategoryFinished;
            return OpportunityAvailability.Available;
        }

        public float GetCurrentTotal() 
        {
            var matchingOpportunities = ResearchOpportunityManager.Instance.CurrentProjectOpportunities.Where(o => o.IsValid() && o.def.GetCategory(o.relation) == this);
            var totalProgress = matchingOpportunities.Sum(o => o.Progress);
            return totalProgress;
        }

        public override void ClearCachedData()
        {
            base.ClearCachedData();
            _settingsCached = null;
        }
    }
}
