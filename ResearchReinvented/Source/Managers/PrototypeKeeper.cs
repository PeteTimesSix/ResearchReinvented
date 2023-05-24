using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public class PrototypeKeeper : GameComponent
    {
        public static PrototypeKeeper Instance => Current.Game.GetComponent<PrototypeKeeper>();

        private HashSet<Thing> _prototypes = new HashSet<Thing>();
        private Dictionary<Map, PrototypeTerrainGrid> _prototypeTerrainGrids = new Dictionary<Map, PrototypeTerrainGrid>();

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
            return _prototypes.Contains(thing);
        }

        public void MarkAsPrototype(Thing thing) 
        {
            _prototypes.Add(thing);
        }

        public void UnmarkAsPrototype(Thing thing)
        {
            _prototypes.Remove(thing);
        }

        public void IsTerrainPrototype(IntVec3 position, Map map)
        {
            GetMapPrototypeTerrainGrid(map).IsTerrainPrototype(position);
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
                var currentGrid = _prototypeTerrainGrids.Where(g => g.Key == Find.CurrentMap).Select(g => g.Value).FirstOrDefault();
                if (currentGrid != null)
                    currentGrid.DebugDrawOnMap();
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
