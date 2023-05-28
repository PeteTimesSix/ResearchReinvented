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
            if(_prototypeTerrainGrids.TryGetValue(map, out PrototypeTerrainGrid grid)) 
            {
                return grid;
            }
            else 
            {
                grid = new PrototypeTerrainGrid(map);
                _prototypeTerrainGrids[map] = grid;
                return grid;
            }
        }


        public bool IsPrototype(Thing thing)
        {
            return Prototypes.Contains(thing);
        }

        public void MarkAsPrototype(Thing thing) 
        {
            Prototypes.Add(thing);
        }

        public void UnmarkAsPrototype(Thing thing)
        {
            Prototypes.Remove(thing);
        }

        public bool IsTerrainPrototype(IntVec3 position, Map map)
        {
            return GetMapPrototypeTerrainGrid(map)?.IsTerrainPrototype(position) ?? false;
        }

        public void MarkTerrainAsPrototype(IntVec3 position, Map map, TerrainDef terrain)
        {
            GetMapPrototypeTerrainGrid(map).MarkTerrainAsPrototype(position, terrain);
        }

        public void UnmarkTerrainAsPrototype(IntVec3 position, Map map)
        {
            GetMapPrototypeTerrainGrid(map).UnmarkTerrainAsPrototype(position);
        }

        public void DebugDrawOnMap()
        {
            if(ResearchReinvented_Debug.drawPrototypeGrid)
            {
                GetMapPrototypeTerrainGrid(Find.CurrentMap)?.DebugDrawOnMap();
            }
        }

        private List<Map> wlistMaps = new List<Map>();
        private List<PrototypeTerrainGrid> wlistGrids = new List<PrototypeTerrainGrid>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _prototypes, "_prototypes", LookMode.Reference);

            Scribe_Collections.Look(ref _prototypeTerrainGrids, "_prototypeTerrainGrids", LookMode.Reference, LookMode.Deep, ref wlistMaps, ref wlistGrids);
        }
    }
}
