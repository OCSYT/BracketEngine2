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
        }

        public class SubMesh
        {
            public VertexBuffer VertexBuffer { get; }
            public IndexBuffer IndexBuffer { get; }
            public int NumVertices { get; }
            public int NumIndices { get; }

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
