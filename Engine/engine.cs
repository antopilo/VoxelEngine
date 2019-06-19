using Godot;
using System;
using System.Threading;
using VoxelEngine.engine;
using VoxelEngine.engine.World;

// TODOs:
//  - Threads, 1 preload, 1 Render
//  - remove side chunk of the mesh
//  - NoiseMachine.
//  - Update chunks
//  - Optimize hidden subchunk.

public class engine : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node scene = GetTree().CurrentScene;

        Chunk chunk;
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                chunk = new Chunk();
                chunk.SetPosition(new Vector2(x, z));
                chunk.ChunkSetup();
                chunk.Render();
                scene.CallDeferred("add_child", chunk);
            }
        }
    }
}


public enum Threads
{
    PRELOAD, RENDER
}
