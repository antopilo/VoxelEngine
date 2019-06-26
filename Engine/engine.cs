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

    private bool m_Debug = true;
    private Control UI;
    private Label FPS, Mem, LoadedCount, x, y, z;

    private Camera Camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        LoadReference();

        var startTime = DateTime.Now;
        ChunkManager.Load();

        ChunkManager.UpdatePreloaded();

        ChunkManager.Render();
        GD.Print("TotalTime: ", (DateTime.Now - startTime).TotalMilliseconds.ToString());
    }

    public void LoadReference()
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

        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                ChunkManager.AddLoadQueue(new Vector2(x, z));
            }
        }
    }

    public override void _Process(float delta)
    {
        CamX = (int)Camera.GlobalTransform.origin.x / 16;
        CamZ = (int)Camera.GlobalTransform.origin.z / 16;

        // Debug UI
        if (m_Debug)
        {
            FPS.Text = Godot.Engine.GetFramesPerSecond().ToString();
            x.Text = CamX.ToString();
            z.Text = CamZ.ToString();
            y.Text = ((int)Camera.GlobalTransform.origin.y).ToString();
            LoadedCount.Text = ChunkManager.GetLoadedCount().ToString();
        }

        ChunkManager.CameraPosition = new Vector2(CamX, CamZ);
    }
}



