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
    public class PrototypeTerrainGrid : IExposable
    {
        private Map parent;
        private ByteGrid grid;

        public PrototypeTerrainGrid() { }

        public PrototypeTerrainGrid(Map map)
        {
            this.parent = map;
            this.grid = new ByteGrid(map);
        }

        public bool IsTerrainPrototype(IntVec3 position)
        {
            return grid[parent.cellIndices.CellToIndex(position)] != 0;
        }

        public void MarkTerrainAsPrototype(IntVec3 position, TerrainDef terrain)
        {
            grid[parent.cellIndices.CellToIndex(position)] = 1;
        }

        public void UnmarkTerrainAsPrototype(IntVec3 position)
        {
            grid[parent.cellIndices.CellToIndex(position)] = 0;
        }


        public void ExposeData()
        {
            Scribe_References.Look(ref parent, "parent");
            Scribe_Deep.Look(ref grid, "grid");
            /*MapExposeUtility.Ex(parent, 
                (IntVec3 pos) => { return (grid[parent.cellIndices.CellToIndex(pos)] ? (ushort)1 : (ushort)0); }, 
                (IntVec3 pos, ushort val) => { protoGrid[parent.cellIndices.CellToIndex(pos)] = (val != 0 ? true : false); }, 
                "protoGrid");*/
        }

        public void DebugDrawOnMap()
        {
            var mapSizeX = parent.Size.x;
            for (int i = 0; i < grid.CellsCount; i++)
            {
                var cell = CellIndicesUtility.IndexToCell(i, mapSizeX);
                CellRenderer.RenderCell(cell, SolidColorMaterials.SimpleSolidColorMaterial(IsTerrainPrototype(cell) ? Color.green : Color.red, false));
            }
        }
    }
}
