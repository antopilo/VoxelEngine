using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class VoxelSprite
{
    public ArrayMesh Mesh;
    public Vector3 Position;

    public VoxelSprite(ArrayMesh mesh, Vector3 localPosition)
    {
        Mesh = mesh;
        Position = localPosition;
    }
}

