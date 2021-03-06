﻿using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

//                  Summary
// Singleton managin every thing chunk related.
// it handles all the stages of chunk loading and
// handles the render calls. It uses 2 threads,
// one for loading and another one for rendering.
//

public class ChunkManager
{
    // Chunks that are not loaded and should be.
    private static List<Vector2> m_ChunkLoadList = new List<Vector2>();

    // rebuild list
    private static List<Vector2> m_ChunkRebuildList = new List<Vector2>();
    
    // Chunks that are fully generated and are waiting to be rendered.
    private static Dictionary<Vector2, Chunk> m_RenderList = new Dictionary<Vector2, Chunk>();

    // Chunks that are fully generated and ready to be accessed
    private static Dictionary<Vector2, Chunk> m_LoadedChunk = new Dictionary<Vector2, Chunk>();



    public static Camera Camera;
    public static Vector2 CameraPosition = new Vector2();
    

    public static void Update()
    {
        // Checks if new chunk should be loaded.
        UpdateAsyncChunker();

        // Setup the new chunks that were loaded above.
        UpdateLoadList();

        
        UpdateRebuildList();

        // Updates the preloaded chunks and generate 
        // second pass.
        UpdateLoaded();

        // Render the fully loaded chunks.
        Render();

        // Checks if any loaded chunks should be unloaded
        UpdateUnloader();
    }


    /// <summary>
    /// Method that updates on a another thread.
    /// Should be called from a thread or it will lock the engine.
    /// </summary>
    public static void UpdateThread()
    {
        while (true)
        {
            Update();
        }
    }

    /// <summary>
    /// Checks if any new chunk should be loaded around the camera.
    /// </summary>
    public static void UpdateAsyncChunker()
    {
        int numChunkQueued = 0;

        for (int x = (int)CameraPosition.x - Engine.RenderDistance; x < (int)CameraPosition.x + Engine.RenderDistance; x++)
            for (int z = (int)CameraPosition.y - Engine.RenderDistance; z < (int)CameraPosition.y + Engine.RenderDistance; z++)
            {
                if (numChunkQueued > 8)
                    break;

                var chunkPosition = new Vector2(x, z);

                // If the chunk position is in the render radius
                // and is not already loaded, add it to the queue.
                if ((CameraPosition - chunkPosition).Length() < Engine.RenderDistance 
                    && !m_LoadedChunk.ContainsKey(chunkPosition) && !m_ChunkLoadList.Contains(chunkPosition))
                {
                    //GD.Print(chunkPosition + " added to Load queue.");
                    m_ChunkLoadList.Add(chunkPosition);
                    numChunkQueued++;
                }
            }
    }


    /// <summary>
    /// Setups new chunks that should be generated. Chunks are the one enqueued by AsyncChunker.
    /// </summary>
    public static void UpdateLoadList()
    {
        int numLoadedChunk = 0;
        foreach (Vector2 chunkPos in m_ChunkLoadList.OrderBy(c => DistanceToChunk(c)))
        {
            if (numLoadedChunk > 8)
                break;

            var newChunk = new Chunk();
            newChunk.SetPosition(chunkPos);

            // Setup
            newChunk.ChunkSetup();

            m_LoadedChunk.Add(chunkPos, newChunk);
            numLoadedChunk++;
        }

        m_ChunkLoadList.Clear();
    }


    public static void UpdateRebuildList()
    {
        var RebuildedChunks = new List<Vector2>();
        foreach (var chunkPosition in m_ChunkRebuildList)
        {
            GD.Print("Rebuilded");
            // Get the chunk
            Chunk chunk = m_LoadedChunk[chunkPosition];
            chunk.Render(false);

            RebuildedChunks.Add(chunkPosition);
        }

        foreach (var rebuilded in RebuildedChunks)
        {
            m_ChunkRebuildList.Remove(rebuilded);
        }
    }


    /// <summary>
    /// Updates preloaded chunks and execute second pass of generation that necessite the chunk
    /// to be surrounded by loaded chunks. E.g: Trees that generates on chunk borders.
    /// </summary>
    public static void UpdateLoaded()
    {
        int numUpdatedChunk = 0;

        foreach (Chunk chunk in m_LoadedChunk.Values)
        {
            if (numUpdatedChunk > 8)
                break;

            if (chunk.Updated == false)
                chunk.Update();

            if (chunk.isSurrounded && !m_RenderList.ContainsKey(chunk.Position) && !chunk.isRendered)
            {
                m_RenderList.Add(chunk.Position, chunk);
                chunk.isRendered = true;
                NoiseMaker.GenerateChunk(chunk);
                numUpdatedChunk++;
                //NoiseMaker.GenerateChunkDecoration(chunk);
                // Generate the landscape.
            }
        }
    }


    // Async rendering thread.
    public static void Render()
    {
        int numChunkRendered = 0;

        // If there is no chunk to render skip.
        if (m_RenderList.Count == 0)
            return;

        foreach (Chunk item in m_RenderList.Values.OrderBy(c => DistanceToChunk(c.Position)))
        {
            // Prevent from rendering too much chunks at once.
            if (numChunkRendered > Engine.chunkRenderedPerTick)
                return;

            //var timeStart = DateTime.Now;
            Chunk chunk = item;


            if (!Engine.Scene.HasNode(chunk.Position.ToString()))
            {
                // Create mesh.
                chunk.Render(true);

                Engine.Scene.CallDeferred("add_child", chunk);
            }

            numChunkRendered++;

            m_RenderList.Remove(chunk.Position);
        }
    }


