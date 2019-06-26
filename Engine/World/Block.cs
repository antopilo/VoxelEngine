using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelEngine.engine.World
{
    public enum BLOCK_TYPE
    {
        Empty = -1,Stone, Dirt, Grass, Sand, Leaves, Trunk
    }

    public class Block
    {
        public static float BLOCK_RENDER_SIZE = 1f;

        public bool Active = true;
        public BLOCK_TYPE Type = BLOCK_TYPE.Stone;
    }
}
