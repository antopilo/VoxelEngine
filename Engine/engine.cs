using Godot;
using System;
using System.Threading;
using VoxelEngine.engine;
using VoxelEngine.engine.World;
using VoxelEngine.Engine.World;
using Thread = System.Threading.Thread;

// TODOs:
//  - Threads, 1 preload, 1 Render
//  - remove side chunk of the mesh
//  - NoiseMachine.
//  - Update chunks

public enum Threads
{
    PRELOAD, RENDER
}

public class Engine : Node
{
    public static Node Scene { get; set; }

    private static Thread[] m_Threads = new Thread[2];
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var threadStart1 = new ThreadStart(ChunkManager.LoadThread);
        var threadStart2 = new ThreadStart(ChunkManager.RenderThread);

        // Initialize threads
        m_Threads[(int)Threads.PRELOAD] = new Thread(threadStart1);
        m_Threads[(int)Threads.RENDER] = new Thread(threadStart2);

        Scene = GetTree().CurrentScene;

        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                ChunkManager.AddLoadQueue(new Vector2(x, z));
            }
        }

        //ChunkManager.LoadThread();
        

        //m_Threads[(int)Threads.PRELOAD].Start();
        m_Threads[(int)Threads.RENDER].Start();
    }

    public override void _Process(float delta)
    {
        ChunkManager.UpdatePreloaded();
        ChunkManager.LoadThread();
        
        //ChunkManager.RenderThread();
    }
    public bool CanThreadStart(Threads thread)
    {
        var t = GetThread(thread); 
        return t.ThreadState == ThreadState.Stopped;
    }

    public static Thread GetThread(Threads thread)
    {
        return m_Threads[(int)thread];
    }
}



