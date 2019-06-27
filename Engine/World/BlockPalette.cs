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
            { BLOCK_TYPE.Water, new Color(0.21176f, 0.5137f, 1, 0.5f) },
            { BLOCK_TYPE.Stone, new Color(0,0,0,1f) },
            { BLOCK_TYPE.Dirt, new Color("a14800") },
            { BLOCK_TYPE.Grass, new Color(0.21176f,  0.87059f,  0.16471f) },
            { BLOCK_TYPE.Sand, new Color(1,1,0) },
            { BLOCK_TYPE.Leaves, new Color(114/ 255f, 252/ 255f, 60/ 255f) },
            { BLOCK_TYPE.Trunk, new Color(114/ 255f, 106/ 255f, 53/ 255f) },
        };

        public static Color GetColor(BLOCK_TYPE type, int x, int z)
        {
            var color = m_Palette[type];

            //if ( type == BLOCK_TYPE.Grass)
            //    color = color.LinearInterpolate(new Color(1, 1, 0),NoiseMaker.GetHumidity(x, z));

            return color;
        }
    }
}
