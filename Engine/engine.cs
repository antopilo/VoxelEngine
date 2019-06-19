using Godot;
using System;
using System.Threading;
using VoxelEngine.engine;


public class engine : Node
{
    // Array of threads
    private Renderer m_Renderer = new Renderer();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node scene = GetTree().CurrentScene;

        // Testing creating a cube.
        var cube = new MeshInstance();
        cube.Mesh = m_Renderer.CreateCube();
        scene.AddChild(cube);
    }
}


public enum Threads
{
    PRELOAD, RENDER
}
