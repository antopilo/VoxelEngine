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

    private SubChunk[] m_SubChunks = new SubChunk[SUBCHUNK_COUNT];
    
    public SubChunk GetSubChunk(int idx)
    {
        return m_SubChunks[idx];
    }

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

    public void ChunkSetup()
    {
        // Filling all subchunks
        for (int sc = 0; sc < SUBCHUNK_COUNT; sc++)
        {
            m_SubChunks[sc] = new SubChunk();
            m_SubChunks[sc].SubChunkId = sc;
            m_SubChunks[sc].Fill(false);
            m_SubChunks[sc].Chunk = this;
        }


        var defaultBlock = new Block();
        defaultBlock.Active = false;   

        Update();
    }

    public void SetPosition(Vector2 position)
    {
        int x = (int)position.x * (int)CHUNK_SIZE;
        int z = (int)position.y * (int)CHUNK_SIZE;

        this.Position = position;
        this.Translation = new Vector3(x, 0, z);
    }

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
    }

    public static int GetSubChunkIdFromHeight(int heigth)
    {
        return Mathf.Clamp((heigth / CHUNK_SIZE), 0, CHUNK_SIZE - 1);
    }

    public Block GetBlock(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;
        return GetBlock(x, y, z);
    }

    public Block GetBlock(int x, int y, int z)
    {
        int subChunkIndex = GetSubChunkIdFromHeight(y);
        int subChunkHeight = y - (CHUNK_SIZE * (subChunkIndex));
        return m_SubChunks[subChunkIndex].GetBlock(x, subChunkHeight, z);
    }

    public void AddBlock(Vector3 position, Block block)
    {
        int subChunkIndex = GetSubChunkIdFromHeight((int)position.y);
        int subChunkHeight = (int)position.y - ((int)CHUNK_SIZE * (subChunkIndex));
        var localPosition = new Vector3(position.x, subChunkHeight, position.z);
        m_SubChunks[subChunkIndex].AddBlock(localPosition, block);

    }

    // Renders all the chunk, including the subchunks.
    public void Render()
    {
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






