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
            { BLOCK_TYPE.Stone, new Color(204 / 255f, 204/ 255f, 204/ 255f) },
            { BLOCK_TYPE.Dirt, new Color(137/ 255f, 117/ 255f, 85/ 255f) },
            { BLOCK_TYPE.Grass, new Color(131/ 255f, 216/ 255f, 82/ 255f) },
            { BLOCK_TYPE.Sand, new Color(255/ 255f, 249/ 255f, 99/ 255f) },
            { BLOCK_TYPE.Leaves, new Color(114/ 255f, 252/ 255f, 60/ 255f) },
            { BLOCK_TYPE.Trunk, new Color(114/ 255f, 106/ 255f, 53/ 255f) },
        };

        public static Color GetColor(BLOCK_TYPE type)
        {
            return m_Palette[type];
        }
    }
}
