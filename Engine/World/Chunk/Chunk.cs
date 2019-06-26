using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine;
using VoxelEngine.engine.World;
using VoxelEngine.Engine.World;

public class Chunk : Spatial
{
    public static int CHUNK_SIZE = 16;
    public static int SUBCHUNK_COUNT = 16;

    public Vector2 Position = new Vector2();
    public bool isSurrounded = false;
    public bool Updated = false;
    public bool Unloaded = true;

    private SubChunk[] m_SubChunks = new SubChunk[SUBCHUNK_COUNT];
    #region Neighbors 
    public Chunk ChunkLeft
    {
        get
        {
            if (isSurrounded)
                return ChunkManager.GetChunk(Position + new Vector2(-1, 0));
            else
                return null;
        }
    }

    public Chunk ChunkRight
    {
        get
        {
            if (isSurrounded)
                return ChunkManager.GetChunk(Position + new Vector2(1, 0));
            else
                return null;
        }
    }

    public Chunk ChunkFront
    {
        get
        {
            if (isSurrounded)
                return ChunkManager.GetChunk(Position + new Vector2(0, 1));
            else
                return null;
        }
    }

    public Chunk ChunkBack
    {
        get
        {
            if (isSurrounded)
                return ChunkManager.GetChunk(Position + new Vector2(0, -1));
            else
                return null;
        }
    }
    #endregion


    public void ChunkSetup()
    {
        this.Name = Position.ToString();

        // Filling all subchunks
        for (int sc = 0; sc < SUBCHUNK_COUNT; sc++)
        {
            m_SubChunks[sc] = new SubChunk();
            m_SubChunks[sc].SubChunkId = sc;
            m_SubChunks[sc].Fill(true);
            m_SubChunks[sc].Chunk = this;
        }


    }


    // Sets the chunk position used in generation.
    public void SetPosition(Vector2 position)
    {
        int x = (int)position.x * (int)CHUNK_SIZE;
        int z = (int)position.y * (int)CHUNK_SIZE;

        this.Position = position;
        this.Translation = new Vector3(x, 0, z);
    }


    // Updates all the flags in each subchunks.
    public void Update()
    {
        isSurrounded = ChunkManager.IsChunkSurrounded(Position);
        for (int i = 0; i < SUBCHUNK_COUNT; i++)
        {
            if (isSurrounded)
            {
                var subChunk = m_SubChunks[i];

                // Top & Bottom.
                if (i != 15 && subChunk.isTopFull() && m_SubChunks[i + 1].isBottomFull())
                    subChunk.RenderTop = false;
                else
                    subChunk.RenderTop = true;

                if (i != 0 && subChunk.isBottomFull() && m_SubChunks[i - 1].isTopFull())
                    subChunk.RenderBottom = false;
                else
                    subChunk.RenderBottom = false;

                // Left & Right.
                if (subChunk.isLeftFull() && this.ChunkLeft.GetSubChunk(i).isRightFull())
                    subChunk.RenderLeft = false;
                else
                    subChunk.RenderLeft = true;

                if (subChunk.isRightFull() && this.ChunkRight.GetSubChunk(i).isLeftFull())
                    subChunk.RenderRight = false;
                else
                    subChunk.RenderRight = true;

                // Front & Back.
                if (subChunk.isFrontFull() && this.ChunkFront.GetSubChunk(i).isBackFull())
                    subChunk.RenderFront = false;
                else
                    subChunk.RenderFront = true;

                if (subChunk.isBackFull() && this.ChunkBack.GetSubChunk(i).isFrontFull())
                    subChunk.RenderBack = false;
                else
                    subChunk.RenderBack = true;
                
            }
        }
        if(isSurrounded)
            Updated = true;

    }


    // Gets the subchunk index from global height.
    public static int GetSubChunkIdFromHeight(int height)
    {
        return Mathf.Clamp((height / CHUNK_SIZE), 0, CHUNK_SIZE - 1);
    }


    public override void _Process(float delta)
    {
        if (ChunkManager.DistanceToChunk(this.Position) > Engine.RenderDistance)
        {
            this.Visible = false;
            //for (int i = 0; i < SUBCHUNK_COUNT; i++)
            //{

            //    //ChunkManager.UnloadChunk(this);
            //}
        }
        else
            this.Visible = true;
    }

    public void Unload()
    {
        this.CallDeferred("queue_free");
    }


    // Gets a subchunk from an index.
    public SubChunk GetSubChunk(int idx)
    {
        return m_SubChunks[idx];
    }


    // Returns a block from global position
    public int GetBlock(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        // Skip duplicate code.
        return GetBlock(x, y, z);
    }


    // Returns a block from global position
    public int GetBlock(int x, int y, int z)
    {
        int subChunkIndex = GetSubChunkIdFromHeight(y);
        int subChunkHeight = y - (CHUNK_SIZE * (subChunkIndex));
        return m_SubChunks[subChunkIndex].GetBlock(x, subChunkHeight, z);
    }


    // Adds a block into a subchunk from global position.
    public void AddBlock(Vector3 position, BLOCK_TYPE block)
    {
        int subChunkIndex = GetSubChunkIdFromHeight((int)position.y);
        int subChunkHeight = (int)position.y - ((int)CHUNK_SIZE * (subChunkIndex));
        var localPosition = new Vector3(position.x, subChunkHeight, position.z);
        m_SubChunks[subChunkIndex].AddBlock(localPosition, block);
        Updated = false;
    }


    // Renders all the chunk, including the subchunks.
    public void Render(bool first)
    {
        if (first)
        {
            var visibilityNotifier = new VisibilityNotifier();
            var size = new Vector3(CHUNK_SIZE, CHUNK_SIZE * SUBCHUNK_COUNT, CHUNK_SIZE);
            visibilityNotifier.Aabb = new AABB(new Vector3(), size);
            this.AddChild(visibilityNotifier);
            for (int i = 0; i < SUBCHUNK_COUNT; i++)
            {
                var subChunk = m_SubChunks[i];
                subChunk.Name = i.ToString();
                subChunk.Translate(new Vector3(0, i * CHUNK_SIZE, 0));
                subChunk.Mesh = Renderer.Render(subChunk);

                this.CallDeferred("add_child", subChunk);

                // Creating a visibility notifier per chunk.
                // This is useful for the frustrum culling.
                // Connect the signals to the methods on the subchunk.
                visibilityNotifier.Connect("camera_entered", subChunk, "CameraEntered", null, 1);
                visibilityNotifier.Connect("camera_exited", subChunk, "CameraExited", null, 1);
            }
        }
        else
        {
            for (int i = 0; i < SUBCHUNK_COUNT; i++)
            {
                var subChunk = m_SubChunks[i];
                subChunk.Mesh = Renderer.Render(subChunk);
            }
        }

    }


    // Force set a subchunk to be visible or not.
    public void SetSubChunkVisibility(int idx, bool toggle)
    {
        m_SubChunks[idx].Visible = toggle;
    }
}






