using Godot;
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

    // Chunks that are ready to be edited.
    private static Dictionary<Vector2, Chunk> m_PreloadedChunks = new Dictionary<Vector2, Chunk>();

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

        // Updates the preloaded chunks and generate 
        // second pass.
        UpdatePreloaded();

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
        for (int x = (int)CameraPosition.x - Engine.RenderDistance; x < (int)CameraPosition.x + Engine.RenderDistance; x++)
            for (int z = (int)CameraPosition.y - Engine.RenderDistance; z < (int)CameraPosition.y + Engine.RenderDistance; z++)
            {
                var chunkPosition = new Vector2(x, z);

                // If the chunk position is in the render radius
                // and is not already loaded, add it to the queue.
                if ((CameraPosition - chunkPosition).Length() < Engine.RenderDistance 
                    && !m_LoadedChunk.ContainsKey(chunkPosition) && !m_ChunkLoadList.Contains(chunkPosition)
                    && !m_RenderList.ContainsKey(chunkPosition) && !m_PreloadedChunks.ContainsKey(chunkPosition))
                {
                    //GD.Print(chunkPosition + " added to Load queue.");
                    m_ChunkLoadList.Add(chunkPosition);
                }
            }
    }


    /// <summary>
    /// Setups new chunks that should be generated. Chunks are the one enqueued by AsyncChunker.
    /// </summary>
    public static void UpdateLoadList()
    {
        int numLoadedChunks = 0;

        foreach (Vector2 chunkPos in m_ChunkLoadList.OrderBy(c => DistanceToChunk(c)))
        {
            if (m_LoadedChunk.ContainsKey(chunkPos))
                break;

            var newChunk = new Chunk();
            newChunk.SetPosition(chunkPos);

            // Setup
            newChunk.ChunkSetup();

            // Generate the landscape.
            NoiseMaker.GenerateChunk(newChunk);

            m_LoadedChunk.Add(chunkPos, newChunk);
        }

        m_ChunkLoadList.Clear();
    }


    /// <summary>
    /// Updates preloaded chunks and execute second pass of generation that necessite the chunk
    /// to be surrounded by loaded chunks. E.g: Trees that generates on chunk borders.
    /// </summary>
    public static void UpdatePreloaded()
    {
        for (int i = 0; i < m_LoadedChunk.Count; i++)
        {
            var chunk = m_LoadedChunk.ElementAt(i).Value;

            if (chunk.Updated == false)
                chunk.Update();

            if (chunk.isSurrounded && !chunk.Rendered && !m_RenderList.ContainsKey(chunk.Position))
            {
                //NoiseMaker.GenerateChunkDecoration(chunk);
                
                m_RenderList.Add(chunk.Position, chunk);
            }
        }

    }


    // Async rendering thread.
    public static void Render()
    {
        int numChunkRendered = 0;

        if (m_RenderList.Count <= 0)
            return;

        foreach (Chunk item in m_RenderList.Values.OrderBy(c => DistanceToChunk(c.Position)))
        {
            // Prevent from rendering too much chunks at once.
            if (numChunkRendered > Engine.chunkRenderedPerTick)
                return;

            //var timeStart = DateTime.Now;
            Chunk chunk = item;

            if (chunk.isSurrounded)
            {
                if (!Engine.Scene.HasNode(chunk.Position.ToString()))
                {
                    // Create mesh.
                    chunk.Render(true);
                    chunk.Rendered = true;
                    Engine.Scene.CallDeferred("add_child", chunk);
                }

                numChunkRendered++;

                m_RenderList.Remove(chunk.Position);
            }
        }
    }


    /// <summary>
    /// Checks if any loaded chunks should be unloaded.
    /// </summary>
    public static void UpdateUnloader()
    {
        List<Vector2> unloadedChunks = new List<Vector2>();

        foreach (Vector2 loadedChunkPos in m_LoadedChunk.Keys)
        {
            // If the chunk position is in the render radius. Add it to the queue.
            if ((CameraPosition - loadedChunkPos).Length() > Engine.RenderDistance * 2)
            {
                Chunk chunk = m_LoadedChunk[loadedChunkPos];
                chunk.Unload();

                unloadedChunks.Add(loadedChunkPos);

                // Remove any trace of the chunk in the memory to prevent
                // access to a unloaded chunks.
                if (m_RenderList.ContainsKey(chunk.Position))
                    m_RenderList.Remove(chunk.Position);

                if (m_PreloadedChunks.ContainsKey(chunk.Position))
                    m_PreloadedChunks.Remove(chunk.Position);
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


    public static void UnloadChunk(Chunk chunk)
    {
        //chunk.Unload();
        //if (m_LoadedChunk.ContainsKey(chunk.Position))
        //    m_LoadedChunk.TryRemove(chunk.Position, out chunk);
        //if (m_PreloadedChunks.ContainsKey(chunk.Position))
        //    m_PreloadedChunks.TryRemove(chunk.Position, out chunk);
        
    }
}