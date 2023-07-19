using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public class ResearchOpportunityManager : GameComponent
    {
        public static ResearchOpportunityManager Instance => Current.Game.GetComponent<ResearchOpportunityManager>();


        private List<ResearchOpportunity> _allGeneratedOpportunities = new List<ResearchOpportunity>();


        public IReadOnlyCollection<ResearchOpportunity> AllGeneratedOpportunities => _allGeneratedOpportunities.AsReadOnly();

        private ResearchProjectDef _currentProject;
        public ResearchProjectDef CurrentProject => _currentProject;

        private List<ResearchOpportunity> _currentProjectOpportunitiesCache;
        public IReadOnlyCollection<ResearchOpportunity> CurrentProjectOpportunities
        {
            get
            {
                bool currentProjectRegenned = CheckForRegeneration();
                if(currentProjectRegenned || _currentProjectOpportunitiesCache == null) 
                {
                    _currentProjectOpportunitiesCache = AllGeneratedOpportunities.Where(o => o.IsValid() && o.project == _currentProject).ToList();
                }
                return _currentProjectOpportunitiesCache.AsReadOnly();
            }
        }
        private HashSet<ResearchOpportunityCategoryDef> _currentOpportunityCategoriesCache;
        public IReadOnlyCollection<ResearchOpportunityCategoryDef> CurrentProjectOpportunityCategories
        {
            get
            {
                bool currentProjectRegenned = CheckForRegeneration();
                if (currentProjectRegenned || _currentOpportunityCategoriesCache == null)
                {
                    _currentOpportunityCategoriesCache = CurrentProjectOpportunities.Select(o => o.def.GetCategory(o.relation)).ToHashSet();
                }
                return _currentOpportunityCategoriesCache.ToList().AsReadOnly();
            }
        }
        public HashSet<ResearchProjectDef> _projectsGenerated = new HashSet<ResearchProjectDef>();

        private List<ResearchOpportunityCategoryTotalsStore> _categoryStores = new List<ResearchOpportunityCategoryTotalsStore>();

        private bool regenerateWhenPossible = false;

        private Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability> _categoryAvailability = new Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability>();

        public ResearchOpportunityManager(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if(regenerateWhenPossible)
            {
                regenerateWhenPossible = false;
                GenerateOpportunities(Find.ResearchManager.currentProj, true);
            }
            CheckForRegeneration();
            //CancelMarkedPrototypes();
            CheckForPopups();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            StartupChecks();
        }

        public void StartupChecks() 
        {
            bool forceRegen = false; 
            if (_projectsGenerated == null)
            {
                Log.Warning("RR: _projectsGenerated was missing!");
                _projectsGenerated = new HashSet<ResearchProjectDef>();
                forceRegen = true;
            }
            if (_allGeneratedOpportunities == null)
            {
                Log.Warning("RR: _allGeneratedOpportunities was missing!");
                _allGeneratedOpportunities = new List<ResearchOpportunity>();
                forceRegen = true;
            }
            if (_categoryAvailability == null)
            {
                Log.Warning("RR: _categoryAvailability was missing!");
                _categoryAvailability = new Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability>();
                forceRegen = true;
            }
            if (_categoryStores == null)
            {
                Log.Warning("RR: _categoryStores was missing!");
                _categoryStores = new List<ResearchOpportunityCategoryTotalsStore>();
                forceRegen = true;
            }

            if (forceRegen)
            {
                GenerateOpportunities(Find.ResearchManager.currentProj, true);
            }
            else
            {
                // contributed by mahenry00
                _allGeneratedOpportunities = _allGeneratedOpportunities
                .Where(o =>
                {
                    if (!o.IsValid())
                    {
                        Log.Warning($"[RR]: Research opportunity invalid, loadID: {o?.loadID}, project: {o?.project?.label}");
                        return false;
                    }
                    return true;
                })
                .ToList();

                CheckForRegeneration();
            }

            //clear caches. TODO: centralize caches in here instead?
            CacheClearer.ClearCaches();
        }

        public bool CheckForRegeneration() 
        {
            if(Find.ResearchManager.currentProj != _currentProject)
            {
                PrototypeKeeper.Instance.CancelPrototypes(_currentProject, Find.ResearchManager.currentProj);
                GenerateOpportunities(Find.ResearchManager.currentProj, false);
                return true;
            }
            return false;
        }

        public void CheckForPopups()
        {
            foreach (var category in CurrentProjectOpportunityCategories)
            {
                var current = category.GetCurrentAvailability(Find.ResearchManager.currentProj);
                if (!_categoryAvailability.ContainsKey(category))
                    _categoryAvailability[category] = current;
                else if (_categoryAvailability[category] != current)
                {
                    _categoryAvailability[category] = current;
                    if(current == OpportunityAvailability.Available)
                    {
                        var toPopup = CurrentProjectOpportunities.Where(o => o.def.generatesPopups &&  o.def.GetCategory(o.relation) == category).ToList();
                        if (toPopup.Any())
                        {
                            var groups = toPopup.GroupBy(o => new { o.def, o.relation });
                            foreach(var group in groups)
                            {
                                bool tooLong = group.Count() > 5;
                                string label = group.Key.def.GetHeaderCap(group.Key.relation);
                                string msg;
                                if (tooLong)
                                    msg = "RR_opportunityTypeReady_Many".Translate(label, string.Join(", ", group.Take(5).Select(o => o.requirement.Subject)), group.Count() - 6);
                                else
                                    msg = "RR_opportunityTypeReady".Translate(label, string.Join(", ", group.Select(o => o.requirement.Subject)));
                                Messages.Message(msg, MessageTypeDefOf.TaskCompletion, historical: false);
                            }
                        }
                    }
                }
            }
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, null);
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Type driverClass)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, (op) => op.JobDefs != null && op.JobDefs.Any(jd => jd.driverClass == driverClass));
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, ResearchOpportunityCategoryDef category)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, (op) => op.def.GetCategory(op.relation) == category);
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Def def)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, (op) => op.requirement.MetBy(def));
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Thing thing)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, (op) => op.requirement.MetBy(thing));
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Faction faction)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, (op) => op.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.MetByFaction(faction));
        }

        public ResearchOpportunity GetFirstFilteredOpportunity(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            return GetOpportunityFilter(desiredAvailability, handledBy, validator);
        }

        private ResearchOpportunity GetOpportunityFilter(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            List<ResearchOpportunity> opportunities = new List<ResearchOpportunity>();
            foreach (var op in CurrentProjectOpportunities)
            {
                if (((!desiredAvailability.HasValue) || (desiredAvailability.Value & op.CurrentAvailability) != 0) &&
                    (!handledBy.HasValue || op.def.handledBy.HasFlag(handledBy.Value)) &&
                    (validator == null || validator(op)))
                    return op;
            }
            return null;
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, null);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Type driverClass)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, (op) => op.JobDefs != null && op.JobDefs.Any(jd => jd.driverClass == driverClass));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, ResearchOpportunityCategoryDef category)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, (op) => op.def.GetCategory(op.relation) == category);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Def def)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, (op) => op.requirement.MetBy(def));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Thing thing)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, (op) => op.requirement.MetBy(thing));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Faction faction)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, (op) => op.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.MetByFaction(faction));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunities(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            return GetOpportunitiesFilter(desiredAvailability, handledBy, validator);
        }

        private IEnumerable<ResearchOpportunity> GetOpportunitiesFilter(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            List<ResearchOpportunity> opportunities = new List<ResearchOpportunity>();
            foreach (var op in CurrentProjectOpportunities)
            {
                if (((!desiredAvailability.HasValue) || (desiredAvailability.Value & op.CurrentAvailability) != 0) &&
                    (!handledBy.HasValue || op.def.handledBy.HasFlag(handledBy.Value)) &&
                    (validator == null || validator(op)))
                opportunities.Add(op);
            }
            return opportunities;
        }

        /*public IEnumerable<ResearchOpportunity> GetCurrentlyAvailableOpportunitiesFiltered(bool includeFinished, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            CheckForRegeneration();
            var ops = CurrentProjectOpportunities.Where(o => o.IsValid());
            if(includeFinished)
                ops = ops.Where(o => o.CurrentAvailability == OpportunityAvailability.Available);
            else
                ops = ops.Where(o => o.CurrentAvailability == OpportunityAvailability.Available || o.CurrentAvailability == OpportunityAvailability.Finished);
            if (handledBy.HasValue)
                ops = ops.Where(o => o.def.handledBy.HasFlag(handledBy));
            if (validator != null)
                ops = ops.Where(o => validator(o));

            return ops;
        }*/

        /*public IEnumerable<ResearchOpportunity> GetCurrentlyAvailableOpportunities(bool includeFinished = false)
        {
            bool currentProjectRegenned = CheckForRegeneration();
            if (!includeFinished)
                return CurrentProjectOpportunities.Where(o => o.IsValid() && o.CurrentAvailability == OpportunityAvailability.Available);
            else
                return CurrentProjectOpportunities.Where(o => o.IsValid() && (o.CurrentAvailability == OpportunityAvailability.Available || o.CurrentAvailability == OpportunityAvailability.Finished));
        }*/

        public ResearchOpportunityCategoryTotalsStore GetTotalsStore(ResearchProjectDef project, ResearchOpportunityCategoryDef category)
        {
            return _categoryStores.FirstOrDefault(cs => cs.project == project && cs.category == category);
        }

        public void PostFinishProject(ResearchProjectDef project)
        {
            _allGeneratedOpportunities.RemoveAll(o => o.project == project);
            _projectsGenerated.Remove(project);
            _categoryStores.RemoveAll(cs => cs.project == project);

            if (_currentProject == project)
            {
                _currentProject = null;
                _currentProjectOpportunitiesCache?.Clear();
                _currentOpportunityCategoriesCache?.Clear();
            }
        }

        public void ResetAllProgress()
        {
            _allGeneratedOpportunities?.Clear();
            _currentProjectOpportunitiesCache?.Clear();
            _currentOpportunityCategoriesCache?.Clear();
            _categoryStores?.Clear();
            _projectsGenerated?.Clear();
        }


        public void FinishProject(ResearchProjectDef project, bool doCompletionDialog = false, Pawn researcher = null)
        {
            Find.ResearchManager.FinishProject(project, doCompletionDialog, researcher);
        }

        public void DelayedRegeneration()
        {
            this.regenerateWhenPossible = true;
        }

        public void GenerateOpportunities(ResearchProjectDef project, bool forceRegen)
        {
            if(_currentProject == project && !forceRegen) 
            {
                return;
            }
            _currentProjectOpportunitiesCache = null;
            _currentOpportunityCategoriesCache = null;
            _currentProject = project;
            if (project == null)
                return;

            if (_projectsGenerated.Contains(project)) 
            {
                if (forceRegen)
                {
                    var preexistingOpportunities = _allGeneratedOpportunities.Where(o => o.project == project).ToList();
                    foreach (var preexisting in preexistingOpportunities)
                        _allGeneratedOpportunities.Remove(preexisting);
                }
                else
                {
                    return;
                }
            }

            var results = ResearchOpportunityPrefabs.MakeOpportunitiesForProject(project);
            var newOpportunities = results.opportunities;
            var categoryStores = results.categoryStores;
            _categoryStores.RemoveAll(cs => cs.project == project);
            _categoryStores.AddRange(categoryStores);

            var invalidOpportunities = newOpportunities.Where(o => !o.IsValid());
            if (invalidOpportunities.Any())
                Log.Warning($"Generated {invalidOpportunities.Count()} invalid opportunities for project {project}!");

            _allGeneratedOpportunities.AddRange(newOpportunities.Where(o => o.IsValid()));
            _projectsGenerated.Add(project);

            if (ResearchReinvented_Debug.debugPrintouts)
            {
                Log.Message($"Listing generated opportunities for current project {_currentProject.label}...");
                foreach (var opportunity in newOpportunities)
                {
                    Log.Message($" |-- {opportunity.ShortDesc} -- {opportunity.debug_source} (imp.: {opportunity.importance})");
                }
            }

            _categoryAvailability.Clear();
            foreach(var category in CurrentProjectOpportunityCategories)
            {
                _categoryAvailability[category] = category.GetCurrentAvailability(project);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if(Scribe.mode == LoadSaveMode.Saving) 
            {
                _allGeneratedOpportunities = _allGeneratedOpportunities.Where(o => o.IsValid()).ToList();
            }
            Scribe_Collections.Look(ref _allGeneratedOpportunities, "_allGeneratedOpportunities", LookMode.Deep);
            Scribe_Collections.Look(ref _projectsGenerated, "_allProjectsWithGeneratedOpportunities", LookMode.Def);
            Scribe_Defs.Look(ref _currentProject, "currentProject");
            Scribe_Collections.Look(ref _categoryStores, "_categoryStores", LookMode.Deep);
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _allGeneratedOpportunities = _allGeneratedOpportunities.Where(o => o.IsValid()).ToList();
            }
        }
    }
}
