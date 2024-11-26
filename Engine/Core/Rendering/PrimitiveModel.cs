using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core.Rendering
{
    public class PrimitiveModel
    {
        public static StaticMesh CreateBox(float width, float height, float depth)
        {
            // Vertices for the box (cuboid) - 8 points
            var vertices = new VertexPositionNormalTexture[24];

            // Define vertices for each face of the box
            // Front face
            vertices[0] = new VertexPositionNormalTexture(new Vector3(-width, -height, -depth), Vector3.Backward, new Vector2(0, 1));  // Bottom-left
            vertices[1] = new VertexPositionNormalTexture(new Vector3(width, -height, -depth), Vector3.Backward, new Vector2(1, 1));   // Bottom-right
            vertices[2] = new VertexPositionNormalTexture(new Vector3(width, height, -depth), Vector3.Backward, new Vector2(1, 0));    // Top-right
            vertices[3] = new VertexPositionNormalTexture(new Vector3(-width, height, -depth), Vector3.Backward, new Vector2(0, 0));   // Top-left

            // Back face
            vertices[4] = new VertexPositionNormalTexture(new Vector3(-width, -height, depth), Vector3.Forward, new Vector2(0, 1));   // Bottom-left
            vertices[5] = new VertexPositionNormalTexture(new Vector3(width, -height, depth), Vector3.Forward, new Vector2(1, 1));    // Bottom-right
            vertices[6] = new VertexPositionNormalTexture(new Vector3(width, height, depth), Vector3.Forward, new Vector2(1, 0));     // Top-right
            vertices[7] = new VertexPositionNormalTexture(new Vector3(-width, height, depth), Vector3.Forward, new Vector2(0, 0));    // Top-left

            // Left face
            vertices[8] = new VertexPositionNormalTexture(new Vector3(-width, -height, -depth), Vector3.Left, new Vector2(0, 1));    // Bottom-left
            vertices[9] = new VertexPositionNormalTexture(new Vector3(-width, -height, depth), Vector3.Left, new Vector2(1, 1));     // Bottom-right
            vertices[10] = new VertexPositionNormalTexture(new Vector3(-width, height, depth), Vector3.Left, new Vector2(1, 0));     // Top-right
            vertices[11] = new VertexPositionNormalTexture(new Vector3(-width, height, -depth), Vector3.Left, new Vector2(0, 0));    // Top-left

            // Right face
            vertices[12] = new VertexPositionNormalTexture(new Vector3(width, -height, -depth), Vector3.Right, new Vector2(0, 1));   // Bottom-left
            vertices[13] = new VertexPositionNormalTexture(new Vector3(width, -height, depth), Vector3.Right, new Vector2(1, 1));    // Bottom-right
            vertices[14] = new VertexPositionNormalTexture(new Vector3(width, height, depth), Vector3.Right, new Vector2(1, 0));     // Top-right
            vertices[15] = new VertexPositionNormalTexture(new Vector3(width, height, -depth), Vector3.Right, new Vector2(0, 0));    // Top-left

            // Bottom face
            vertices[16] = new VertexPositionNormalTexture(new Vector3(-width, -height, -depth), Vector3.Down, new Vector2(0, 1));   // Bottom-left
            vertices[17] = new VertexPositionNormalTexture(new Vector3(width, -height, -depth), Vector3.Down, new Vector2(1, 1));    // Bottom-right
            vertices[18] = new VertexPositionNormalTexture(new Vector3(width, -height, depth), Vector3.Down, new Vector2(1, 0));     // Top-right
            vertices[19] = new VertexPositionNormalTexture(new Vector3(-width, -height, depth), Vector3.Down, new Vector2(0, 0));    // Top-left

            // Top face
            vertices[20] = new VertexPositionNormalTexture(new Vector3(-width, height, -depth), Vector3.Up, new Vector2(0, 1));      // Bottom-left
            vertices[21] = new VertexPositionNormalTexture(new Vector3(width, height, -depth), Vector3.Up, new Vector2(1, 1));       // Bottom-right
            vertices[22] = new VertexPositionNormalTexture(new Vector3(width, height, depth), Vector3.Up, new Vector2(1, 0));        // Top-right
            vertices[23] = new VertexPositionNormalTexture(new Vector3(-width, height, depth), Vector3.Up, new Vector2(0, 0));       // Top-left

            // Indices
            short[] indices = new short[]
            {
        // Front face
        0, 2, 3,
        0, 1, 2,

        // Back face
        4, 7, 6,
        4, 6, 5,

        // Left face
        8, 11, 10,
        8, 10, 9,

        // Right face
        12, 14, 15,
        12, 13, 14,

        // Bottom face
        16, 19, 18,
        16, 18, 17,

        // Top face
        20, 22, 23,
        20, 21, 22
            };

            return CreatePrimitiveModel(vertices, indices);
        }

        public static StaticMesh CreateSphere(float radius, int latitudeDivisions = 64, int longitudeDivisions = 64)
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            // Generate vertices
            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float theta = lat * MathF.PI / latitudeDivisions; // Latitude angle
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions; // Longitude angle

                    // Spherical to Cartesian conversion
                    float x = -radius * MathF.Sin(theta) * MathF.Cos(phi);
                    float y = radius * MathF.Cos(theta);
                    float z = radius * MathF.Sin(theta) * MathF.Sin(phi);

                    // Normal vector (unit vector in the same direction)
                    Vector3 normal = new Vector3(x, y, z);
                    normal.Normalize();

                    // Texture coordinates (uv mapping)
                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    // Add vertex
                    vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, z), normal, new Vector2(u, v)));
                }
            }



            // Generate indices for the triangles
            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    // Two triangles per sector
                    if (lat != 0) // Skip the first row for the top triangle
                    {
                        indices.Add((short)current);
                        indices.Add((short)(current + 1));
                        indices.Add((short)next);
                    }

                    if (lat != latitudeDivisions - 1) // Skip the last row for the bottom triangle
                    {
                        indices.Add((short)(current + 1));
                        indices.Add((short)(next + 1));
                        indices.Add((short)next);
                    }
                }
            }

            return CreatePrimitiveModel(vertices.ToArray(), indices.ToArray());
        }

        public static StaticMesh CreateCapsule(float radius, float height, int latitudeDivisions = 64, int longitudeDivisions = 64)
        {
            height = height / 2;
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            // Create the bottom hemisphere (using negative height for offset)
            CreateHemisphere(vertices, indices, radius, -height / 2, latitudeDivisions, longitudeDivisions);

            // Create the cylinder body
            CreateCylinderVertex(vertices, indices, radius, height, latitudeDivisions, longitudeDivisions);

            // Create the top hemisphere (using positive height for offset)
            CreateHemisphere(vertices, indices, radius, height / 2, latitudeDivisions, longitudeDivisions);

            return CreatePrimitiveModel(vertices.ToArray(), indices.ToArray());
        }

        private static void CreateHemisphere(List<VertexPositionNormalTexture> vertices, List<short> indices, float radius, float offsetY, int latitudeDivisions, int longitudeDivisions)
        {
            int vertexCount = vertices.Count;

            // Generate vertices for the hemisphere
            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float theta = lat * MathF.PI / latitudeDivisions; // Latitude angle (0 to Pi)
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions; // Longitude angle (0 to 2Pi)

                    // Spherical to Cartesian conversion for the hemisphere
                    float x = -radius * MathF.Sin(theta) * MathF.Cos(phi);
                    float y = radius * MathF.Cos(theta) + offsetY;  // Offset Y for hemisphere positioning
                    float z = radius * MathF.Sin(theta) * MathF.Sin(phi);

                    // Normal vector (pointing outward)
                    Vector3 normal = new Vector3(x, y, z);
                    normal.Normalize();

                    // Texture coordinates (uv mapping)
                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    // Add vertex
                    vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, z), normal, new Vector2(u, v)));
                }
            }

            // Create indices for the hemisphere (top-down triangle strips)
            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = vertexCount + lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    // Two triangles per sector
                    if (lat != 0) // Skip the first row for the top triangle
                    {
                        indices.Add((short)current);
                        indices.Add((short)(current + 1));
                        indices.Add((short)next);
                    }

                    if (lat != latitudeDivisions - 1) // Skip the last row for the bottom triangle
                    {
                        indices.Add((short)(current + 1));
                        indices.Add((short)(next + 1));
                        indices.Add((short)next);
                    }
                }
            }
        }

        private static void CreateCylinderVertex(List<VertexPositionNormalTexture> vertices, List<short> indices, float radius, float height, int latitudeDivisions, int longitudeDivisions)
        {
            int vertexCount = vertices.Count;

            // Generate vertices for the cylinder
            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float theta = lat * MathF.PI / latitudeDivisions; // Latitude angle (0 to Pi)
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions; // Longitude angle (0 to 2Pi)

                    // Spherical to Cartesian conversion for the cylinder
                    float x = radius * MathF.Cos(phi);
                    float z = radius * MathF.Sin(phi);
                    float y = height * (lat / (float)latitudeDivisions - 0.5f); // Linear interpolation for height (cylinder)

                    // Normal vector (pointing outward along the XZ plane)
                    Vector3 normal = new Vector3(x, 0, z);
                    normal.Normalize();

                    // Texture coordinates (uv mapping)
                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    // Add vertex
                    vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, z), normal, new Vector2(u, v)));
                }
            }

            // Create indices for the cylinder (side triangles)
            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = vertexCount + lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    // Two triangles per sector
                    if (lat != 0 && lat != latitudeDivisions - 1) // Skip top and bottom for cylinder sides
                    {
                        indices.Add((short)current);
                        indices.Add((short)(current + 1));
                        indices.Add((short)next);

                        indices.Add((short)(current + 1));
                        indices.Add((short)(next + 1));
                        indices.Add((short)next);
                    }
                }
            }
        }
        public static StaticMesh CreateCylinder(float radius, float height, int latitudeDivisions = 64, int longitudeDivisions = 64)
        {
            height = height * 2;
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            // Create the bottom face (centered at -height / 2)
            CreateCylinderEnd(vertices, indices, radius, -height / 2, latitudeDivisions, longitudeDivisions, true);

            // Create the top face (centered at height / 2)
            CreateCylinderEnd(vertices, indices, radius, height / 2, latitudeDivisions, longitudeDivisions, false);

            // Create the side surface of the cylinder
            CreateCylinderSides(vertices, indices, radius, height, latitudeDivisions, longitudeDivisions);

            return CreatePrimitiveModel(vertices.ToArray(), indices.ToArray());
        }

        private static void CreateCylinderEnd(List<VertexPositionNormalTexture> vertices, List<short> indices, float radius, float offsetY, int latitudeDivisions, int longitudeDivisions, bool bottom)
        {
            int vertexCount = vertices.Count;

            // The center vertex for the end
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, offsetY, 0),
                new Vector3(0, bottom ? -1 : 1, 0), new Vector2(0.5f, 0.5f)));  // Center vertex at offsetY

            // Generate vertices for the outer edge of the end
            for (int lon = 0; lon <= longitudeDivisions; lon++)
            {
                float phi = lon * 2 * MathF.PI / longitudeDivisions; // Longitude angle (0 to 2Pi)

                float x = (bottom ? -1 : 1) * radius * MathF.Cos(phi);
                float z = radius * MathF.Sin(phi);

                // Normal vector pointing up for top or down for bottom
                Vector3 normal = new Vector3(0, bottom ? -1 : 1, 0); // Adjust normal for top/bottom face

                // Texture coordinates (circular mapping)
                float u = (MathF.Cos(phi) + 1) / 2;  // Horizontal texture wrap
                float v = (MathF.Sin(phi) + 1) / 2;  // Vertical texture wrap

                // Add outer vertex
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, offsetY, z), normal, new Vector2(u, v)));
            }

            // Create indices for the end (form triangles from the center vertex)
            for (int lon = 0; lon < longitudeDivisions; lon++)
            {
                int current = vertexCount + 1 + lon;
                int next = vertexCount + 1 + ((lon + 1) % longitudeDivisions);

                // Create a triangle for each sector of the circle (from center vertex to two outer vertices)
                indices.Add((short)(vertexCount));  // Center vertex
                indices.Add((short)(current));
                indices.Add((short)(next));
            }
        }

        private static void CreateCylinderSides(List<VertexPositionNormalTexture> vertices, List<short> indices, float radius, float height, int latitudeDivisions, int longitudeDivisions)
        {
            int vertexCount = vertices.Count;

            // Generate vertices for the sides of the cylinder
            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float y = height * (lat / (float)latitudeDivisions - 0.5f); // Linear interpolation for height (cylinder)

                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions; // Longitude angle (0 to 2Pi)

                    // Spherical to Cartesian conversion for the cylinder's sides
                    float x = radius * MathF.Cos(phi);
                    float z = radius * MathF.Sin(phi);

                    // Normal vector (pointing outward along the XZ plane)
                    Vector3 normal = new Vector3(x, 0, z);
                    normal.Normalize();

                    // Texture coordinates (cylindrical mapping)
                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    // Add vertex
                    vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, z), normal, new Vector2(u, v)));
                }
            }

            // Create indices for the sides (side surface)
            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = vertexCount + lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    // Two triangles per sector
                    indices.Add((short)current);
                    indices.Add((short)(current + 1));
                    indices.Add((short)next);

                    indices.Add((short)(current + 1));
                    indices.Add((short)(next + 1));
                    indices.Add((short)next);
                }
            }
        }

        public static StaticMesh CreateCone(float radius, float height, int divisions = 64)
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            // Center vertex for the base of the cone
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Up, new Vector2(0.5f, 0.5f)));

            // Vertices for the base of the cone (circle), normal is pointing up now
            for (int i = 0; i < divisions; i++)
            {
                float angle = MathF.PI * 2 * i / divisions;
                float x = radius * MathF.Cos(angle);
                float z = radius * MathF.Sin(angle);

                // Invert the normal to point up
                Vector3 normal = Vector3.Up;

                // Texture coordinates (simple radial mapping)
                float u = (x / radius + 1) / 2;
                float v = (z / radius + 1) / 2;

                // Add base vertex
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, 0, z), normal, new Vector2(u, v)));
            }

            // Vertex for the tip of the cone
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, height, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));

            // Create indices for the base (fan) - invert the winding order for the base
            for (int i = 1; i < divisions - 1; i++)
            {
                indices.Add(0); // Center of the base
                indices.Add((short)(i + 1)); // Reverse order of the base vertices
                indices.Add((short)(i));
            }
            // Last triangle in the base fan (inverted winding order)
            indices.Add(0);
            indices.Add(1); // Reverse order for the last triangle
            indices.Add((short)(divisions - 1));

            // Create indices for the sides (cone surface)
            int tipIndex = vertices.Count - 1;
            for (int i = 1; i < divisions; i++)
            {
                indices.Add((short)i);
                indices.Add((short)((i + 1) % divisions + 1)); // Wrap around to 1 if i is the last vertex
                indices.Add((short)tipIndex);
            }

            float offsetY = -.5f;
            List<VertexPositionNormalTexture> VertexList = new List<VertexPositionNormalTexture>();
            for (int i = 0; i < vertices.ToArray().Length; i++)
            {
                VertexList.Add( new VertexPositionNormalTexture(
                        new Vector3(vertices[i].Position.X, vertices[i].Position.Y + offsetY, vertices[i].Position.Z),
                        vertices[i].Normal,
                        vertices[i].TextureCoordinate
                    ));
            }


            return CreatePrimitiveModel(VertexList.ToArray(), indices.ToArray());
        }



        private static StaticMesh CreatePrimitiveModel(VertexPositionNormalTexture[] vertices, short[] indices)
        {
            var vertexBuffer = new VertexBuffer(MainEngine.Instance.GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            var indexBuffer = new IndexBuffer(MainEngine.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);
            StaticMesh.SubMesh subMesh = new StaticMesh.SubMesh(vertexBuffer, indexBuffer, vertices.Length, indices.Length);

            StaticMesh Mesh = new StaticMesh
            {
                SubMeshes = new List<StaticMesh.SubMesh>
                {
                    subMesh
                }
            };
            Mesh.CalculateBoundingSpheres(MainEngine.Instance.GraphicsDevice);
            return Mesh;
        }
    }
}
