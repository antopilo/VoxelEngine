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
    public static bool MultiThreaded = true;
    private Thread[] Threads = new Thread[3];

    private static float TickDuration = 1 / 20f;
    private static float NextTick = 0f;

    public static int chunkRenderedPerTick = 16;

    // UI components for debugging.
    private bool m_Debug = true;
    private Control UI;
    private Label FPS, LoadedCount, x, y, z;
    private Camera Camera;
    private ImmediateGeometry debugCursor;

    private Vector3? LookingAtPosition = null;

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

        CreateBlockOutline();
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

        debugCursor = Scene.GetNode("BlockOutline") as ImmediateGeometry;
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


    public override void _PhysicsProcess(float delta)
    {
        CheckCameraIntersect();

        if (Input.IsActionJustPressed("mouse1"))
        {
            if(LookingAtPosition != null)
            {
                ChunkManager.SetBlock((Vector3)LookingAtPosition, BLOCK_TYPE.Empty);
            }
            
        }
        //if(LookingAtPosition != null)
        //    GD.Print(LookingAtPosition);
    }

    private void Tick()
    {
        GD.Print("Tick");
        ChunkManager.Update();

        // Calculate when the next tick should happen.
        NextTick = DeltaTime + TickDuration;
    }


    private void CreateBlockOutline()
    {
        debugCursor.Begin(Mesh.PrimitiveType.LineStrip);
        debugCursor.SetColor(new Color(1, 0, 0));

        debugCursor.AddVertex(Renderer.CUBE_VERTICES[0] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[1] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[2] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[3] * 0.01f);

        debugCursor.AddVertex(Renderer.CUBE_VERTICES[3] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[0] * 1.01f);
                                                          
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[4] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[5] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[1] * 1.01f);
                                                          
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[5] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[6] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[2] * 1.01f);
                                                          
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[6] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[7] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[3] * 1.01f);
                                                          
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[7] * 1.01f);
        debugCursor.AddVertex(Renderer.CUBE_VERTICES[4] * 1.01f);
        debugCursor.End();
    }


    /// <summary>
    /// Check if the camera is currently aiming at a voxel in the world.
    /// </summary>
    private void CheckCameraIntersect()
    {
        Vector3 hitPosition;
        var cam = this.Camera;
        var rayLength = 100;

        Vector2 viewportSize = cam.GetViewport().Size / 2;

        // Making sure we start the raycast in the middle of the screen.
        var rayOrigin = cam.ProjectRayOrigin(viewportSize);
        var rayDirection = rayOrigin + cam.ProjectRayNormal(viewportSize) * rayLength;

        // direct state is used to make collisions queuries
        var directState = PhysicsServer.SpaceGetDirectState(cam.GetWorld().Space);

        // TODO: Add hit exception in here.
        var rayResult = directState.IntersectRay(rayOrigin, rayDirection, new Godot.Collections.Array() { cam, debugCursor});
        var offset = new Vector3(0.5f, 0.5f, 0.5f);

        // If the ray hit something.
        if (rayResult.Count != 0)
        {
            hitPosition = (Vector3)rayResult["position"] - offset;
            Vector3 hitNormal = (Vector3)rayResult["normal"];

            // removing floating points.
            hitPosition = new Vector3(Mathf.Stepify(hitPosition.x, 1),
                                    Mathf.Stepify(hitPosition.y, 1),
                                    Mathf.Stepify(hitPosition.z, 1));

            debugCursor.Translation = hitPosition - hitNormal;

            LookingAtPosition = hitPosition - hitNormal;
        }
        else
        {
            LookingAtPosition = null;
        }

        
    }
}



