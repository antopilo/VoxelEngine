using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelEngine.Engine.World
{
    public class ChunkManager
    {
        private static List<Chunk> m_ChunkLoadList = new List<Chunk>();

        public void AddToLoadList(Chunk chunk)
        {
            if (m_ChunkLoadList.Contains(chunk))
            {
                GD.Print("Warning: AddToLoadList->chunk already in list");
                return;
            }

            m_ChunkLoadList.Add(chunk);
        }

    }
}
