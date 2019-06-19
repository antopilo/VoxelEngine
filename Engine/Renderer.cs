using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine.World;

namespace VoxelEngine.engine
{
    public enum CUBE_FACES
    {
        Top, Bottom,
        Left, Right,
        Front, Back
    }

    public class Renderer
    {
        public static float CubeSize { get; } = 1f;
        public static string DefaultMaterialPath = "res://Content/Materials/Default.tres";
        public static SpatialMaterial DefaultMaterial;
        public static RandomNumberGenerator rng = new RandomNumberGenerator();

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
        public static Vector3[] CUBE_NORMALS =
        {
            new Vector3(0, 1, 0), new Vector3(0, -1, 0),
            new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1), new Vector3(0, 0, -1)
        };


        public static Vector3[] TestTop = { new Vector3(0, 15, 0), new Vector3(15, 15, 15) };
        public static Vector3[] TestBottom = { new Vector3(0, 15, 0), new Vector3(15, 15, 15) };


        private static Color CurrentColor = new Color();
        private static List<Vector3> Vertices = new List<Vector3>();
        private static List<Vector3> Normals = new List<Vector3>();
        private static List<Color> Colors = new List<Color>();

        // Creates a cube using an array.
        public static ArrayMesh Render(Block[,,] data)
        {
            var arrayMesh = new ArrayMesh();

            // Array container other arrays.
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)ArrayMesh.ArrayType.Max);

            // Reset data.
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Colors = new List<Color>();

            // 16 x 16 x 16 = 4096 blocks.
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    for (int y  = 0; y < Chunk.CHUNK_SIZE; y++)
                    {
                        // No cube if not active.
                        if (data[x, y, z].Active == false)
                            continue;

                        // Create cube.
                        CreateBlock(new Vector3(x, y, z), data);
                    }
                }
            }

            // Fill the array with the others arrays.
            arrays[(int)ArrayMesh.ArrayType.Vertex] = Vertices.ToArray();
            arrays[(int)ArrayMesh.ArrayType.Normal] = Normals.ToArray();
            arrays[(int)ArrayMesh.ArrayType.Color] = Colors.ToArray();
            
            // Create surface from arrays.
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

            ApplyMaterial(arrayMesh);

            // Done.
            return arrayMesh;
        }

        private static void CreateBlock(Vector3 position, Block[,,] data)
        {
            // Position as integer.
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;

            // False if there is a block next to those faces. 
            bool left, right, top, bottom, front, back;
            left   = x != 0  ? !data[x - 1, y, z].Active : true;
            right  = x != 15 ? !data[x + 1, y, z].Active : true;
            top    = y != 15 ? !data[x, y + 1, z].Active : true;
            bottom = y != 0  ? !data[x, y - 1, z].Active : true;
            front  = z != 15 ? !data[x, y, z + 1].Active : true;
            back   = z != 0  ? !data[x, y, z - 1].Active : true;

            // If the block is completly surrounded(not seen).
            if (left && right && top && bottom && front && back)
                return;

            // TODO have a way to access the surrounding chunk.
            // to remove the faces on the borders of every chunk.

            // Look up table for iteration.
            bool[] lutFaces = { top, bottom, left, right, front, back };

            // Iterating through each face and check if we should place
            // or not a face of a cube.
            foreach (CUBE_FACES face in Enum.GetValues(typeof(CUBE_FACES)))
                if (lutFaces[(int)face] == true)
                    CreateFace(face, position);

        }


        // Create a face at position.
        private static void CreateFace(CUBE_FACES face, Vector3 position)
        {
            switch (face)
            {
                case CUBE_FACES.Top:
                    AddVertex(position, 4, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 5, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 7, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 5, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 6, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 7, CUBE_NORMALS[(int)face]);
                    break;
                case CUBE_FACES.Bottom:
                    AddVertex(position, 1, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 3, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 2, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 1, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 0, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 3, CUBE_NORMALS[(int)face]);
                    break;
                case CUBE_FACES.Left:
                    AddVertex(position, 0, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 7, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 3, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 0, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 4, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 7, CUBE_NORMALS[(int)face]);
                    break;
                case CUBE_FACES.Right:
                    AddVertex(position, 2, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 5, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 1, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 2, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 6, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 5, CUBE_NORMALS[(int)face]);
                    break;
                case CUBE_FACES.Front:
                    AddVertex(position, 3, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 6, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 2, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 3, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 7, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 6, CUBE_NORMALS[(int)face]);
                    break;
                case CUBE_FACES.Back:
                    AddVertex(position, 0, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 1, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 5, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 5, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 4, CUBE_NORMALS[(int)face]);
                    AddVertex(position, 0, CUBE_NORMALS[(int)face]);
                    break;
            }
        }
        

        private static void AddVertex(Vector3 position, int idx, Vector3 normal)
        {
            Vertices.Add(position + CUBE_VERTICES[idx]);
            Normals.Add(normal);
            Colors.Add(CurrentColor.Blend(new Color(rng.Randf(), rng.Randf(), rng.Randf())));
        }

        // Apply the default material on each surface
        private static void ApplyMaterial(ArrayMesh mesh)
        {
            if (DefaultMaterial is null)
                DefaultMaterial = (SpatialMaterial)ResourceLoader.Load(DefaultMaterialPath);

            // Apply the material on each surface
            for (int i = 0; i < mesh.GetSurfaceCount(); i++)
            {
                mesh.SurfaceSetMaterial(i, DefaultMaterial);
            }
        }
    }
}
