using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine;
using VoxelEngine.Engine;

public class ChunkManager
{
    // Stages
    private static Dictionary<Vector2, Chunk> m_ChunkLoadList = new Dictionary<Vector2, Chunk>();
    private static Dictionary<Vector2, Chunk> m_RenderList = new Dictionary<Vector2, Chunk>();

    // Loaded chunks
    private static Dictionary<Vector2,Chunk> m_LoadedChunk = new Dictionary<Vector2, Chunk>();

    private static int RenderPerCycle = 4;
    private static int maxCount = 0;

    public static void RenderThread()
    {
        if(m_RenderList.Count > 0)
        {
            var chunk = m_RenderList.ElementAt(0);
                
            chunk.Value.Render();
            Engine.Scene.CallDeferred("add_child", chunk.Value);

            m_RenderList.Remove(chunk.Key);
            
        }
        RenderThread();
    }

    public static void AddToLoadedList(Chunk chunk)
    {
        m_LoadedChunk.Add(chunk.Position, chunk);
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



    public static void AddToLoadList(Chunk chunk)
    {
        if (m_ChunkLoadList.ContainsKey(chunk.Position))
        {
            GD.Print("Warning: AddToLoadList->chunk already in list");
            return;
        }

        m_ChunkLoadList.Add(chunk.Position, chunk);
    }

    public static void AddToRenderList(Chunk chunk)
    {
        if (m_ChunkLoadList.ContainsKey(chunk.Position))
            m_ChunkLoadList.Remove(chunk.Position);

        m_RenderList.Add(chunk.Position, chunk);
    }

    public static void Update()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                Chunk chunk = new Chunk();
                chunk.SetPosition(new Vector2(x, z));
                chunk.ChunkSetup();
                AddToLoadList(chunk);
            }
        }
    }
}

