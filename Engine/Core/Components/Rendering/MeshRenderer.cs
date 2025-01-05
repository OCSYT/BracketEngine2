using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Engine.Core.ECS;
using Engine.Core.Rendering;
using Engine.Core.Components;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using Aether.Animation;
namespace Engine.Core.Components.Rendering
{
    public class MeshRenderer : Component
    {
        public Model Model { get; set; }
        public StaticMesh StaticMesh { get; set; }
        public Material[] Materials { get; set; }
        public Animations AnimationPlayer;
        private readonly Dictionary<int, Effect> _effectCache = new();
        private readonly Dictionary<int, Material> _lastMaterialCache = new();
        public int SortOrderTotal = 0;
        public MeshRenderer(Model model, Material[] materials = null, Animations animationPlayer = null)
        {
            Model = model;
            StaticMesh = null;
            Materials = materials ?? new Material[model?.Meshes.Count ?? 0];
            AnimationPlayer = animationPlayer;
        }

        public MeshRenderer(StaticMesh staticMesh, Material[] materials = null)
        {
            StaticMesh = staticMesh;
            Model = null;
            Materials = materials ?? new Material[staticMesh.SubMeshes.Count];
            AnimationPlayer = null;
        }


        private void CalculateSortOrder()
        {
            SortOrderTotal = 0;
            foreach (var material in Materials)
            {
                SortOrderTotal += material.SortOrder;
            }
        }

        public override void Awake()
        {
            CalculateSortOrder();
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            CalculateSortOrder();
        }

        public void RenderMesh(BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            var worldMatrix = Transform.GetWorldMatrix();
            var viewProjectionMatrix = viewMatrix * projectionMatrix;
            var frustum = new BoundingFrustum(viewProjectionMatrix);

            if (Model != null)
            {
                RenderModel(worldMatrix, viewMatrix, projectionMatrix, frustum, gameTime);
            }
            else if (StaticMesh != null)
            {
                RenderStaticMesh(worldMatrix, viewMatrix, projectionMatrix, frustum, gameTime);
            }
        }

        private void RenderModel(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum frustum, GameTime gameTime)
        {
            var defaultShader = EngineManager.Instance.DefaultShader;

            foreach (var mesh in Model.Meshes)
            {
                if (!frustum.Intersects(mesh.BoundingSphere.Transform(worldMatrix))) continue;

                var sortedMeshParts = mesh.MeshParts
                    .OrderBy(part => Materials?[mesh.MeshParts.IndexOf(part)].SortOrder ?? int.MaxValue)
                    .ToList();

                foreach (var part in sortedMeshParts)
                {
                    if (AnimationPlayer != null)
                    {
                        part.UpdateVertices(AnimationPlayer.AnimationTransforms);
                    }

                    int partIndex = mesh.MeshParts.IndexOf(part);
                    var effect = GetOrCreateEffect(partIndex, Materials, defaultShader);

                    part.Effect = effect;
                    ApplyEffectParameters(effect, worldMatrix, viewMatrix, projectionMatrix, Materials?[partIndex], (float)gameTime.TotalGameTime.TotalSeconds);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }

                mesh.Draw();
            }
        }

        private void RenderStaticMesh(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum frustum, GameTime gameTime)
        {
            var defaultShader = EngineManager.Instance.DefaultShader;
            var device = EngineManager.Instance.Graphics.GraphicsDevice;

            var sortedSubMeshes = StaticMesh.SubMeshes
                .OrderBy(subMesh => Materials?[StaticMesh.SubMeshes.IndexOf(subMesh)].SortOrder ?? int.MaxValue)
                .ToList();

            foreach (var subMesh in sortedSubMeshes)
            {
                int subMeshIndex = StaticMesh.SubMeshes.IndexOf(subMesh);

                if (!frustum.Intersects(subMesh.BoundingSphere.Transform(worldMatrix))) continue;

                device.SetVertexBuffer(subMesh.VertexBuffer);
                device.Indices = subMesh.IndexBuffer;

                var effect = GetOrCreateEffect(subMeshIndex, Materials, defaultShader);
                ApplyEffectParameters(effect, worldMatrix, viewMatrix, projectionMatrix, Materials?[subMeshIndex], (float)gameTime.TotalGameTime.TotalSeconds);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subMesh.NumIndices / 3);
            }

            device.SetVertexBuffer(null);
            device.Indices = null;
        }

        private Effect GetOrCreateEffect(int index, Material[] materials, Effect defaultShader)
        {
            if (materials != null && index < materials.Length && materials[index] != null)
            {
                var material = materials[index];

                if (!_lastMaterialCache.TryGetValue(index, out var cachedMaterial) || cachedMaterial != material)
                {
                    _effectCache[index] = material.Shader?.Clone() ?? defaultShader.Clone();
                    _lastMaterialCache[index] = material;
                }

                return _effectCache[index];
            }

            if (!_effectCache.TryGetValue(index, out var effect))
            {
                effect = defaultShader.Clone();
                _effectCache[index] = effect;
            }

            return effect;
        }

        private void ApplyEffectParameters(Effect effect, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Material material, float currentTime)
        {
            if (effect == null) return;

            LightManager.Instance.UpdateLights(ref effect);

            try
            {
                effect.Parameters["Time"]?.SetValue(currentTime);
            }
            catch
            {
                // Ignore missing parameter
            }

            try
            {
                effect.Parameters["World"]?.SetValue(worldMatrix);
                effect.Parameters["View"]?.SetValue(viewMatrix);
                effect.Parameters["Projection"]?.SetValue(projectionMatrix);
            }
            catch
            {
                // Ignore missing parameters
            }

            if (material != null)
            {
                material.ApplyEffectParameters(effect, material.Shader == null);
            }
            else
            {
                Material.Default.ApplyEffectParameters(effect, true);
            }
        }
    }
}
