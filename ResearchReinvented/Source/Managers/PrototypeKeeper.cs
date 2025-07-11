﻿using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public class PrototypeKeeper : GameComponent
    {
        public static PrototypeKeeper Instance => Current.Game.GetComponent<PrototypeKeeper>();

        private HashSet<Thing> _prototypes = new HashSet<Thing>();
        private Dictionary<Map, PrototypeTerrainGrid> _prototypeTerrainGrids = new Dictionary<Map, PrototypeTerrainGrid>();

        public HashSet<Thing> Prototypes => _prototypes;

        public PrototypeKeeper(Game game) { }

        public PrototypeTerrainGrid GetMapPrototypeTerrainGrid(Map map)
        {
            if (_prototypeTerrainGrids == null)
                _prototypeTerrainGrids = new Dictionary<Map, PrototypeTerrainGrid>();

            if (_prototypeTerrainGrids.TryGetValue(map, out PrototypeTerrainGrid grid)) 
            {
                return grid;
            }
            else 
            {
                grid = map.GetComponent<PrototypeTerrainGrid>();
                _prototypeTerrainGrids[map] = grid;
                return grid;
            }
        }

        public bool IsPrototype(Thing thing)
        {
            var unwrapped = thing.UnwrapIfWrapped();
            if(unwrapped != thing)
            {
                if(Prototypes.Contains(unwrapped)) 
                    return true;
            }
            return Prototypes.Contains(thing);
        }

        public void MarkAsPrototype(Thing thing)
        {
            Prototypes.Add(thing);
        }

        public void UnmarkAsPrototype(Thing thing)
        {
            var unwrapped = thing.UnwrapIfWrapped();
            if (unwrapped != thing)
            {
                Prototypes.Remove(unwrapped);
            }
            Prototypes.Remove(thing);
        }

        public bool IsTerrainPrototype(IntVec3 position, Map map)
        {
            return GetMapPrototypeTerrainGrid(map)?.IsTerrainPrototype(position) ?? false;
        }
        public bool IsFoundationTerrainPrototype(IntVec3 position, Map map)
        {
            return GetMapPrototypeTerrainGrid(map)?.IsFoundationTerrainPrototype(position) ?? false;
        }

        public void MarkTerrainAsPrototype(IntVec3 position, Map map, TerrainDef terrain)
        {
            GetMapPrototypeTerrainGrid(map)?.MarkTerrainAsPrototype(position, terrain);
        }
        public void UnmarkTerrainAsPrototype(IntVec3 position, Map map)
        {
            GetMapPrototypeTerrainGrid(map)?.UnmarkTerrainAsPrototype(position);
        }

        public void MarkFoundationTerrainAsPrototype(IntVec3 position, Map map, TerrainDef terrain)
        {
            GetMapPrototypeTerrainGrid(map)?.MarkFoundationTerrainAsPrototype(position, terrain);
        }
        public void UnmarkFoundationTerrainAsPrototype(IntVec3 position, Map map)
        {
            GetMapPrototypeTerrainGrid(map)?.UnmarkFoundationTerrainAsPrototype(position);
        }

        public void DebugDrawOnMap()
        {
            if(ResearchReinvented_Debug.drawPrototypeGrid)
            {
                GetMapPrototypeTerrainGrid(Find.CurrentMap)?.DebugDrawOnMap();
            }
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                var tempList = this._prototypes.ToList();
                foreach (var thing in tempList)
                {
                    if (thing.Destroyed)
                        this._prototypes.Remove(thing);
                }
            }
            Scribe_Collections.Look(ref _prototypes, "_prototypes", LookMode.Reference);
        }

        public void CancelPrototypes(ResearchProjectDef previousProject, ResearchProjectDef currentProject)
        {
            if (ResearchReinventedMod.Settings.disablePrototypeBillCancellation)
                return;

            var protoOps = ResearchOpportunityManager.Instance.AllGeneratedOpportunities.Where(o => o.project != currentProject && o.def.handledBy.HasFlag(HandlingMode.Special_Prototype));

            var defsToCancel = new HashSet<Def>();
            foreach (var protoOp in protoOps)
            {
                if (protoOp.requirement is ROComp_RequiresThing regThing)
                {
                    foreach(var altThing in regThing.AllThings)
                    {
                        defsToCancel.Add(altThing);
                    }
                }
                else if (protoOp.requirement is ROComp_RequiresTerrain regTerrain)
                {
                    foreach (var altTerrain in regTerrain.AllTerrains)
                    {
                        defsToCancel.Add(altTerrain);
                    }
                }
                else if (protoOp.requirement is ROComp_RequiresRecipe regRecipe)
                {
                    foreach (var altRecipe in regRecipe.AllRecipes)
                    {
                        defsToCancel.Add(altRecipe);
                    }
                }
            }
            if (defsToCancel.Contains(null))
                defsToCancel.Remove(null);

            foreach (var map in Find.Maps)
            {
                foreach (var blueprint in map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).Where(t => t.Faction == Faction.OfPlayer).ToList()) //lets not cancel hostile mortars and such
                {
                    if (defsToCancel.Contains(blueprint.def.entityDefToBuild))
                    {
                        if (!blueprint.Destroyed)
                        {
                            Prototypes.Remove(blueprint);
                            blueprint.Destroy(DestroyMode.Cancel);
                        }
                    }
                }
                foreach (var frame in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame).Where(t => t.Faction == Faction.OfPlayer).ToList()) //lets not cancel hostile mortars and such
                {
                    if (defsToCancel.Contains(frame.def.entityDefToBuild))
                    {
                        if (!frame.Destroyed)
                        {
                            Prototypes.Remove(frame);
                            frame.Destroy(DestroyMode.Cancel);
                        }
                    }
                }
                foreach (var uft in map.listerThings.AllThings.Where(t => t.def.isUnfinishedThing && t is UnfinishedThing).Cast<UnfinishedThing>().ToList())
                {
                    if (defsToCancel.Contains(uft.Recipe))
                    {
                        if (!uft.Destroyed)
                        {
                            Prototypes.Remove(uft);
                            uft.Destroy(DestroyMode.Cancel);
                        }
                    }
                }
                foreach (var billHolder in map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver).Where(t => t is IBillGiver).Cast<IBillGiver>().ToList())
                {
                    var billsToCancel = billHolder.BillStack.Bills.Where(b => defsToCancel.Contains(b.recipe)).ToList();
                    foreach(var bill in billsToCancel)
                    {
                        billHolder.BillStack.Delete(bill);
                    }
                }
            }
        }
    }
}
