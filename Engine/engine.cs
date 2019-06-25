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
    private static int CamX = 0;
    private static int CamZ = 0;

    public static Vector2 CameraPosition
    {
        get
        {
            return new Vector2(CamX, CamZ);
        }
    }


    private static Thread[] m_Threads = new Thread[2];

    private bool m_Debug = true;
    private Control UI;
    private Label FPS, Mem, LoadedCount, x, y, z;

    private Camera Camera;
    private Vector2 CurrentChunkPosition = new Vector2();
    private int m_loadDistance = 8;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Scene = GetTree().CurrentScene;
        Camera = (Camera)Scene.GetNode("CameraInGame");

        if (m_Debug)
        {
            UI = (Control)Scene.GetNode("DEBUG_UI/HBoxContainer");
            FPS = (Label)UI.GetNode("fps");
            Mem = (Label)UI.GetNode("mem");
            LoadedCount = (Label)UI.GetNode("loaded");
            x = (Label)UI.GetNode("x");
            y = (Label)UI.GetNode("y");
            z = (Label)UI.GetNode("z");
        }
            

        var threadStart1 = new ThreadStart(ChunkManager.LoadThread);
        var threadStart2 = new ThreadStart(ChunkManager.RenderThread);

        // Initialize threads
        m_Threads[(int)Threads.PRELOAD] = new Thread(threadStart1);
        m_Threads[(int)Threads.RENDER] = new Thread(threadStart2);

        //for (int x = 0; x < 32; x++)
        //{
        //    for (int z = 0; z < 32; z++)
        //    {
        //        ChunkManager.AddLoadQueue(new Vector2(x, z));
        //    }
        //}

        //ChunkManager.LoadThread();
        

        m_Threads[(int)Threads.PRELOAD].Start();
        m_Threads[(int)Threads.RENDER].Start();
    }

    public override void _Process(float delta)
    {
        CamX = (int)Camera.GlobalTransform.origin.x / 16;
        CamZ = (int)Camera.GlobalTransform.origin.z / 16;
        CurrentChunkPosition = new Vector2(CamX, CamZ);

        if (m_Debug)
        {
            FPS.Text = Godot.Engine.GetFramesPerSecond().ToString();
            x.Text = CamX.ToString();
            z.Text = CamZ.ToString();
            y.Text = ((int)Camera.GlobalTransform.origin.y).ToString();
            LoadedCount.Text = ChunkManager.GetLoadedCount().ToString();
        }

        ChunkManager.CameraPosition = new Vector2(CamX, CamZ);

        for (int x = CamX - m_loadDistance; x < CamX + m_loadDistance; x++)
        {
            for (int z = CamZ - m_loadDistance; z < CamZ + m_loadDistance; z++)
            {
                ChunkManager.AddLoadQueue(new Vector2(x, z));
            }
        }

        
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



