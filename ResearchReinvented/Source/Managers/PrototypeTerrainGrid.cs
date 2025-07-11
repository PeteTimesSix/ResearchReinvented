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
        private ByteGrid terrainGrid;
        private ByteGrid foundationGrid;

        public PrototypeTerrainGrid(Map map): base(map)
        {
            this.terrainGrid = new ByteGrid(map);
            this.foundationGrid = new ByteGrid(map);
        }

        public bool IsTerrainPrototype(IntVec3 position)
        {
            return terrainGrid[map.cellIndices.CellToIndex(position)] != 0;
        }

        public void MarkTerrainAsPrototype(IntVec3 position, TerrainDef terrain)
        {
            terrainGrid[map.cellIndices.CellToIndex(position)] = 1;
        }

        public void UnmarkTerrainAsPrototype(IntVec3 position)
        {
            terrainGrid[map.cellIndices.CellToIndex(position)] = 0;
        }

        public bool IsFoundationTerrainPrototype(IntVec3 position)
        {
            return foundationGrid[map.cellIndices.CellToIndex(position)] != 0;
        }

        public void MarkFoundationTerrainAsPrototype(IntVec3 position, TerrainDef terrain)
        {
            foundationGrid[map.cellIndices.CellToIndex(position)] = 1;
        }

        public void UnmarkFoundationTerrainAsPrototype(IntVec3 position)
        {
            foundationGrid[map.cellIndices.CellToIndex(position)] = 0;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_References.Look(ref map, "parent");
            Scribe_Deep.Look(ref terrainGrid, "terrainGrid");
            Scribe_Deep.Look(ref foundationGrid, "foundationGrid");
            /*MapExposeUtility.Ex(parent, 
                (IntVec3 pos) => { return (grid[parent.cellIndices.CellToIndex(pos)] ? (ushort)1 : (ushort)0); }, 
                (IntVec3 pos, ushort val) => { protoGrid[parent.cellIndices.CellToIndex(pos)] = (val != 0 ? true : false); }, 
                "protoGrid");*/
        }

        public void DebugDrawOnMap()
        {
            var mapSizeX = map.Size.x;
            for (int i = 0; i < terrainGrid.CellsCount; i++)
            {
                var cell = CellIndicesUtility.IndexToCell(i, mapSizeX);
                bool isTerrainProto = IsTerrainPrototype(cell);
                bool isFoundationProto = IsFoundationTerrainPrototype(cell);
                Color color = Color.black;
                if (isTerrainProto && isFoundationProto)
                    color = Color.red;
                else if (isTerrainProto)
                    color = Color.green;
                else if (isFoundationProto)
                    color = Color.blue;

                if (color != Color.black)
                    CellRenderer.RenderCell(cell, SolidColorMaterials.SimpleSolidColorMaterial(color, false));
            }
        }
    }
}
