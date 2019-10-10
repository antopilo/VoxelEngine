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
    private static ConcurrentQueue<Vector2> m_ChunkLoadList = new ConcurrentQueue<Vector2>();

    // Loaded chunk.
    private static ConcurrentDictionary<Vector2, Chunk> m_PreloadedChunks = new ConcurrentDictionary<Vector2, Chunk>();

    // Render queue.
    private static ConcurrentDictionary<Vector2, Chunk> m_RenderList = new ConcurrentDictionary<Vector2, Chunk>();

    // All Loaded chunks
    private static ConcurrentDictionary<Vector2,Chunk> m_LoadedChunk = new ConcurrentDictionary<Vector2, Chunk>();

    public static Vector2 CameraPosition = new Vector2();
    public static Camera Camera;

    public static void Load()
    {
        while (true)
        {
            try
            {
                for (int x = (int)CameraPosition.x - Engine.RenderDistance; x < (int)CameraPosition.x + Engine.RenderDistance; x++)
                {
                    for (int z = (int)CameraPosition.y - Engine.RenderDistance; z < (int)CameraPosition.y + Engine.RenderDistance; z++)
                    {
                        var currentPosition = new Vector2(x, z);
                        if ((CameraPosition - currentPosition).Length() < Engine.RenderDistance 
                            && ShouldRenderChunk(currentPosition))
                            AddLoadQueue(currentPosition);
                        
                    }
                }

                int count = 0;
                foreach (var item in m_ChunkLoadList)
                {
                    if (count > 2)
                    {
                        //UpdatePreloaded();
                        break;
                    }

                    //var timeStart = DateTime.Now;
                    Vector2 newPosition;
                    bool result = m_ChunkLoadList.TryDequeue(out newPosition); ;

                    if (result)
                    {
                        var newChunk = new Chunk();
                        newChunk.SetPosition(newPosition);

                        newChunk.ChunkSetup();
                        NoiseMaker.GenerateChunk(newChunk);


                        AddToLoadedList(newChunk);
                        AddToPreloadedList(newChunk, newChunk.Position);

                        //var timeEnd = DateTime.Now;
                        //GD.Print("Loading took:", (timeEnd - timeStart).TotalMilliseconds.ToString(), "ms");
                        count++;
                    }
                }
            }
            catch(Exception e)
            {
                GD.Print(e.ToString());
            }
            
        }
    }


    // Update current preloaded chunk. 
    public static void UpdatePreloaded()
    {
        try
        {
            for (int i = 0; i < m_PreloadedChunks.Count; i++)
            {
                

                var chunk = m_PreloadedChunks.ElementAt(i).Value;

                if (chunk.Updated == false)
                    chunk.Update();

                Chunk n = null;
                if (chunk.isSurrounded)
                {
                    NoiseMaker.GenerateVegetation(chunk);
                    AddToRenderList(chunk);
                    m_PreloadedChunks.TryRemove(chunk.Position, out n);
                }
            }
        }
        catch { }
        
    }

    // Async rendering thread.
    public static void Render()
    {
        while (true)
        {
            try
            {
                if (m_RenderList.Count <= 1)
                    continue;

                foreach (Chunk item in m_RenderList.Values)
                {
                    //var timeStart = DateTime.Now;
                    Chunk chunk = item;

                    if (chunk.isSurrounded)
                    {
                        if (!Engine.Scene.HasNode(chunk.Position.ToString()))
                        {

                            // Create mesh.
                            chunk.Render(true);

                            Engine.Scene.CallDeferred("add_child", chunk);
                        }
                        else
                        {
                            chunk.Render(false);
                        }

                        // Queues the instancing.
                        
                            

                        Chunk n = null;
                        // Remove from queue.
                        m_RenderList.TryRemove(chunk.Position, out n);
                        //var timeEnd = DateTime.Now;
                        //GD.Print("Meshing took:", (timeEnd - timeStart).TotalMilliseconds.ToString(), "ms");
                    }
                }
            }
            catch (Exception e)
            {
                GD.Print(e.ToString());
            }
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
        m_PreloadedChunks.TryAdd(position, chunk);
    }

    // Adds a chunk to the loaded chunk list.
    public static void AddToLoadedList(Chunk chunk)
    {
        m_LoadedChunk.TryAdd(chunk.Position, chunk);
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

    public static void UnloadChunk(Chunk chunk)
    {
        //chunk.Unload();
        //if (m_LoadedChunk.ContainsKey(chunk.Position))
        //    m_LoadedChunk.TryRemove(chunk.Position, out chunk);
        //if (m_PreloadedChunks.ContainsKey(chunk.Position))
        //    m_PreloadedChunks.TryRemove(chunk.Position, out chunk);
        
    }
}

