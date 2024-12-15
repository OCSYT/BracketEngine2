using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Core.Rendering
{
    public class StaticMesh
    {
        public List<SubMesh> SubMeshes;

        public StaticMesh()
        {
            SubMeshes = new List<SubMesh>();
        }

        public void AddSubMesh(VertexBuffer VertexBuffer, IndexBuffer IndexBuffer, int NumVertices, int NumIndices)
        {
            SubMeshes.Add(new SubMesh(VertexBuffer, IndexBuffer, NumVertices, NumIndices));
            CalculateBoundingSphereForSubMesh(SubMeshes.Count - 1, EngineManager.Instance.Graphics.GraphicsDevice);
        }
        public void ModifySubMesh(int subMeshIndex, VertexBuffer newVertexBuffer, IndexBuffer newIndexBuffer, int newNumVertices, int newNumIndices)
        {
            if (subMeshIndex >= 0 && subMeshIndex < SubMeshes.Count)
            {
                SubMesh subMesh = SubMeshes[subMeshIndex];

                subMesh.VertexBuffer = newVertexBuffer;
                subMesh.IndexBuffer = newIndexBuffer;
                subMesh.NumVertices = newNumVertices;
                subMesh.NumIndices = newNumIndices;

                CalculateBoundingSphereForSubMesh(subMeshIndex, EngineManager.Instance.Graphics.GraphicsDevice);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(subMeshIndex), "Invalid SubMesh index.");
            }
        }

        public void CalculateBoundingSpheres(GraphicsDevice graphicsDevice)
        {
            foreach (var subMesh in SubMeshes)
            {
                subMesh.BoundingSphere = CalculateBoundingSphere(subMesh.VertexBuffer, graphicsDevice);
            }
        }

        public void CalculateBoundingSphereForSubMesh(int subMeshIndex, GraphicsDevice graphicsDevice)
        {
            if (subMeshIndex >= 0 && subMeshIndex < SubMeshes.Count)
            {
                var subMesh = SubMeshes[subMeshIndex];
                subMesh.BoundingSphere = CalculateBoundingSphere(subMesh.VertexBuffer, graphicsDevice);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(subMeshIndex), "Invalid SubMesh index.");
            }
        }

        private BoundingSphere CalculateBoundingSphere(VertexBuffer vertexBuffer, GraphicsDevice graphicsDevice)
        {
            VertexPosition[] vertices = new VertexPosition[vertexBuffer.VertexCount];
            vertexBuffer.GetData(vertices);

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var vertex in vertices)
            {
                Vector3 position = vertex.Position;
                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);
            }

            Vector3 center = (min + max) / 2f;

            float radiusSquared = 0f;
            foreach (var vertex in vertices)
            {
                float distanceSquared = Vector3.DistanceSquared(center, vertex.Position);
                radiusSquared = Math.Max(radiusSquared, distanceSquared);
            }
            return new BoundingSphere(center, (float)Math.Sqrt(radiusSquared));
        }


        public class SubMesh
        {
            public BoundingSphere BoundingSphere;
            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;
            public int NumVertices;
            public int NumIndices;

            public SubMesh(VertexBuffer VertexBuffer, IndexBuffer IndexBuffer, int NumVertices, int NumIndices)
            {
                this.VertexBuffer = VertexBuffer;
                this.IndexBuffer = IndexBuffer;
                this.NumVertices = NumVertices;
                this.NumIndices = NumIndices;
            }
        }
    }
}
