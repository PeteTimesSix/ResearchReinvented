using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public class ResearchOpportunityManager : GameComponent
    {
        public static ResearchOpportunityManager instance => Current.Game.GetComponent<ResearchOpportunityManager>();

        private List<ResearchOpportunity> _currentOpportunities = new List<ResearchOpportunity>();

        private List<ResearchOpportunity> _allGeneratedOpportunities = new List<ResearchOpportunity>();
        private List<ResearchProjectDef> _allProjectsWithGeneratedOpportunities = new List<ResearchProjectDef>();


        private ResearchProjectDef _currentProject;
        public ResearchProjectDef CurrentProject { get { return _currentProject; } }

        private Dictionary<int, ResearchOpportunity> _jobToOpportunityMap = new Dictionary<int, ResearchOpportunity>();

        private static readonly List<ResearchOpportunity> ListEmpty = new List<ResearchOpportunity>();

        public IEnumerable<ResearchOpportunity> GetCurrentlyAvailableOpportunities(bool includeFinished = false)
        {
            if(!includeFinished)
                return AllCurrentOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available);
            else
                return AllCurrentOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available || o.CurrentAvailability == OpportunityAvailability.Finished);

        }

        public IReadOnlyCollection<ResearchOpportunity> AllCurrentOpportunities
        {
            get 
            {
                if (Find.ResearchManager.currentProj != null)
                {
                    if (Find.ResearchManager.currentProj != _currentProject || _currentOpportunities == null) 
                    {
                        _currentOpportunities.Clear();
                        GenerateOpportunities(Find.ResearchManager.currentProj);
                    }
                    return _currentOpportunities;
                }
                else
                    return ListEmpty;
            }
        }

        public IReadOnlyCollection<ResearchOpportunityCategoryDef> AllCurrentOpportunityCategories
        {
            get
            {
                HashSet<ResearchOpportunityCategoryDef> categories = new HashSet<ResearchOpportunityCategoryDef>();
                foreach(var opportunity in AllCurrentOpportunities) 
                {
                    foreach(var category in opportunity.def.GetAllCategories())
                    {
                        if (category == null)
                            Log.Warning("got null category from " + opportunity.def.defName);
                        if (!categories.Contains(category))
                            categories.Add(category);
                    }
                }
                return categories;
            }
        }

        public ResearchOpportunityManager(Game game)
        {
        }

        private List<int> wList1;
        private List<ResearchOpportunity> wList2;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _allGeneratedOpportunities, "_allGeneratedOpportunities", LookMode.Deep);
            Scribe_Collections.Look(ref _currentOpportunities, "_currentOpportunities", LookMode.Reference);
            Scribe_Collections.Look(ref _jobToOpportunityMap, "_jobToOpportunityMap", LookMode.Value, LookMode.Reference, ref wList1, ref wList2);
            Scribe_Collections.Look(ref _allProjectsWithGeneratedOpportunities, "_allProjectsWithGeneratedOpportunities", LookMode.Def);
            Scribe_Defs.Look(ref _currentProject, "currentProject");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (_jobToOpportunityMap == null)
                _jobToOpportunityMap = new Dictionary<int, ResearchOpportunity>();
            //if (_pawnToOpportunityMap == null)
            //    _pawnToOpportunityMap = new Dictionary<Pawn, ResearchOpportunity>();
            //ResearchOpportunityPrefabs.GenerateAllImplicitOpportunities();
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
        }

        internal void FinishProject(ResearchProjectDef project, bool doCompletionDialog = false, Pawn researcher = null)
        {
            Find.ResearchManager.FinishProject(project, doCompletionDialog, researcher);
        }

        private void GenerateOpportunities(ResearchProjectDef project)
        {
            _currentProject = project;
            _currentOpportunities = new List<ResearchOpportunity>();
            if (project == null)
                return;

            if (_allProjectsWithGeneratedOpportunities.Contains(project)) 
            {
                var matches = _allGeneratedOpportunities.Where(o => o.project == project);
                _currentOpportunities.Clear();
                _currentOpportunities.AddRange(matches);
            }
            else 
            {
                var newOpportunities = ResearchOpportunityPrefabs.MakeOpportunitiesForProject(project);
                _currentOpportunities.Clear();
                _currentOpportunities.AddRange(newOpportunities);
                _allGeneratedOpportunities.AddRange(newOpportunities);
                _allProjectsWithGeneratedOpportunities.Add(project);

                if (ResearchReinventedMod.Settings.debugPrintouts)
                {
                    Debug.LogMessage($"Listing generated opportunities for current project {_currentProject.label}...");
                    foreach (var opportunity in newOpportunities)
                    {
                        Debug.LogMessage($" |-- {opportunity.ShortDesc}");
                    }
                }
            }
        }

        public void AssociateJobWithOpportunity(Pawn pawn, Job job, ResearchOpportunity opportunity)
        {
            if (opportunity != null)
                _jobToOpportunityMap[job.loadID] = opportunity;
            else
                Log.Warning($"attempted to associate job {job} for {pawn} with null opportunity");
            //Log.Message($"pawn {pawn} associated job {job} (load id: {job.GetUniqueLoadID()}) with opportunity {opportunity} (load id: {opportunity.GetUniqueLoadID()})");
        }

        public void ClearAssociatedJobWithOpportunity(Pawn pawn, Job job)
        {
            if (_jobToOpportunityMap.ContainsKey(job.loadID))
                _jobToOpportunityMap.Remove(job.loadID);
            //Log.Message($"pawn {pawn} cleared associated job {job} (load id: {job.GetUniqueLoadID()})");
        }

        public ResearchOpportunity GetOpportunityForJob(Job job)
        {
            if (_jobToOpportunityMap.TryGetValue(job.loadID, out ResearchOpportunity result))
                return result;
            else
                return null;
        }

        /*public float GetCategoryProgressFraction(ResearchOpportunityCategoryDef category)
        {
            category.targetFractionMultiplier 
        }*/
    }
}
