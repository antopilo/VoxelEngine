using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelEngine.engine
{
    public class Renderer : Godot.Object
    {
        public float CubeSize { get; } = 1f;

        public static Vector3[] CUBE_VERTICES =
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1)
        };

        // Creates a cube using an array.
        public ArrayMesh CreateCube()
        {
            var arrayMesh = new ArrayMesh();

            // Array container other arrays.
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)ArrayMesh.ArrayType.Max);

            var vertices = new Vector3[] 
            {
                new Vector3(-1, -1, -1),
                new Vector3(1, -1, -1),
                new Vector3(1, 1, -1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, -1, 1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, 1),
                new Vector3(-1, 1, 1)
            };

            var normals  = new List<Vector3>();
            var colors   = new List<Color>();
            

            // Fill the array with the others arrays.
            arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
            arrays[(int)ArrayMesh.ArrayType.Index] = indices;
            //arrays[(int)ArrayMesh.ArrayType.Normal] = normals.ToArray();
            //arrays[(int)ArrayMesh.ArrayType.Color] = colors.ToArray();

            // Create surface from arrays.
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

            // Done.
            return arrayMesh;
        }

        // Add a vertex at last position in an array.
        private void AddVertex(Vector3 vertex, ref List<Vector3> vertices)
        {
            vertices.Add(vertex);
        }

        // Add a normal     at last position in an array.
        private void AddNormal(Vector3 normal, ref List<Vector3> normals)
        {
            normals.Add(normal);
        }
    }
}
