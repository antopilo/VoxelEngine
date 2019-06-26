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
    // Load queue.
    private static Queue<Vector2> m_ChunkLoadList = new Queue<Vector2>();

    // Loaded chunk.
    private static Dictionary<Vector2, Chunk> m_PreloadedChunks = new Dictionary<Vector2, Chunk>();

    // Render queue.
    private static Dictionary<Vector2, Chunk> m_RenderList = new Dictionary<Vector2, Chunk>();

    // All Loaded chunks
    private static Dictionary<Vector2,Chunk> m_LoadedChunk = new Dictionary<Vector2, Chunk>();

    public static Vector2 CameraPosition = new Vector2();

    public static void Load()
    {     
        foreach (var item in m_ChunkLoadList.ToArray())
        {
            var timeStart = DateTime.Now;

            Vector2 newPosition = m_ChunkLoadList.Dequeue(); ;

            var newChunk = new Chunk();
            newChunk.SetPosition(newPosition);

            newChunk.ChunkSetup();
            NoiseMaker.GenerateChunk(newChunk);

            
            AddToLoadedList(newChunk);
            AddToPreloadedList(newChunk, newChunk.Position);

            var timeEnd = DateTime.Now;
            GD.Print("Loading took:",(timeEnd - timeStart).TotalMilliseconds.ToString(),"ms");
        }
    }


    // Update current preloaded chunk. 
    public static void UpdatePreloaded()
    {
        foreach (var item in m_PreloadedChunks.ToArray())
        {
            var chunk = item.Value;
            chunk.Update();

            if (chunk.isSurrounded)
            {
                AddToRenderList(chunk);
                m_PreloadedChunks.Remove(chunk.Position);
            }
        }
    }

    // Async rendering thread.
    public static void Render()
    {
        foreach (Chunk item in m_RenderList.Values.ToArray())
        {
            var timeStart = DateTime.Now;
            Chunk chunk = item;
                    
            if (chunk.isSurrounded)
            {
                // Create mesh.
                chunk.Render();

                // Queues the instancing.
                Engine.Scene.CallDeferred("add_child", chunk);

                // Remove from queue.
                m_RenderList.Remove(chunk.Position);
                var timeEnd = DateTime.Now;
                GD.Print("Meshing took:",(timeEnd - timeStart).TotalMilliseconds.ToString(), "ms");
            }        
        }
    }


    public static float DistanceToChunk(Vector2 position)
    {
        return (position - CameraPosition).Length();
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

    public static int GetLoadedCount()
    {
        return m_LoadedChunk.Count();
    }


    public static void AddToPreloadedList(Chunk chunk, Vector2 position)
    {
        m_PreloadedChunks.Add(position, chunk);
    }

    // Adds a chunk to the loaded chunk list.
    public static void AddToLoadedList(Chunk chunk)
    {
        m_LoadedChunk.Add(chunk.Position, chunk);
    }

    // Adds a chunk to the loading chunk list.
    public static void AddLoadQueue(Vector2 position)
    {
        if (m_ChunkLoadList.Contains(position) || m_LoadedChunk.ContainsKey(position))
        {
            return;
        }

        m_ChunkLoadList.Enqueue( position);
    }

    // Adds a chunk to the render chunk list.
    public static void AddToRenderList(Chunk chunk)
    {
        m_RenderList.Add(chunk.Position, chunk);
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

