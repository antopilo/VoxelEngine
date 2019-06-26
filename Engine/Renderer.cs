using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine.World;
using VoxelEngine.Engine.World;

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

        private static Color CurrentColor = new Color();

        private static Vector3[] Vertices;
        private static Vector3[] Normals;
        private static Color[] Colors;

        private static int vertexIdx = 0;

        // Creates a cube using an array.
        public static ArrayMesh Render(SubChunk chunk)
        {
            var arrayMesh = new ArrayMesh();

            // Array containing other arrays.
            // OpenGL array buffer.
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)ArrayMesh.ArrayType.Max);

            // Reset data.
            Vertices = new Vector3[9999];
            Normals = new Vector3[9999];
            Colors = new Color[9999];

            vertexIdx = 0;

            if (chunk.isEmpty())
                return null;


            // If sub-chunk is completely surrounded. dont render.
            if (!chunk.RenderBottom && !chunk.RenderTop && 
                !chunk.RenderLeft && !chunk.RenderRight &&
                !chunk.RenderFront && !chunk.RenderBack)
                return null;

            // 16 x 16 x 16 = 4096 blocks.
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    {
                        // No cube if not active.
                        if (chunk.GetBlock(x, y, z).Active == false)
                            continue;

                        if (x == 0 && !chunk.RenderLeft)
                        {
                            x++;
                            continue;
                        }
                            
                        if (y == 0 && !chunk.RenderBottom)
                            continue;
                        if (y == 15 && !chunk.RenderTop)
                            continue;

                        if (z == 0 && !chunk.RenderBack)
                        {
                            z++;
                            continue;
                        }
                        if (z == 15 && !chunk.RenderFront)
                            continue;

                        // Create cube.
                        CreateBlock(new Vector3(x, y, z), chunk);
                    }
                }   
            }

            System.Array.Resize(ref Vertices, vertexIdx);
            System.Array.Resize(ref Normals, vertexIdx);
            System.Array.Resize(ref Colors, vertexIdx);

            // Fill the array with the others arrays.
            arrays[0] = Vertices;
            arrays[1] = Normals;
            arrays[3] = Colors;

            // Create surface from arrays.
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

            ApplyMaterial(arrayMesh);

            // Done.
            return arrayMesh;
        }

        private static void CreateBlock(Vector3 position, SubChunk chunk)
        {
            // subChunk index in the Chunk itself. [0, 15]
            int subChunkId = chunk.SubChunkId;
            
            // Position as integer.
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;

            // False if there is a block next to those faces. 
            bool left, right, top, bottom, front, back;
            top    = y != 15 ? !chunk.GetBlock(x, y + 1, z).Active : true;
            bottom = y != 0  ? !chunk.GetBlock(x, y - 1, z).Active : true;
            left   = x != 0  ? !chunk.GetBlock(x - 1, y, z).Active : true;
            right  = x != 15 ? !chunk.GetBlock(x + 1, y, z).Active : true;
            front  = z != 15 ? !chunk.GetBlock(x, y, z + 1).Active : true;
            back   = z != 0  ? !chunk.GetBlock(x, y, z - 1).Active : true;

            // If the block is completly surrounded(not seen).
            if (left && right && top && bottom && front && back)
                return;

            // Should draw faces? except chunk borders.
            bool leftChunk = true;
            bool rightChunk = true;
            bool backChunk = true;
            bool frontChunk = true;
            
            if(x == 0)
                leftChunk  = !chunk.Chunk.ChunkLeft.GetSubChunk(subChunkId).GetBlock(15, y, z).Active;
            if(x == 15)
                rightChunk = !chunk.Chunk.ChunkRight.GetSubChunk(subChunkId).GetBlock(0, y, z).Active;
            if(z == 0)
                backChunk  = !chunk.Chunk.ChunkBack.GetSubChunk(subChunkId).GetBlock(x, y, 15).Active;
            if(z == 15)
                frontChunk = !chunk.Chunk.ChunkFront.GetSubChunk(subChunkId).GetBlock(x, y, 0).Active;

            // Check if there is a block in the above subchunk.
            // Dont check if the subchunk is the bottom or top one.
            bool topChunk = true;
            bool bottomChunk = true;
            if (subChunkId != 15)
            {
                SubChunk topSubChunk = chunk.Chunk.GetSubChunk(subChunkId + 1);
                if (topSubChunk.isFull())
                    topChunk = true;
                else
                    topChunk = !topSubChunk.GetBlock(x, 0, z).Active;
            }
            if (subChunkId != 0)
            {
                SubChunk botSubChunk = chunk.Chunk.GetSubChunk(subChunkId - 1);
                if (botSubChunk.isFull())
                    bottomChunk = true;
                else
                    bottomChunk = !botSubChunk.GetBlock(x, 15, z).Active;
            }

            // False if should not render chunk border faces.
            bool topBorder    = y == 15 ? chunk.RenderTop    && topChunk    : top;
            bool bottomBorder = y == 0  ? chunk.RenderBottom && bottomChunk : bottom;

            bool leftBorder   = x == 0  ? chunk.RenderLeft   && leftChunk   : left;
            bool rightBorder  = x == 15 ? chunk.RenderRight  && rightChunk  : right;
            bool frontBorder  = z == 15 ? chunk.RenderFront  && frontChunk  : front;
            bool backBorder   = z == 0  ? chunk.RenderBack   && backChunk   : back;

            // Display the chunk in green if chunk is surrounded.
            CurrentColor = BlockPalette.GetColor(chunk.GetBlock(x,y,z).Type);
            
            // Represent each faces of a cube and if it should place
            // each faces. Placed in the same order in CUBE_FACES enum.
            bool[] lutFaces = 
            {
                topBorder, bottomBorder,
                leftBorder, rightBorder,
                frontBorder, backBorder
            };

            // Iterating through each face and check if we should place
            // or not a face of a cube in the LUT just declared before.
            foreach (CUBE_FACES face in Enum.GetValues(typeof(CUBE_FACES)))
            {
                // If the LUT returns true, create the face.
                if (lutFaces[(int)face] == true)
                    CreateFace((int)face, position);
            }
        }


        // Create a face at position.
        private static void CreateFace(int face, Vector3 position)
        {
            switch (face)
            {
                case (int)CUBE_FACES.Top:
                    AddVertex(position, 4, CUBE_NORMALS[face]);
                    AddVertex(position, 5, CUBE_NORMALS[face]);
                    AddVertex(position, 7, CUBE_NORMALS[face]);
                    AddVertex(position, 5, CUBE_NORMALS[face]);
                    AddVertex(position, 6, CUBE_NORMALS[face]);
                    AddVertex(position, 7, CUBE_NORMALS[face]);
                    break;
                case (int)CUBE_FACES.Bottom:
                    AddVertex(position, 1, CUBE_NORMALS[face]);
                    AddVertex(position, 3, CUBE_NORMALS[face]);
                    AddVertex(position, 2, CUBE_NORMALS[face]);
                    AddVertex(position, 1, CUBE_NORMALS[face]);
                    AddVertex(position, 0, CUBE_NORMALS[face]);
                    AddVertex(position, 3, CUBE_NORMALS[face]);
                    break;
                case (int)CUBE_FACES.Left:
                    AddVertex(position, 0, CUBE_NORMALS[face]);
                    AddVertex(position, 7, CUBE_NORMALS[face]);
                    AddVertex(position, 3, CUBE_NORMALS[face]);
                    AddVertex(position, 0, CUBE_NORMALS[face]);
                    AddVertex(position, 4, CUBE_NORMALS[face]);
                    AddVertex(position, 7, CUBE_NORMALS[face]);
                    break;
                case (int)CUBE_FACES.Right:
                    AddVertex(position, 2, CUBE_NORMALS[face]);
                    AddVertex(position, 5, CUBE_NORMALS[face]);
                    AddVertex(position, 1, CUBE_NORMALS[face]);
                    AddVertex(position, 2, CUBE_NORMALS[face]);
                    AddVertex(position, 6, CUBE_NORMALS[face]);
                    AddVertex(position, 5, CUBE_NORMALS[face]);
                    break;
                case (int)CUBE_FACES.Front:
                    AddVertex(position, 3, CUBE_NORMALS[face]);
                    AddVertex(position, 6, CUBE_NORMALS[face]);
                    AddVertex(position, 2, CUBE_NORMALS[face]);
                    AddVertex(position, 3, CUBE_NORMALS[face]);
                    AddVertex(position, 7, CUBE_NORMALS[face]);
                    AddVertex(position, 6, CUBE_NORMALS[face]);
                    break;
                case (int)CUBE_FACES.Back:
                    AddVertex(position, 0, CUBE_NORMALS[face]);
                    AddVertex(position, 1, CUBE_NORMALS[face]);
                    AddVertex(position, 5, CUBE_NORMALS[face]);
                    AddVertex(position, 5, CUBE_NORMALS[face]);
                    AddVertex(position, 4, CUBE_NORMALS[face]);
                    AddVertex(position, 0, CUBE_NORMALS[face]);
                    break;
            }
        }
        

        private static void AddVertex(Vector3 position, int idx, Vector3 normal)
        {
            Vertices[vertexIdx] = position + CUBE_VERTICES[idx];
            Normals[vertexIdx] = normal;
            Colors[vertexIdx] = CurrentColor;

            vertexIdx++;
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
