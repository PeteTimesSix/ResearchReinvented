using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public class PrototypeTerrainGrid : MapComponent
    {
        private ByteGrid grid;

        public PrototypeTerrainGrid(Map map): base(map)
        {
            this.grid = new ByteGrid(map);
        }

        public bool IsTerrainPrototype(IntVec3 position)
        {
            return grid[map.cellIndices.CellToIndex(position)] != 0;
        }

        public void MarkTerrainAsPrototype(IntVec3 position, TerrainDef terrain)
        {
            grid[map.cellIndices.CellToIndex(position)] = 1;
        }

        public void UnmarkTerrainAsPrototype(IntVec3 position)
        {
            grid[map.cellIndices.CellToIndex(position)] = 0;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "parent");
            Scribe_Deep.Look(ref grid, "grid");
            /*MapExposeUtility.Ex(parent, 
                (IntVec3 pos) => { return (grid[parent.cellIndices.CellToIndex(pos)] ? (ushort)1 : (ushort)0); }, 
                (IntVec3 pos, ushort val) => { protoGrid[parent.cellIndices.CellToIndex(pos)] = (val != 0 ? true : false); }, 
                "protoGrid");*/
        }

        public void DebugDrawOnMap()
        {
            var mapSizeX = map.Size.x;
            for (int i = 0; i < grid.CellsCount; i++)
            {
                var cell = CellIndicesUtility.IndexToCell(i, mapSizeX);
                CellRenderer.RenderCell(cell, SolidColorMaterials.SimpleSolidColorMaterial(IsTerrainPrototype(cell) ? Color.green : Color.red, false));
            }
        }
    }
}
