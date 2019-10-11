using Godot;
using System;
using System.Threading;
using VoxelEngine.Engine.World;
using Thread = System.Threading.Thread;

// TODOs:
//  - Update chunks

public enum Threads
{
    PRELOAD, RENDER
}

public class Engine : Node
{
    public static Node Scene { get; set; }


    public static int CamX = 0;
    public static int CamZ = 0;

    public static int RenderDistance = 16;
    public static int SpawnRadius = 8;

    // Multi-threading
    private bool MultiThreaded = false;
    private Thread[] Threads = new Thread[3];

    private static float TickDuration = 1 / 20f;
    private static float NextTick = 0f;

    public static int chunkRenderedPerTick = 4;

    // UI components for debugging.
    private bool m_Debug = true;
    private Control UI;
    private Label FPS, LoadedCount, x, y, z;
    private Camera Camera;

    public static Vector2 CameraPosition
    {
        get
        {
            return new Vector2(CamX, CamZ);
        }
    }

    private float DeltaTime = 0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Godot.Engine.TargetFps = 0;

        LoadReference();
        ModelLoader.LoadModels();
        NoiseMaker.Initialize();

        if (MultiThreaded)
        {
            var threadStart1 = new ThreadStart(ChunkManager.UpdateThread);
            Threads[0] = new Thread(threadStart1);
            Threads[0].Start();
        }
    }

    public void LoadReference()
    {
        Scene = GetTree().CurrentScene;
        Camera = (Camera)Scene.GetNode("CameraInGame");

        ChunkManager.Camera = Camera;

        if (m_Debug)
        {
            UI = (Control)Scene.GetNode("DEBUG_UI/HBoxContainer");
            FPS = (Label)UI.GetNode("fps");
            LoadedCount = (Label)UI.GetNode("loaded");
            x = (Label)UI.GetNode("x");
            y = (Label)UI.GetNode("y");
            z = (Label)UI.GetNode("z");
        }
    }

    public override void _Process(float delta)
    {
        // Update delta time.
        DeltaTime += delta;
        
        // Get Chunk position of the camera.
        CamX = (int)Camera.GlobalTransform.origin.x / 16;
        CamZ = (int)Camera.GlobalTransform.origin.z / 16;

        // Send updated camera position to chunk manager.
        ChunkManager.CameraPosition = new Vector2(CamX, CamZ);

        // Update Debugging UI;
        if (m_Debug)
        {
            FPS.Text = "FPS " + Godot.Engine.GetFramesPerSecond().ToString();
            x.Text = "X " + ((int)Camera.GlobalTransform.origin.x).ToString();
            z.Text = "X " + ((int)Camera.GlobalTransform.origin.z).ToString();
            y.Text = "Y " +  ((int)Camera.GlobalTransform.origin.y).ToString();
            LoadedCount.Text = "Chunks: " + ChunkManager.GetLoadedCount().ToString();
        }

        if (!MultiThreaded)
        {
            // Tick
            if (DeltaTime >= NextTick)
                Tick();
        }
    }

    private void Tick()
    {
        GD.Print("Tick");
        ChunkManager.Update();

        // Calculate when the next tick should happen.
        NextTick = DeltaTime + TickDuration;
    }
}



