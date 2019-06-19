using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine.World;

namespace VoxelEngine.Engine.World
{
    public static class BlockPalette
    {
        private static Dictionary<BLOCK_TYPE, Color> m_Palette = new Dictionary<BLOCK_TYPE, Color>
        {
            { BLOCK_TYPE.Stone, new Color(1,0,0) },
            { BLOCK_TYPE.Dirt, new Color(1,0,0) },
            { BLOCK_TYPE.Grass, new Color(1,0,0) },
            { BLOCK_TYPE.Sand, new Color(1,0,0) },
            { BLOCK_TYPE.Leaves, new Color(1,0,0) },
            { BLOCK_TYPE.Trunk, new Color(1,0,0) },
        };
    }
}
