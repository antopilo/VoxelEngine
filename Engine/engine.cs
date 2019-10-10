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

// remy

//@ssss
public class Engine : Node
{
    public static int RenderDistance = 16;
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

    private bool m_Debug = true;
    private Control UI;
    private Label FPS, Mem, LoadedCount, x, y, z;

    private Camera Camera;
    private Thread[] Threads = new Thread[3];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        LoadReference();
        ModelLoader.LoadModels();

        NoiseMaker.Initialize();

        var startTime = DateTime.Now;

        var threadStart1 = new ThreadStart(ChunkManager.Load);
        var threadStart2 = new ThreadStart(ChunkManager.Render);
        var threadStart3 = new ThreadStart(ChunkManager.UpdatePreloaded);

        Threads[0] = new Thread(threadStart1);
        Threads[1] = new Thread(threadStart2);
        Threads[2] = new Thread(threadStart3);

        Threads[0].Start();
        Threads[1].Start();
        // Threads[2].Start();

        // 1- ChunkManager.Load();
        // 2- ChunkManager.UpdatePreloaded();
        // 3- ChunkManager.Render();

        //GD.Print("TotalTime: ", (DateTime.Now - startTime).TotalMilliseconds.ToString());
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
            //Mem = (Label)UI.GetNode("mem");
            LoadedCount = (Label)UI.GetNode("loaded");
            x = (Label)UI.GetNode("x");
            y = (Label)UI.GetNode("y");
            z = (Label)UI.GetNode("z");
        }
    }

    public override void _Process(float delta)
    {
        CamX = (int)Camera.GlobalTransform.origin.x / 16;
        CamZ = (int)Camera.GlobalTransform.origin.z / 16;
        ChunkManager.CameraPosition = new Vector2(CamX, CamZ);

        // Debug UI
        if (m_Debug)
        {
            FPS.Text = "FPS " + Godot.Engine.GetFramesPerSecond().ToString();
            x.Text = "X " + ((int)Camera.GlobalTransform.origin.x).ToString();
            z.Text = "X " + ((int)Camera.GlobalTransform.origin.z).ToString();
            y.Text = "Y " +  ((int)Camera.GlobalTransform.origin.y).ToString();
            LoadedCount.Text = "Chunks: " + ChunkManager.GetLoadedCount().ToString();
        }

       
        ChunkManager.UpdatePreloaded();
    }
}



