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
    // Stages
    private static ConcurrentQueue<Vector2> m_ChunkLoadList = new ConcurrentQueue<Vector2>();
    private static ConcurrentDictionary<Vector2, Chunk> m_PreloadedChunks = new ConcurrentDictionary<Vector2, Chunk>();
    private static ConcurrentDictionary<Vector2, Chunk> m_RenderList = new ConcurrentDictionary<Vector2, Chunk>();

    // All Loaded chunks
    private static Dictionary<Vector2,Chunk> m_LoadedChunk = new Dictionary<Vector2, Chunk>();


    public static void LoadThread()
    {
        //while (true)
        //{
            try
            {
                if (m_ChunkLoadList.Count > 0)
                {
                    foreach (var item in m_ChunkLoadList)
                    {
                        Vector2 newPosition = item;

                        var newChunk = new Chunk();
                        newChunk.SetPosition(newPosition);

                        newChunk.ChunkSetup();
                        NoiseMaker.GenerateChunk(newChunk);

                        newChunk.Update();
                        AddToLoadedList(newChunk);
                        AddToPreloadedList(newChunk, newChunk.Position);
                        m_ChunkLoadList.TryDequeue(out newChunk.Position);
                    }

                }
            }
            catch(Exception e)
            {
                GD.Print("Error in Loadthread - " + e);
            }
        //}
    }


    // Update current preloaded chunk. 
    public static void UpdatePreloaded()
    {
        try
        {
            if (m_PreloadedChunks.Count > 0)
            {
                foreach (var item in m_PreloadedChunks.Values)
                {
                    
                    var chunk = item;
                    chunk.Update();
                    if (chunk.isSurrounded)
                    {
                        AddToRenderList(chunk);
                        m_PreloadedChunks.TryRemove(chunk.Position, out chunk);

                    }
                }
            }
        }
        catch
        {
            
        }
        
    }

    // Async rendering thread.
    public static void RenderThread()
    {
        while (true)
        {
            try
            {
                if (m_RenderList.Count < 1)
                    continue;

                UpdatePreloaded();

                foreach (Chunk item in m_RenderList.Values)
                {
                    Chunk chunk = item;

                    if (chunk.isSurrounded)
                    {
                        // Create mesh.
                        chunk.Render();

                        // Queues the instancing.
                        Engine.Scene.CallDeferred("add_child", chunk);

                        // Remove from queue.
                        m_RenderList.TryRemove(chunk.Position, out chunk);
                    }
                }
            }
            catch(Exception e)
            {
                GD.Print(e.ToString());
            }
        }
    }

    // Returns true if a chunk all the chunk surrounding 
    // this one has been loaded(meaning that it's accesible).
    public static bool IsChunkSurrounded(Vector2 position)
    {
        bool left, right, front, back;
        left = m_LoadedChunk.ContainsKey(position + new Vector2(-1, 0));
        right = m_LoadedChunk.ContainsKey(position + new Vector2(1, 0));
        front = m_LoadedChunk.ContainsKey(position + new Vector2(0, -1));
        back = m_LoadedChunk.ContainsKey(position + new Vector2(0, 1));
        return left && right && front && back;
    }


    public static void AddToPreloadedList(Chunk chunk, Vector2 position)
    {
        m_PreloadedChunks.TryAdd(position, chunk);
    }

    // Adds a chunk to the loaded chunk list.
    public static void AddToLoadedList(Chunk chunk)
    {
        m_LoadedChunk.Add(chunk.Position, chunk);
        
        
    }

    // Adds a chunk to the loading chunk list.
    public static void AddLoadQueue(Vector2 position)
    {
        if (m_ChunkLoadList.Contains(position))
        {
            GD.Print("Warning: AddToLoadList->chunk already in list");
            return;
        }

        m_ChunkLoadList.Enqueue( position);
    }

    // Adds a chunk to the render chunk list.
    public static void AddToRenderList(Chunk chunk)
    {
        m_RenderList.TryAdd(chunk.Position, chunk);
    }

    // Gets a chunk
    public static Chunk GetChunk(Vector2 position)
    {
        if (m_LoadedChunk.ContainsKey(position))
            return m_LoadedChunk[position];
        else
            throw new System.IndexOutOfRangeException("Attempted to get a non-loaded chunk at: " + position.ToString() + ".");
    }
}

