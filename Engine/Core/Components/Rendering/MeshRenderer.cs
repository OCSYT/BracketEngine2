using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Engine.Core.ECS;
using Engine.Core.Rendering;

namespace Engine.Core.Components.Rendering
{
    public class MeshRenderer : Component
    {
        public Model Model { get; set; }
        public StaticMesh StaticMesh { get; set; }
        public Material[] Materials { get; set; }
        private Dictionary<int, Effect> EffectCache = new Dictionary<int, Effect>();
        private Dictionary<int, Material> LastMaterialCache = new Dictionary<int, Material>();

        public MeshRenderer(Model model, Material[] materials = null)
        {
            Model = model;
            StaticMesh = null; // Default is Model
            Materials = materials ?? new Material[model?.Meshes.Count ?? 0];
        }

        public MeshRenderer(StaticMesh staticMesh, Material[] materials = null)
        {
            StaticMesh = staticMesh;
            Model = null; // Use StaticMesh instead of Model
            Materials = materials ?? new Material[staticMesh.SubMeshes.Count];
        }

        public override void Render(BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            var transform = ECSManager.Instance.GetComponent<Transform>(EntityId);
            if (transform == null) { return; }

            var worldMatrix = transform.GetWorldMatrix();
            var viewProjectionMatrix = viewMatrix * projectionMatrix;
            var frustum = new BoundingFrustum(viewProjectionMatrix);

            if (Model != null) // Handle Model Rendering
            {
                foreach (var mesh in Model.Meshes)
                {
                    var boundingSphere = mesh.BoundingSphere.Transform(worldMatrix);
                    if (!frustum.Intersects(boundingSphere)) continue;

                    for (int i = 0; i < mesh.MeshParts.Count; i++)
                    {
                        var part = mesh.MeshParts[i];
                        Effect partEffect = null;

                        // Check if we have a specific material for the mesh part
                        if (Materials != null && i < Materials.Length && Materials[i] != null)
                        {
                            var material = Materials[i];

                            // Handle effect caching per material
                            if (!LastMaterialCache.ContainsKey(i) || LastMaterialCache[i] != material)
                            {
                                if (EffectCache.ContainsKey(i))
                                {
                                    EffectCache.Remove(i);
                                }

                                // Clone effect based on material or use the default effect
                                partEffect = material.Shader?.Clone() ?? effect.Clone();
                                EffectCache[i] = partEffect;
                                LastMaterialCache[i] = material;
                            }
                            else
                            {
                                partEffect = EffectCache[i];
                            }

                            part.Effect = partEffect;
                            material.ApplyEffectParameters(partEffect, effect, true);
                        }
                        else
                        {
                            // If no material provided, use default effect
                            if (!EffectCache.ContainsKey(i))
                            {
                                partEffect = effect.Clone();
                                EffectCache[i] = partEffect;
                            }
                            else
                            {
                                partEffect = EffectCache[i];
                            }

                            part.Effect = partEffect;
                            Material.Default.ApplyEffectParameters(partEffect, effect, true);
                        }

                        if (partEffect is BasicEffect basicEffect)
                        {
                            basicEffect.World = worldMatrix;
                            basicEffect.View = viewMatrix;
                            basicEffect.Projection = projectionMatrix;
                        }
                    }

                    mesh.Draw();
                }
            }
            else if (StaticMesh != null)
            {
                int subMeshCount = StaticMesh.SubMeshes.Count;
                int materialCount = Materials?.Length ?? 0;

                for (int i = 0; i < subMeshCount; i++)
                {
                    var subMesh = StaticMesh.SubMeshes[i];

                    EngineManager.Instance.Graphics.GraphicsDevice.SetVertexBuffer(subMesh.VertexBuffer);
                    EngineManager.Instance.Graphics.GraphicsDevice.Indices = subMesh.IndexBuffer;

                    Effect subMeshEffect = null;

                    if (Materials != null && i < materialCount && Materials[i] != null)
                    {
                        var material = Materials[i];

                        // Handle effect caching for static meshes
                        if (!EffectCache.ContainsKey(i) || EffectCache[i] == null || LastMaterialCache[i] != material)
                        {
                            if (EffectCache.ContainsKey(i)) { EffectCache.Remove(i); }
                            subMeshEffect = material.Shader?.Clone() ?? effect.Clone();
                            EffectCache[i] = subMeshEffect;
                            LastMaterialCache[i] = material;
                        }
                        else
                        {
                            subMeshEffect = EffectCache[i];
                        }
                        material.ApplyEffectParameters(subMeshEffect, effect, true);
                    }
                    else
                    {
                        if (!EffectCache.ContainsKey(i))
                        {
                            subMeshEffect = effect.Clone();
                            EffectCache[i] = subMeshEffect;
                        }
                        else
                        {
                            subMeshEffect = EffectCache[i];
                        }

                        Material.Default.ApplyEffectParameters(subMeshEffect, effect, true);
                    }
                    if (subMeshEffect is BasicEffect basicEffect)
                    {
                        basicEffect.World = worldMatrix;
                        basicEffect.View = viewMatrix;
                        basicEffect.Projection = projectionMatrix;
                    }

                    // Apply the effect
                    subMeshEffect.CurrentTechnique.Passes[0].Apply();

                    // Draw the static mesh
                    EngineManager.Instance.Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subMesh.NumIndices/3);

                    EngineManager.Instance.Graphics.GraphicsDevice.SetVertexBuffer(null);
                    EngineManager.Instance.Graphics.GraphicsDevice.Indices = null;
                }
            }
        }
    }
}
