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
            var vertices = new VertexPositionNormalTexture[24];

            vertices[0] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, -depth),
                Vector3.Backward,
                new Vector2(0, 1)
            );
            vertices[1] = new VertexPositionNormalTexture(
                new Vector3(width, -height, -depth),
                Vector3.Backward,
                new Vector2(1, 1)
            );
            vertices[2] = new VertexPositionNormalTexture(
                new Vector3(width, height, -depth),
                Vector3.Backward,
                new Vector2(1, 0)
            );
            vertices[3] = new VertexPositionNormalTexture(
                new Vector3(-width, height, -depth),
                Vector3.Backward,
                new Vector2(0, 0)
            );
            vertices[4] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, depth),
                Vector3.Forward,
                new Vector2(0, 1)
            );
            vertices[5] = new VertexPositionNormalTexture(
                new Vector3(width, -height, depth),
                Vector3.Forward,
                new Vector2(1, 1)
            );
            vertices[6] = new VertexPositionNormalTexture(
                new Vector3(width, height, depth),
                Vector3.Forward,
                new Vector2(1, 0)
            );
            vertices[7] = new VertexPositionNormalTexture(
                new Vector3(-width, height, depth),
                Vector3.Forward,
                new Vector2(0, 0)
            );
            vertices[8] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, -depth),
                Vector3.Left,
                new Vector2(0, 1)
            );
            vertices[9] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, depth),
                Vector3.Left,
                new Vector2(1, 1)
            );
            vertices[10] = new VertexPositionNormalTexture(
                new Vector3(-width, height, depth),
                Vector3.Left,
                new Vector2(1, 0)
            );
            vertices[11] = new VertexPositionNormalTexture(
                new Vector3(-width, height, -depth),
                Vector3.Left,
                new Vector2(0, 0)
            );

            vertices[12] = new VertexPositionNormalTexture(
                new Vector3(width, -height, -depth),
                Vector3.Right,
                new Vector2(0, 1)
            );
            vertices[13] = new VertexPositionNormalTexture(
                new Vector3(width, -height, depth),
                Vector3.Right,
                new Vector2(1, 1)
            );
            vertices[14] = new VertexPositionNormalTexture(
                new Vector3(width, height, depth),
                Vector3.Right,
                new Vector2(1, 0)
            );
            vertices[15] = new VertexPositionNormalTexture(
                new Vector3(width, height, -depth),
                Vector3.Right,
                new Vector2(0, 0)
            );

            vertices[16] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, -depth),
                Vector3.Down,
                new Vector2(0, 1)
            );
            vertices[17] = new VertexPositionNormalTexture(
                new Vector3(width, -height, -depth),
                Vector3.Down,
                new Vector2(1, 1)
            );
            vertices[18] = new VertexPositionNormalTexture(
                new Vector3(width, -height, depth),
                Vector3.Down,
                new Vector2(1, 0)
            );
            vertices[19] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, depth),
                Vector3.Down,
                new Vector2(0, 0)
            );

            vertices[20] = new VertexPositionNormalTexture(
                new Vector3(-width, height, -depth),
                Vector3.Up,
                new Vector2(0, 1)
            );
            vertices[21] = new VertexPositionNormalTexture(
                new Vector3(width, height, -depth),
                Vector3.Up,
                new Vector2(1, 1)
            );
            vertices[22] = new VertexPositionNormalTexture(
                new Vector3(width, height, depth),
                Vector3.Up,
                new Vector2(1, 0)
            );
            vertices[23] = new VertexPositionNormalTexture(
                new Vector3(-width, height, depth),
                Vector3.Up,
                new Vector2(0, 0)
            );

            short[] indices = new short[]
            {
                0,
                2,
                3,
                0,
                1,
                2,
                4,
                7,
                6,
                4,
                6,
                5,
                8,
                11,
                10,
                8,
                10,
                9,
                12,
                14,
                15,
                12,
                13,
                14,
                16,
                19,
                18,
                16,
                18,
                17,
                20,
                22,
                23,
                20,
                21,
                22
            };

            return CreateStaticMesh(vertices, indices);
        }
        public static StaticMesh CreateQuad(float width, float height)
        {
            var vertices = new VertexPositionNormalTexture[4];

            vertices[0] = new VertexPositionNormalTexture(
                new Vector3(-width, -height, 0),
                Vector3.Backward,
                new Vector2(0, 1)
            );
            vertices[1] = new VertexPositionNormalTexture(
                new Vector3(width, -height, 0),
                Vector3.Backward,
                new Vector2(1, 1)
            );
            vertices[2] = new VertexPositionNormalTexture(
                new Vector3(width, height, 0),
                Vector3.Backward,
                new Vector2(1, 0)
            );
            vertices[3] = new VertexPositionNormalTexture(
                new Vector3(-width, height, 0),
                Vector3.Backward,
                new Vector2(0, 0)
            );
            short[] indices = new short[] { 0, 2, 3, 0, 1, 2 };

            return CreateStaticMesh(vertices, indices);
        }

        public static StaticMesh CreateSphere(
            float radius,
            int latitudeDivisions = 64,
            int longitudeDivisions = 64
        )
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float theta = lat * MathF.PI / latitudeDivisions;
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions;
                    float x = -radius * MathF.Sin(theta) * MathF.Cos(phi);
                    float y = radius * MathF.Cos(theta);
                    float z = radius * MathF.Sin(theta) * MathF.Sin(phi);

                    Vector3 normal = new Vector3(x, y, z);
                    normal.Normalize();

                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    vertices.Add(
                        new VertexPositionNormalTexture(
                            new Vector3(x, y, z),
                            normal,
                            new Vector2(u, v)
                        )
                    );
                }
            }

            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    if (lat != 0)
                    {
                        indices.Add((short)current);
                        indices.Add((short)(current + 1));
                        indices.Add((short)next);
                    }

                    if (lat != latitudeDivisions - 1)
                    {
                        indices.Add((short)(current + 1));
                        indices.Add((short)(next + 1));
                        indices.Add((short)next);
                    }
                }
            }

            return CreateStaticMesh(vertices.ToArray(), indices.ToArray());
        }

        public static StaticMesh CreateCapsule(
            float radius,
            float height,
            int latitudeDivisions = 64,
            int longitudeDivisions = 64
        )
        {
            height = height / 2;
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            CreateHemisphere(
                vertices,
                indices,
                radius,
                -height / 2,
                latitudeDivisions,
                longitudeDivisions
            );

            CreateCylinderVertex(
                vertices,
                indices,
                radius,
                height,
                latitudeDivisions,
                longitudeDivisions
            );

            CreateHemisphere(
                vertices,
                indices,
                radius,
                height / 2,
                latitudeDivisions,
                longitudeDivisions
            );

            return CreateStaticMesh(vertices.ToArray(), indices.ToArray());
        }

        private static void CreateHemisphere(
            List<VertexPositionNormalTexture> vertices,
            List<short> indices,
            float radius,
            float offsetY,
            int latitudeDivisions,
            int longitudeDivisions
        )
        {
            int vertexCount = vertices.Count;

            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float theta = lat * MathF.PI / latitudeDivisions;
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions;
                    float x = -radius * MathF.Sin(theta) * MathF.Cos(phi);
                    float y = radius * MathF.Cos(theta) + offsetY;
                    float z = radius * MathF.Sin(theta) * MathF.Sin(phi);

                    Vector3 normal = new Vector3(x, y, z);
                    normal.Normalize();

                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    vertices.Add(
                        new VertexPositionNormalTexture(
                            new Vector3(x, y, z),
                            normal,
                            new Vector2(u, v)
                        )
                    );
                }
            }

            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = vertexCount + lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    if (lat != 0)
                    {
                        indices.Add((short)current);
                        indices.Add((short)(current + 1));
                        indices.Add((short)next);
                    }

                    if (lat != latitudeDivisions - 1)
                    {
                        indices.Add((short)(current + 1));
                        indices.Add((short)(next + 1));
                        indices.Add((short)next);
                    }
                }
            }
        }

        private static void CreateCylinderVertex(
            List<VertexPositionNormalTexture> vertices,
            List<short> indices,
            float radius,
            float height,
            int latitudeDivisions,
            int longitudeDivisions
        )
        {
            int vertexCount = vertices.Count;

            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float theta = lat * MathF.PI / latitudeDivisions;
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions;
                    float x = radius * MathF.Cos(phi);
                    float z = radius * MathF.Sin(phi);
                    float y = height * (lat / (float)latitudeDivisions - 0.5f);
                    Vector3 normal = new Vector3(x, 0, z);
                    normal.Normalize();

                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    vertices.Add(
                        new VertexPositionNormalTexture(
                            new Vector3(x, y, z),
                            normal,
                            new Vector2(u, v)
                        )
                    );
                }
            }

            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = vertexCount + lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

                    if (lat != 0 && lat != latitudeDivisions - 1)
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
        public static StaticMesh CreateCylinder(
            float radius,
            float height,
            int latitudeDivisions = 64,
            int longitudeDivisions = 64
        )
        {
            height = height * 2;
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<short> indices = new List<short>();

            CreateCylinderEnd(
                vertices,
                indices,
                radius,
                -height / 2,
                latitudeDivisions,
                longitudeDivisions,
                true
            );

            CreateCylinderEnd(
                vertices,
                indices,
                radius,
                height / 2,
                latitudeDivisions,
                longitudeDivisions,
                false
            );

            CreateCylinderSides(
                vertices,
                indices,
                radius,
                height,
                latitudeDivisions,
                longitudeDivisions
            );

            return CreateStaticMesh(vertices.ToArray(), indices.ToArray());
        }

        private static void CreateCylinderEnd(
            List<VertexPositionNormalTexture> vertices,
            List<short> indices,
            float radius,
            float offsetY,
            int latitudeDivisions,
            int longitudeDivisions,
            bool bottom
        )
        {
            int vertexCount = vertices.Count;

            vertices.Add(
                new VertexPositionNormalTexture(
                    new Vector3(0, offsetY, 0),
                    new Vector3(0, bottom ? -1 : 1, 0),
                    new Vector2(0.5f, 0.5f)
                )
            );
            for (int lon = 0; lon <= longitudeDivisions; lon++)
            {
                float phi = lon * 2 * MathF.PI / longitudeDivisions;
                float x = (bottom ? -1 : 1) * radius * MathF.Cos(phi);
                float z = radius * MathF.Sin(phi);

                Vector3 normal = new Vector3(0, bottom ? -1 : 1, 0);
                float u = (MathF.Cos(phi) + 1) / 2;
                float v = (MathF.Sin(phi) + 1) / 2;
                vertices.Add(
                    new VertexPositionNormalTexture(
                        new Vector3(x, offsetY, z),
                        normal,
                        new Vector2(u, v)
                    )
                );
            }

            for (int lon = 0; lon < longitudeDivisions; lon++)
            {
                int current = vertexCount + 1 + lon;
                int next = vertexCount + 1 + ((lon + 1) % longitudeDivisions);

                indices.Add((short)(vertexCount));
                indices.Add((short)(current));
                indices.Add((short)(next));
            }
        }

        private static void CreateCylinderSides(
            List<VertexPositionNormalTexture> vertices,
            List<short> indices,
            float radius,
            float height,
            int latitudeDivisions,
            int longitudeDivisions
        )
        {
            int vertexCount = vertices.Count;

            for (int lat = 0; lat <= latitudeDivisions; lat++)
            {
                float y = height * (lat / (float)latitudeDivisions - 0.5f);
                for (int lon = 0; lon <= longitudeDivisions; lon++)
                {
                    float phi = lon * 2 * MathF.PI / longitudeDivisions;
                    float x = radius * MathF.Cos(phi);
                    float z = radius * MathF.Sin(phi);

                    Vector3 normal = new Vector3(x, 0, z);
                    normal.Normalize();

                    float u = (float)lon / longitudeDivisions;
                    float v = (float)lat / latitudeDivisions;

                    vertices.Add(
                        new VertexPositionNormalTexture(
                            new Vector3(x, y, z),
                            normal,
                            new Vector2(u, v)
                        )
                    );
                }
            }

            for (int lat = 0; lat < latitudeDivisions; lat++)
            {
                for (int lon = 0; lon < longitudeDivisions; lon++)
                {
                    int current = vertexCount + lat * (longitudeDivisions + 1) + lon;
                    int next = current + longitudeDivisions + 1;

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

            vertices.Add(
                new VertexPositionNormalTexture(Vector3.Zero, Vector3.Up, new Vector2(0.5f, 0.5f))
            );

            for (int i = 0; i < divisions; i++)
            {
                float angle = MathF.PI * 2 * i / divisions;
                float x = radius * MathF.Cos(angle);
                float z = radius * MathF.Sin(angle);

                Vector3 normal = Vector3.Up;

                float u = (x / radius + 1) / 2;
                float v = (z / radius + 1) / 2;

                vertices.Add(
                    new VertexPositionNormalTexture(new Vector3(x, 0, z), normal, new Vector2(u, v))
                );
            }

            vertices.Add(
                new VertexPositionNormalTexture(
                    new Vector3(0, height, 0),
                    Vector3.Up,
                    new Vector2(0.5f, 0.5f)
                )
            );

            for (int i = 1; i < divisions - 1; i++)
            {
                indices.Add(0);
                indices.Add((short)(i + 1));
                indices.Add((short)(i));
            }
            indices.Add(0);
            indices.Add(1);
            indices.Add((short)(divisions - 1));

            int tipIndex = vertices.Count - 1;
            for (int i = 1; i < divisions; i++)
            {
                indices.Add((short)i);
                indices.Add((short)((i + 1) % divisions + 1));
                indices.Add((short)tipIndex);
            }

            float offsetY = -.5f;
            List<VertexPositionNormalTexture> VertexList = new List<VertexPositionNormalTexture>();
            for (int i = 0; i < vertices.ToArray().Length; i++)
            {
                VertexList.Add(
                    new VertexPositionNormalTexture(
                        new Vector3(
                            vertices[i].Position.X,
                            vertices[i].Position.Y + offsetY,
                            vertices[i].Position.Z
                        ),
                        vertices[i].Normal,
                        vertices[i].TextureCoordinate
                    )
                );
            }

            return CreateStaticMesh(VertexList.ToArray(), indices.ToArray());
        }

        public static StaticMesh CreateStaticMesh(
            VertexPositionNormalTexture[] vertices,
            short[] indices
        )
        {
            var vertexBuffer = new VertexBuffer(
                EngineManager.Instance.GraphicsDevice,
                typeof(VertexPositionNormalTexture),
                vertices.Length,
                BufferUsage.None
            );
            vertexBuffer.SetData(vertices);

            var indexBuffer = new IndexBuffer(
                EngineManager.Instance.GraphicsDevice,
                IndexElementSize.SixteenBits,
                indices.Length,
                BufferUsage.None
            );
            indexBuffer.SetData(indices);
            StaticMesh.SubMesh subMesh = new StaticMesh.SubMesh(
                vertexBuffer,
                indexBuffer,
                vertices.Length,
                indices.Length
            );

            StaticMesh Mesh = new StaticMesh
            {
                SubMeshes = new List<StaticMesh.SubMesh> { subMesh }
            };
            Mesh.CalculateBoundingSpheres(EngineManager.Instance.GraphicsDevice);
            return Mesh;
        }
    }
}
