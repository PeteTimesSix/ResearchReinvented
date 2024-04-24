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

        private ResearchProjectDef _lastCurrentProject;
        private List<ResearchOpportunity> _allGeneratedOpportunities = new List<ResearchOpportunity>();

        private Dictionary<ResearchProjectDef, List<ResearchOpportunity>> _generatedOpportunitiesByProject = new();

        public IReadOnlyCollection<ResearchOpportunity> AllGeneratedOpportunities => _allGeneratedOpportunities.AsReadOnly();

        public HashSet<ResearchProjectDef> _projectsGenerated = new HashSet<ResearchProjectDef>();

        private List<ResearchOpportunityCategoryTotalsStore> _categoryStores = new List<ResearchOpportunityCategoryTotalsStore>();

        private bool regenerateWhenPossible = false;

        private Dictionary<ResearchProjectDef, Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability>> _categoryAvailability = new Dictionary<ResearchProjectDef, Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability>>();

        public ResearchOpportunityManager(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if(regenerateWhenPossible)
            {
                regenerateWhenPossible = false;
                GenerateOpportunities(Find.ResearchManager.GetProject(), true);
            }
            CheckForRegeneration();
            //CancelMarkedPrototypes();
            if (!Find.TickManager.Paused && Find.TickManager.TicksGame % (60 * Find.TickManager.TickRateMultiplier) == 0) //only check popups once every second
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
            if(_generatedOpportunitiesByProject == null)
            {
                Log.Warning("RR: _generatedOpportunitiesByProject was missing!");
                _generatedOpportunitiesByProject = new();
                forceRegen = true;
            }
            if (_categoryAvailability == null)
            {
                Log.Warning("RR: _categoryAvailability was missing!");
                _categoryAvailability = new Dictionary<ResearchProjectDef, Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability>>();
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
                GenerateOpportunities(Find.ResearchManager.GetProject(), true);
            }
            else
            {
                // contributed by mahenry00, modified later
                var invalids = _allGeneratedOpportunities.Where(o => !o.IsValid());
                foreach (var o in invalids)
                {
                    Log.Warning($"[RR]: Research opportunity invalid, loadID: {o?.loadID}, project: {o?.project?.label}");
                    if(o?.project != null)
                        _generatedOpportunitiesByProject[o.project]?.Remove(o);
                }
                _allGeneratedOpportunities = _allGeneratedOpportunities.Except(invalids).ToList();

                CheckForRegeneration();
            }
        }

        public bool CheckForRegeneration() 
        {
            if(Find.ResearchManager.GetProject() != _lastCurrentProject)
            {
                //PrototypeKeeper.Instance.CancelPrototypes(_currentProject, Find.ResearchManager.GetProject());
                GenerateOpportunities(Find.ResearchManager.GetProject(), false);
                return true;
            }
            return false;
        }

        public void CheckForPopups()
        {
            foreach (var project in _projectsGenerated)
            {
                if(!_categoryAvailability.ContainsKey(project))
                {
                    _categoryAvailability[project] = new();
                    foreach(var category in DefDatabase<ResearchOpportunityCategoryDef>.AllDefs)
                    {
                        _categoryAvailability[project][category] = OpportunityAvailability.UnavailableReasonUnknown;
                    }
                }
                var categories = _categoryAvailability[project];
                var kvPairsCopy = categories.ToArray();
                foreach (var (category, storedAvailability) in kvPairsCopy)
                {
                    var currentAvailability = category.GetCurrentAvailability(project);
                    if (storedAvailability != currentAvailability)
                    {
                        categories[category] = currentAvailability;
                        if (currentAvailability == OpportunityAvailability.Available)
                        {
                            var toPopup = GetFilteredOpportunitiesOfProject(project, null, null, (op) => op.def.generatesPopups && op.def.GetCategory(op.relation) == category);
                            //var toPopup = CurrentProjectOpportunities.Where(o => o.def.generatesPopups && o.def.GetCategory(o.relation) == category).ToList();
                            if (toPopup.Any())
                            {
                                var groups = toPopup.GroupBy(o => new { o.def, o.relation });
                                foreach (var group in groups)
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
                    categories[category] = currentAvailability;
                }
            }
        }

        public IEnumerable<ResearchOpportunity> GetOpportunitiesOfProject(ResearchProjectDef project)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesMainSingle(project, null, null, null);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesMainSingle(project, desiredAvailability, handledBy, null);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Type driverClass)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProject(project, desiredAvailability, handledBy, (op) => op.JobDefs != null && op.JobDefs.Any(jd => jd.driverClass == driverClass));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, ResearchOpportunityCategoryDef category)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProject(project, desiredAvailability, handledBy, (op) => op.def.GetCategory(op.relation) == category);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Def def)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProject(project, desiredAvailability, handledBy, (op) => op.requirement.MetBy(def));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Thing thing)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProject(project, desiredAvailability, handledBy, (op) => op.requirement.MetBy(thing));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Faction faction)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProject(project, desiredAvailability, handledBy, (op) => op.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.MetByFaction(faction));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProject(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            if (project == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesMainSingle(project, desiredAvailability, handledBy, validator);
        }

        public IEnumerable<ResearchOpportunity> GetOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesMain(projects, null, null, null);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesMain(projects, desiredAvailability, handledBy, null);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Type driverClass)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProjects(projects, desiredAvailability, handledBy, (op) => op.JobDefs != null && op.JobDefs.Any(jd => jd.driverClass == driverClass));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, ResearchOpportunityCategoryDef category)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProjects(projects, desiredAvailability, handledBy, (op) => op.def.GetCategory(op.relation) == category);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Def def)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProjects(projects, desiredAvailability, handledBy, (op) => op.requirement.MetBy(def));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Thing thing)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProjects(projects, desiredAvailability, handledBy, (op) => op.requirement.MetBy(thing));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Faction faction)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesOfProjects(projects, desiredAvailability, handledBy, (op) => op.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.MetByFaction(faction));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfProjects(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            if (projects == null)
                return Enumerable.Empty<ResearchOpportunity>();
            return GetFilteredOpportunitiesMain(projects, desiredAvailability, handledBy, validator);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy)
        {
            return GetFilteredOpportunitiesOfAllMain(desiredAvailability, handledBy, null);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Type driverClass)
        {
            return GetFilteredOpportunitiesOfAll(desiredAvailability, handledBy, (op) => op.JobDefs != null && op.JobDefs.Any(jd => jd.driverClass == driverClass));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, ResearchOpportunityCategoryDef category)
        {
            return GetFilteredOpportunitiesOfAll(desiredAvailability, handledBy, (op) => op.def.GetCategory(op.relation) == category);
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Def def)
        {
            return GetFilteredOpportunitiesOfAll(desiredAvailability, handledBy, (op) => op.requirement.MetBy(def));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Thing thing)
        {
            return GetFilteredOpportunitiesOfAll(desiredAvailability, handledBy, (op) => op.requirement.MetBy(thing));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Faction faction)
        {
            return GetFilteredOpportunitiesOfAll(desiredAvailability, handledBy, (op) => op.requirement is ROComp_RequiresFaction requiresFaction && requiresFaction.MetByFaction(faction));
        }

        public IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAll(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            return GetFilteredOpportunitiesOfAllMain(desiredAvailability, handledBy, validator);
        }

        List<ResearchOpportunity> tempOpportunities = new();
        private IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesMainSingle(ResearchProjectDef project, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            if(project == null)
                return Enumerable.Empty<ResearchOpportunity>();

            if (!_projectsGenerated.Contains(project))
                GenerateOpportunities(project, false);

            if(!_generatedOpportunitiesByProject.ContainsKey(project))
            {
                var newList = new List<ResearchOpportunity>();
                newList.AddRange(_allGeneratedOpportunities.Where(op => op.project == project));
                _generatedOpportunitiesByProject[project] = newList;
            }

            tempOpportunities.Clear();

            foreach (var op in _generatedOpportunitiesByProject[project])
            {
                if ((project == null || op.project == project) &&
                    (((!desiredAvailability.HasValue) || (desiredAvailability.Value & op.CurrentAvailability) != 0) &&
                    (!handledBy.HasValue || op.def.handledBy.HasFlag(handledBy.Value)) &&
                    (validator == null || validator(op))))
                {
                    tempOpportunities.Add(op);
                }
            }
            return tempOpportunities;
        }

        private IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesMain(IEnumerable<ResearchProjectDef> projects, OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            if (projects == null || !projects.Any())
                return Enumerable.Empty<ResearchOpportunity>();

            tempOpportunities.Clear();
            foreach (var project in projects)
            {
                if (!_projectsGenerated.Contains(project))
                    GenerateOpportunities(project, false);

                if (!_generatedOpportunitiesByProject.ContainsKey(project))
                {
                    var newList = new List<ResearchOpportunity>();
                    newList.AddRange(_allGeneratedOpportunities.Where(op => op.project == project));
                    _generatedOpportunitiesByProject[project] = newList;
                }
            }

            foreach (var project in projects)
            {
                foreach (var op in _generatedOpportunitiesByProject[project])
                {
                    if ((projects == null || projects.Contains(op.project)) &&
                        (((!desiredAvailability.HasValue) || (desiredAvailability.Value & op.CurrentAvailability) != 0) &&
                        (!handledBy.HasValue || op.def.handledBy.HasFlag(handledBy.Value)) &&
                        (validator == null || validator(op))))
                    {
                        tempOpportunities.Add(op);
                    }
                }
            }
            return tempOpportunities;
        }

        private IEnumerable<ResearchOpportunity> GetFilteredOpportunitiesOfAllMain(OpportunityAvailability? desiredAvailability, HandlingMode? handledBy, Func<ResearchOpportunity, bool> validator)
        {
            foreach (var op in _allGeneratedOpportunities)
            {
                if ((((!desiredAvailability.HasValue) || (desiredAvailability.Value & op.CurrentAvailability) != 0) &&
                    (!handledBy.HasValue || op.def.handledBy.HasFlag(handledBy.Value)) &&
                    (validator == null || validator(op))))
                {
                    tempOpportunities.Add(op);
                }
            }
            return tempOpportunities;
        }

        public ResearchOpportunityCategoryTotalsStore GetTotalsStore(ResearchProjectDef project, ResearchOpportunityCategoryDef category)
        {
            return _categoryStores.FirstOrDefault(cs => cs.project == project && cs.category == category);
        }

        public void PostFinishProject(ResearchProjectDef project)
        {
            _allGeneratedOpportunities.RemoveAll(o => o.project == project);
            _generatedOpportunitiesByProject.Remove(project);
            _projectsGenerated.Remove(project);
            _categoryStores.RemoveAll(cs => cs.project == project);
            _lastCurrentProject = null;
            OpportunityCachesHandler.ClearCaches();
        }

        public void ResetAllProgress()
        {
            _allGeneratedOpportunities?.Clear();
            _generatedOpportunitiesByProject?.Clear();
            _categoryStores?.Clear();
            _projectsGenerated?.Clear();
            _lastCurrentProject = null;
            OpportunityCachesHandler.ClearCaches();
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

            OpportunityCachesHandler.ClearCaches();
            _lastCurrentProject = project;

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
                Log.Message($"Listing generated opportunities for project {project.label}...");
                foreach (var opportunity in newOpportunities)
                {
                    Log.Message($" |-- {opportunity.ShortDesc} -- {opportunity.debug_source} (imp.: {opportunity.importance})");
                }
            }

            _categoryAvailability.Clear();
            if(!_categoryAvailability.ContainsKey(project))
            {
                _categoryAvailability[project] = new Dictionary<ResearchOpportunityCategoryDef, OpportunityAvailability>();
                foreach(var category in DefDatabase<ResearchOpportunityCategoryDef>.AllDefs)
                {
                    _categoryAvailability[project][category] = category.GetCurrentAvailability(project);
                }
            }
            var projectCategoryAvailability = _categoryAvailability[project];
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
            Scribe_Defs.Look(ref _lastCurrentProject, "_lastCurrentProject");
            Scribe_Collections.Look(ref _categoryStores, "_categoryStores", LookMode.Deep);
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _allGeneratedOpportunities = _allGeneratedOpportunities.Where(o => o.IsValid()).ToList();
            }
        }
    }
}
