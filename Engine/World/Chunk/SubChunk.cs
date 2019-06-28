using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SubChunk : MeshInstance
{
    public int SubChunkId = 0;
    public bool RenderTop = true;
    public bool RenderBottom = true;
    public bool RenderLeft = true;
    public bool RenderRight = true;
    public bool RenderFront = true;
    public bool RenderBack = true;

    private int[,,] m_Blocks = new int[16, 16, 16];
    private VoxelSprite[] m_Decoration = new VoxelSprite[64];

    private int m_DecorationCount = 0;

    // Total count of block on each faces.
    private int m_topCount = 0;
    private int m_bottomCount = 0;
    private int m_leftCount = 0;
    private int m_rightCount = 0;
    private int m_frontCount = 0;
    private int m_backCount = 0;

    public void Fill(bool filled)
    {
        if (!filled)
        {
            BlockCount = (int)Math.Pow(Chunk.CHUNK_SIZE, 3);
        }

        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    m_Blocks[x, y, z] = -1;
                }
    }


    public int BlockCount { get; private set; } = 0;

    
    // Whole chunk is full or empty
    public bool isFull()
    {
        return BlockCount == Math.Pow(Chunk.CHUNK_SIZE, 3);
    }

    public bool isEmpty()
    {
        return BlockCount == 0;
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
    public int[,,] GetData()
    {
        return m_Blocks;
    }

    public VoxelSprite[] GetDecorations()
    {
        return m_Decoration;
    }

    public void AddDecoration(VoxelSprite sprite)
    {
        if (m_DecorationCount == m_Decoration.Length)
            return;

        m_Decoration[m_DecorationCount] = sprite;
        m_DecorationCount++;
    }


    // The parent of this subchunk. All chunks have 16 subchunks.
    public Chunk Chunk
    {
        get; set;
    }

    
    public int GetBlock(int x, int y, int z)
    {
        return m_Blocks[x, y, z];
    }

    // Adds a block in this subchunk.
    public void AddBlock(Vector3 position, BLOCK_TYPE block)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;
    
        // If we are adding an empty block, just remove it.
        if ((int)block == -1)
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
        m_Blocks[x, y, z] = (int)block;
        BlockCount++;
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


        // Remove it and decrease the count.
        m_Blocks[x, y, z] = -1;
        BlockCount--;
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

