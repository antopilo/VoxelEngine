using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine;
using VoxelEngine.engine.World;

public class Chunk : Spatial
{
    public static int CHUNK_SIZE = 16;
    public static int SUBCHUNK_COUNT = 16;


    private SubChunk[] m_SubChunks = new SubChunk[SUBCHUNK_COUNT];
    public Vector2 Position = new Vector2();
    public bool isSurrounded = false;

    public void ChunkSetup()
    {
        // Filling all subchunks
        for (int sc = 0; sc < SUBCHUNK_COUNT; sc++)
        {
            m_SubChunks[sc] = new SubChunk();
            m_SubChunks[sc].SetChunk(this);
        }

        // Filling the chunk for testings.
        for (int y = 0; y < Chunk.CHUNK_SIZE * SUBCHUNK_COUNT; y++)
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    AddBlock(new Vector3(x, y, z), new Block());

        Update();
    }

    public void SetPosition(Vector2 position)
    {
        int x = (int)position.x * (int)CHUNK_SIZE;
        int z = (int)position.y * (int)CHUNK_SIZE;

        this.Position = position;
        this.Translation = new Vector3(x, 0, z);
    }

    private void Update()
    {
        isSurrounded = ChunkManager.IsChunkSurrounded(Position);
        for (int i = 0; i < SUBCHUNK_COUNT; i++)
        {
            if (i == 0)
                m_SubChunks[i].RenderBottom = false;
            else
            {
                bool previousTop = m_SubChunks[i - 1].isTopFull();
                if(previousTop && m_SubChunks[i].isBottomFull())
                {
                    m_SubChunks[i - 1].RenderTop = false;
                    m_SubChunks[i].RenderBottom = false;
                }
            }

        }
    }


    public void AddBlock(Vector3 position, Block block)
    {
        int subChunkIndex  = Mathf.Clamp(((int)position.y / CHUNK_SIZE), 0, CHUNK_SIZE - 1);
        int subChunkHeight = (int)position.y - ((int)CHUNK_SIZE * (subChunkIndex));
        var localPosition = new Vector3(position.x, subChunkHeight, position.z);

        m_SubChunks[subChunkIndex].AddBlock(localPosition, block);

    }

    // Renders all the chunk, including the subchunks.
    public void Render()
    {
        Update();

        for (int i = 0; i < SUBCHUNK_COUNT; i++)
        {
            var subChunk = m_SubChunks[i];
            subChunk.Name = i.ToString();
            subChunk.Translate(new Vector3(0, i * CHUNK_SIZE, 0));
            subChunk.Mesh = Renderer.Render(subChunk);

            this.CallDeferred("add_child", subChunk);

            // Creating a visibility notifier per chunk.
            // This is useful for the frustrum culling.
            var visibilityNotifier = new VisibilityNotifier();
            var size = new Vector3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
            visibilityNotifier.Aabb = new AABB(new Vector3(), size);
            subChunk.AddChild(visibilityNotifier);
            visibilityNotifier.Connect("camera_entered", subChunk, "CameraEntered", null, 1);
            visibilityNotifier.Connect("camera_exited", subChunk, "CameraExited", null, 1);
                
        }
    }

    public void SetSubChunkVisibility(int idx, bool toggle)
    {
        m_SubChunks[idx].Visible = toggle;
    }
}

public class SubChunk : MeshInstance
{
    private Block[,,] m_Blocks = new Block[16, 16, 16];
    private int m_count = 0;
    private Chunk m_Chunk;

    public bool RenderTop = true;
    public bool RenderBottom = true;

    // Face flags
    private int m_TopCount = 0;
    private int m_BottomCount = 0;
    private int m_LeftCount = 0;
    private int m_RightCount = 0;
    private int m_FrontCount = 0;
    private int m_BackCount = 0;

    public bool isTopFull()
    {
        return m_TopCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
    }
    public bool isBottomFull()
    {
        return m_BottomCount == Math.Pow(Chunk.CHUNK_SIZE, 2);
    }

    // Select a region and returns true if the region is full.
    public static bool TestRegionIsFull(Vector3 start, Vector3 end, Block[,,] data)
    {
        for (int x = (int)start.x; x < end.x; x++)
            for (int y = (int)start.y; y <  end.y; y++)
                for (int z = (int)start.z; z < end.z; z++)
                {
                    // If one is not active then its not full.
                    if (!data[x, y, z].Active)
                        return false;
                }

        return true;
    }

    public bool isFull()
    {
        return m_count == Math.Pow(Chunk.CHUNK_SIZE, 3);
    }

    public bool isEmpty()
    {
        return m_count == 0;
    }

    public Block[,,] GetData()
    {
        return m_Blocks;
    }

    public void SetChunk(Chunk chunk)
    {
        this.m_Chunk = chunk;
    }

    public Chunk GetChunk()
    {
        return m_Chunk;
    }

    public void AddBlock(Vector3 position, Block block)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        // If we are adding an empty block, just remove it.
        if(block.Active == false)
        {
            RemoveBlock(position);
            return;
        }

        if (y == Chunk.CHUNK_SIZE - 1)
            m_TopCount++;
        if (y == 0)
            m_BottomCount++;
        if (x == Chunk.CHUNK_SIZE - 1)
            m_RightCount++;
        if (x == 0)
            m_LeftCount++;
        if (z == 0)
            m_BackCount++;
        if (z == Chunk.CHUNK_SIZE - 1)
            m_FrontCount++;

        m_Blocks[x, y, z] = block;
        m_count++;
    }

    public void RemoveBlock(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        if (y == Chunk.CHUNK_SIZE - 1)
            m_TopCount--;
        if (y == 0)
            m_BottomCount--;
        if (x == Chunk.CHUNK_SIZE - 1)
            m_RightCount--;
        if (x == 0)
            m_LeftCount--;
        if (z == Chunk.CHUNK_SIZE - 1)
            m_FrontCount--;
        if (z == 0)
            m_BackCount--;

        // Remove it and decrease the count.
        m_Blocks[x, y, z].Active = false;
        m_count--;
    }

    // Signals for frustrum culling
    public void CameraEntered(Camera camera)
    {
        if(camera.Name == "CameraInGame")
            this.Visible = true;
    }

    public void CameraExited(Camera camera)
    {
        if (camera.Name == "CameraInGame")
            this.Visible = false;
    }
}




