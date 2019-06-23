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
            { BLOCK_TYPE.Stone, new Color(204, 204, 204) },
            { BLOCK_TYPE.Dirt, new Color(137, 117, 85) },
            { BLOCK_TYPE.Grass, new Color(131, 216, 82) },
            { BLOCK_TYPE.Sand, new Color(255, 249, 99) },
            { BLOCK_TYPE.Leaves, new Color(114, 252, 60) },
            { BLOCK_TYPE.Trunk, new Color(114, 106, 53) },
        };

        public static Color GetColor(BLOCK_TYPE type)
        {
            return m_Palette[type];
        }
    }
}
