using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Chunk : Spatial
{
    public static int CHUNK_SIZE = 16;
    public static int SUBCHUNK_COUNT = 16;

    public Vector2 Position = new Vector2();

    public bool isSetup = false;

    public bool isSurrounded = false;

    public bool needUpdate = false;

    public bool Updated = false;

    public bool isRendered = false;


    public BIOME_TYPE Biome = BIOME_TYPE.Plains;

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

        isSetup = true;
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
        if (ChunkLeft != null)
            ChunkLeft.Updated = false;
        if (ChunkFront != null)
            ChunkFront.Updated = false;
        if (ChunkRight != null)
            ChunkRight.Updated = false;
        if (ChunkBack != null)
            ChunkBack.Updated = false;

        this.QueueFree();
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

        return GetBlock(x, y, z);
    }


    // Returns a block from global position
    public int GetBlock(int x, int y, int z)
    {
        if (!IsPositionValid(x, y, z))
        {

            GD.Print("position invalid:" + new Vector3(x, y, z) + ". Chunk at:" + this.Position);
            return -1;
        }
            
        int subChunkIndex = GetSubChunkIdFromHeight(y);
        int subChunkHeight = y - (CHUNK_SIZE * (subChunkIndex));
        return m_SubChunks[subChunkIndex].GetBlock(x, subChunkHeight, z);
    }


    // Adds a block into a subchunk from globalposition using Vector3.
    public void AddBlock(Vector3 position, BLOCK_TYPE block)
    {
        AddBlock((int)position.x, (int)position.y, (int)position.z, block);
    }

    // Adds a block into a subchunk from globalposition using ints.
    public void AddBlock(int x, int y, int z, BLOCK_TYPE block)
    {

        if(!IsPositionValid(x, y, z))
        {
            GD.Print("Invalid block palced");
            PlaceInvalidBlock(x, y, z, block);
            return;
        }

        int subChunkIndex = GetSubChunkIdFromHeight(y);
        int subChunkHeight = y - ((int)CHUNK_SIZE * (subChunkIndex));
        var localPosition = new Vector3(x, subChunkHeight, z);

        m_SubChunks[subChunkIndex].AddBlock(localPosition, block);
    }

    // Returns true if the given position is inside the chunk boundaries.
    private bool IsPositionValid(int x, int y, int z)
    {
        if(x < 0 || x > 15 || z < 0 || z > 15)
            return false;

        return true;
    }


    // Place an invalid block into the right neigbhor chunk.
    private void PlaceInvalidBlock(int x, int y, int z, BLOCK_TYPE block)
    {
        block = BLOCK_TYPE.Water;

        int x2 = x, z2 = z;
        if(x < 0)
        {
            x2 += 16;
            this.ChunkLeft.AddBlock(x2, y, z2, block);
            return;
        }
        
        if(z < 0)
        {
            z2 = 16 + z2;
            this.ChunkBack.AddBlock(x2, y, z2, block);
            return;
        }

        if(x > 15)
        {
            x2 -= 16;
            this.ChunkRight.AddBlock(x2, y, z2, block);
            return;
        }
            
        if(z > 15)
        {
            z2 -= 16;
            this.ChunkFront.AddBlock(x2, y, z2, block);
            return;
        }
    }


    public void AddBlocks(int[,,] data, Vector3 Origin, bool overwrite = false)
    {
         for(int x = 0; x < data.GetLength(0); x++)
            for(int y = 0; y < data.GetLength(1); y++)
                for(int z = 0; z < data.GetLength(2); z++)
                {
                    var block = data[x, y, z];

                    AddBlock(Origin + new Vector3(x, y, z), (BLOCK_TYPE)block);
                }
    }


    public void AddSprite(Vector3 position, Models model)
    {
        int subChunkIndex = GetSubChunkIdFromHeight((int)position.y);

        int subChunkHeight = (int)position.y - ((int)CHUNK_SIZE * (subChunkIndex));
        var localPosition = new Vector3(position.x, subChunkHeight, position.z);

        var mesh = ModelLoader.GetModel(model);
        var sprite = new VoxelSprite(mesh, localPosition);

        m_SubChunks[subChunkIndex].AddDecoration(sprite);

    }


    public int HighestBlockAt(int x, int z)
    {
        // Scan from top of the chunk.
        // Continue until there is a solid block. != -1
        // else return bottom of chunk.
        for(int i = 255; i > 0; i--)
        {
            if(this.GetBlock(x, i, z) != -1)
                return i;
        }
        return 0;
    }


    /// <summary>
    /// Renders a chunk from its data. If the chunk is rendered
    /// for the first time. It adds it to the scene. If not, it 
    /// just replaces its mesh with the new one.
    /// </summary>
    /// <param name="firstRender"></param>
    public void Render(bool firstRender)
    {
        if (firstRender)
        {
            for (int i = 0; i < SUBCHUNK_COUNT; i++)
            {
                // Create each sub chunks and place them
                var subChunk = m_SubChunks[i];
                subChunk.Name = i.ToString();
                subChunk.Translate(new Vector3(0, i * CHUNK_SIZE, 0));

                // Creates the mesh.
                subChunk.Mesh = Renderer.Render(subChunk);

                // If the subchunk is not empty, create the collision.
                if(subChunk.Mesh != null)
                    subChunk.CreateTrimeshCollision();

                // Place each voxel sprite in the chunk.
                foreach (VoxelSprite voxelSprite in subChunk.GetDecorations())
                {
                    if (voxelSprite is null)
                        continue;

                    var newMesh = new MeshInstance();
                    newMesh.Scale = new Vector3(1f / 8f, 1f / 8f, 1f / 8f);
                    newMesh.AddToGroup("decoration");
                    newMesh.MaterialOverride = Renderer.GetWavingShader;
                    newMesh.SetDeferred("translation", voxelSprite.Position);
                    newMesh.Mesh = voxelSprite.Mesh;
                    subChunk.CallDeferred("add_child", newMesh);
                }

                this.CallDeferred("add_child", subChunk);
                
                // Creating a visibility notifier per chunk.
                // This is useful for the frustrum culling.
                // Connect the signals to the methods on the subchunk.
                //visibilityNotifier.Connect("camera_entered", subChunk, "CameraEntered", null, 1);
                //visibilityNotifier.Connect("camera_exited", subChunk, "CameraExited", null, 1);
            }
        }
        else
        {
            for (int i = 0; i < SUBCHUNK_COUNT; i++)
            {
                var subChunk = m_SubChunks[i];
                subChunk.Mesh = Renderer.Render(subChunk);

                // removes old collisions
                foreach (Node node in subChunk.GetChildren())
                {
                    node.CallDeferred("queue_free");
                }
                // Update collisions too
                if (subChunk.Mesh != null)
                    subChunk.CreateTrimeshCollision();
            }

            needUpdate = false;
        }

    }


    // Force set a subchunk to be visible or not.
    public void SetSubChunkVisibility(int idx, bool toggle)
    {
        // m_SubChunks[idx].Visible = toggle;
    }
}