    /// <summary>
    /// Checks if any loaded chunks should be unloaded.
    /// </summary>
    public static void UpdateUnloader()
    {
        int numUnloaded = 0;
        // List of chunk that will be unloaded after the scan.
        List<Vector2> unloadedChunks = new List<Vector2>();

        // Scan for chunk that should be unloaded, then adds them to the list.
        foreach (Vector2 loadedChunkPos in m_LoadedChunk.Keys)
        {
            if (numUnloaded > 8)
                break;

            // If the chunk position is outside the render distance.
            if ((CameraPosition - loadedChunkPos).Length() > Engine.RenderDistance * 2)
            {
                // Get the chunk and call unload on it.
                Chunk chunk = m_LoadedChunk[loadedChunkPos];
                chunk.Unload();

                // Add the chunk to the list.
                unloadedChunks.Add(loadedChunkPos);

                // Remove any trace of the chunk in the memory to prevent
                // access to a unloaded chunks.
                if (m_RenderList.ContainsKey(chunk.Position))
                    m_RenderList.Remove(chunk.Position);

                numUnloaded++;
            }
        }

        // Reiterate and remove all unloaded chunks.
        for (int i = 0; i < unloadedChunks.Count; i++)
        {
            m_LoadedChunk.Remove(unloadedChunks[i]);
        }
    }


    public static float DistanceToChunk(Vector2 position)
    {
        return (position - CameraPosition).Length();
    }


    public static bool ShouldRenderChunk(Vector2 chunkPosition)
    {
        Vector3 result = new Vector3(chunkPosition.x * 16, Camera.GlobalTransform.origin.y, chunkPosition.y * 16);
        Vector2 position = Camera.UnprojectPosition(result);
        if (position.x < 0 || position.x > 800 || position.y < 0 || position.y > 600)
            return false;
        return true;
    }


    public static bool IsChunkSurrounded(Vector2 position)
    {
        bool left, right, front, back;
        left = m_LoadedChunk.ContainsKey(position + new Vector2(-1, 0));
        right = m_LoadedChunk.ContainsKey(position + new Vector2(1, 0));
        front = m_LoadedChunk.ContainsKey(position + new Vector2(0, -1));
        back = m_LoadedChunk.ContainsKey(position + new Vector2(0, 1));

        return left && right && front && back;
    }


    public static int GetLoadedCount()
    {
        return m_LoadedChunk.Count();
    }


    // Gets a chunk
    public static Chunk GetChunk(Vector2 position)
    {
        if (m_LoadedChunk.ContainsKey(position))
            return m_LoadedChunk[position];
        else
            return null;
    }
    
    
    /// <summary>
    /// Returns the block type at a given global position.
    /// </summary>
    /// <param name="position">Global voxel position</param>
    /// <returns></returns>
    public static int GetBlock(Vector3 position)
    {
        var globalPosition = StepifyVector(position);

        var localX = Mathf.Abs(globalPosition.x % 16);
        var localZ = Mathf.Abs(globalPosition.z % 16);
        var localPosition = new Vector3(localX, globalPosition.y, localZ);

        int chunkX = (int)Mathf.Stepify(globalPosition.x, 16) / 16;
        int chunkZ = (int)Mathf.Stepify(globalPosition.z, 16) / 16;
        var chunkPosition = new Vector2(chunkX, chunkZ);

        var chunk = GetChunk(chunkPosition);


        return chunk.GetBlock(localPosition);
    }

    public static void SetBlock(Vector3 position, BLOCK_TYPE block)
    {
        var globalPosition = StepifyVector(position);

        int localX = (int)globalPosition.x % 16;
        int localZ = (int)globalPosition.z % 16;

        if (localX < 0)
            localX += 16;
        if (localZ < 0)
            localZ += 16;

        var localPosition = new Vector3(localX, globalPosition.y, localZ);

        int chunkX = (int)Mathf.Stepify(globalPosition.x, 16) / 16;
        int chunkZ = (int)Mathf.Stepify(globalPosition.z, 16) / 16;
        var chunkPosition = new Vector2(chunkX, chunkZ);

        var chunk = GetChunk(chunkPosition);

        chunk.AddBlock(localPosition, block);

        if (!m_ChunkRebuildList.Contains(chunk.Position))
            m_ChunkRebuildList.Add(chunk.Position);
    }


    public static void RemoveBlock(Vector3 position)
    {
        SetBlock(position, BLOCK_TYPE.Empty);
    }
    

    /// <summary>
    /// Converts a global position from the space and returns the global
    /// voxel position.
    /// </summary>
    /// <param name="position"></param>
    private static Vector3 StepifyVector(Vector3 position)
    {
        float x, y, z;
        x = (int)Mathf.Stepify(position.x, 1);
        y = (int)Mathf.Stepify(position.y, 1);
        z = (int)Mathf.Stepify(position.z, 1);

        return new Vector3(x, y, z);

    }

    public static void UnloadChunk(Chunk chunk)
    {
        //chunk.Unload();
        //if (m_LoadedChunk.ContainsKey(chunk.Position))
        //    m_LoadedChunk.TryRemove(chunk.Position, out chunk);
        //if (m_PreloadedChunks.ContainsKey(chunk.Position))
        //    m_PreloadedChunks.TryRemove(chunk.Position, out chunk);
        
    }
}