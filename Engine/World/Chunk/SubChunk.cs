using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine.World;

namespace VoxelEngine.Engine.World
{
    public class SubChunk : MeshInstance
    {
        public int SubChunkId = 0;
        public bool RenderTop = true;
        public bool RenderBottom = true;
        public bool RenderLeft = true;
        public bool RenderRight = true;
        public bool RenderFront = true;
        public bool RenderBack = true;

        private Block[,,] m_Blocks = new Block[16, 16, 16];

        // Total count of block in m_Block.
        private int m_count = 0;

        // Total count of block on each faces.
        private int m_topCount = 0;
        private int m_bottomCount = 0;
        private int m_leftCount = 0;
        private int m_rightCount = 0;
        private int m_frontCount = 0;
        private int m_backCount = 0;

        public void Fill(bool filled)
        {
            var block = new Block
            {
                Active = false
            };

            if (filled)
            {
                m_count = (int)Math.Pow(Chunk.CHUNK_SIZE, 3);
                block.Active = true;
            }

            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        m_Blocks[x, y, z] = block;
                    }
        }


        public int BlockCount
        {
            get
            {
                return m_count;
            }
        }



        // Whole chunk is full or empty
        public bool isFull()
        {
            return m_count == Math.Pow(Chunk.CHUNK_SIZE, 3);
        }

        public bool isEmpty()
        {
            return m_count == 0;
        }

        // Is each side full or empty.
        public bool isTopFull()
        {
            return m_topCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
        }

        public bool isBottomFull()
        {
            return m_bottomCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
        }

        public bool isLeftFull()
        {
            return m_leftCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
        }

        public bool isRightFull()
        {
            return m_rightCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
        }

        public bool isFrontFull()
        {
            return m_frontCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
        }

        public bool isBackFull()
        {
            return m_backCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
        }

        // Returns the data in this sub-chunk.
        public Block[,,] GetData()
        {
            return m_Blocks;
        }

        // The parent of this subchunk. All chunks have 16 subchunks.
        public Chunk Chunk
        {
            get; set;
        }

       
        public Block GetBlock(int x, int y, int z)
        {
            return m_Blocks[x, y, z];
        }

        // Adds a block in this subchunk.
        public void AddBlock(Vector3 position, Block block)
        {
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;
       
            // If we are adding an empty block, just remove it.
            if (block.Active == false)
            {
                RemoveBlock(position);
                return;
            }

            // Update flags for each side.
            if (y == Chunk.CHUNK_SIZE - 1)
                m_topCount++;
            if (y == 0)
                m_bottomCount++;
            if (x == Chunk.CHUNK_SIZE - 1)
                m_rightCount++;
            if (x == 0)
                m_leftCount++;
            if (z == Chunk.CHUNK_SIZE - 1)
                m_frontCount++;
            if (z == 0)
                m_backCount++;

            
            // Sets the block and update the total count.
            m_Blocks[x, y, z] = block;
            m_count++;
        }

        // Removes a block in this subchunk.
        public void RemoveBlock(Vector3 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;

            // Update flags for each side.
            if (y == Chunk.CHUNK_SIZE - 1)
                m_topCount--;
            if (y == 0)
                m_bottomCount--;
            if (x == Chunk.CHUNK_SIZE - 1)
                m_rightCount--;
            if (x == 0)
                m_leftCount--;
            if (z == Chunk.CHUNK_SIZE - 1)
                m_frontCount--;
            if (z == 0)
                m_backCount--;

            // If null add one.
            if (m_Blocks[x, y, z] is null)
                m_Blocks[x, y, z] = new Block();

            // Remove it and decrease the count.
            m_Blocks[x, y, z].Active = false;
            m_count--;
        }

        // Signals for frustrum culling
        public void CameraEntered(Camera camera)
        {
            if (camera.Name == "CameraInGame")
                this.Visible = true;
        }

        public void CameraExited(Camera camera)
        {
            if (camera.Name == "CameraInGame")
                this.Visible = false;
        }
    }
}
