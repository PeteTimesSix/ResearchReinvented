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
        private List<ResearchOpportunityCategoryTotalsStore> _categoryStores = new List<ResearchOpportunityCategoryTotalsStore>();


        public IEnumerable<ResearchOpportunity> GetCurrentlyAvailableOpportunities(bool includeFinished = false)
        {
            if(!includeFinished)
                return AllCurrentOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available);
            else
                return AllCurrentOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available || o.CurrentAvailability == OpportunityAvailability.Finished);

        }

        internal ResearchOpportunityCategoryTotalsStore GetTotalsStore(ResearchProjectDef project, ResearchOpportunityCategoryDef category)
        {
            return _categoryStores.FirstOrDefault(cs => cs.project == project && cs.category == category);
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
                        _jobToOpportunityMap.Clear();
                        GenerateOpportunities(Find.ResearchManager.currentProj, false);
                    }
                    return _currentOpportunities;
                }
                else
                    return new List<ResearchOpportunity>();
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
            Scribe_Collections.Look(ref _categoryStores, "_categoryStores", LookMode.Deep);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (_jobToOpportunityMap == null)
                _jobToOpportunityMap = new Dictionary<int, ResearchOpportunity>();
            if (_categoryStores == null)
                _categoryStores = new List<ResearchOpportunityCategoryTotalsStore>();
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
        }

        // mahenry00 - Not sure this is the right spot to put this.
        public override void LoadedGame()
        {
            base.LoadedGame();
            _allGeneratedOpportunities = _allGeneratedOpportunities
            .Where(o =>
            {
                if (!o.IsValid)
                    Log.Warning($"[RR]: Research opportunity invalid, loadID: {o.loadID}, project: {o.project.label}");

                return o.IsValid;
            })
            .ToList();
        }

        public void FinishProject(ResearchProjectDef project, bool doCompletionDialog = false, Pawn researcher = null)
        {
            Find.ResearchManager.FinishProject(project, doCompletionDialog, researcher);
        }
        
        public void GenerateOpportunities(ResearchProjectDef project, bool forceRegen)
        {
            _currentProject = project;
            _currentOpportunities = new List<ResearchOpportunity>();
            if (project == null)
                return;

            if (_allProjectsWithGeneratedOpportunities.Contains(project)) 
            {
                if (!forceRegen)
                {
                    var matches = _allGeneratedOpportunities.Where(o => o.project == project);
                    _currentOpportunities.Clear();
                    _currentOpportunities.AddRange(matches);
                    return;
                }
                else
                {
                    var matches = _allGeneratedOpportunities.Where(o => o.project == project).ToList();
                    _currentOpportunities.Clear();
                    foreach(var match in matches)
                        _allGeneratedOpportunities.Remove(match);
                }
            }

            var results = ResearchOpportunityPrefabs.MakeOpportunitiesForProject(project);
            var newOpportunities = results.opportunities;
            var categoryStores = results.categoryStores;
            _categoryStores.RemoveAll(cs => cs.project == project);
            _categoryStores.AddRange(categoryStores);
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
