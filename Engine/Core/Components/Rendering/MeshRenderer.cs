using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Engine.Core.ECS;
using Engine.Core.Rendering;
using Engine.Core.Components;

namespace Engine.Core.Components.Rendering
{
    public class MeshRenderer : Component
    {
        public Model Model { get; set; }
        public StaticMesh StaticMesh { get; set; }
        public Material[] Materials { get; set; }
        private readonly Dictionary<int, Effect> _effectCache = new();
        private readonly Dictionary<int, Material> _lastMaterialCache = new();

        public MeshRenderer(Model model, Material[] materials = null)
        {
            Model = model;
            StaticMesh = null;
            Materials = materials ?? new Material[model?.Meshes.Count ?? 0];
        }

        public MeshRenderer(StaticMesh staticMesh, Material[] materials = null)
        {
            StaticMesh = staticMesh;
            Model = null;
            Materials = materials ?? new Material[staticMesh.SubMeshes.Count];
        }

        public override void Render(BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            var transform = ECSManager.Instance.GetComponent<Transform>(EntityId);
            if (transform == null || EngineManager.Instance.DefaultShader == null) return;

            var worldMatrix = transform.GetWorldMatrix();
            var viewProjectionMatrix = viewMatrix * projectionMatrix;
            var frustum = new BoundingFrustum(viewProjectionMatrix);

            if (Model != null)
            {
                RenderModel(worldMatrix, viewMatrix, projectionMatrix, frustum);
            }
            else if (StaticMesh != null)
            {
                RenderStaticMesh(worldMatrix, viewMatrix, projectionMatrix, frustum);
            }
        }

        private void RenderModel(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum frustum)
        {
            var defaultShader = EngineManager.Instance.DefaultShader;

            foreach (var mesh in Model.Meshes)
            {
                if (!frustum.Intersects(mesh.BoundingSphere.Transform(worldMatrix))) continue;

                foreach (var part in mesh.MeshParts)
                {
                    int partIndex = mesh.MeshParts.IndexOf(part);
                    var effect = GetOrCreateEffect(partIndex, Materials, defaultShader);

                    part.Effect = effect;
                    ApplyEffectParameters(effect, worldMatrix, viewMatrix, projectionMatrix, Materials?[partIndex]);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }

                mesh.Draw();
            }
        }

        private void RenderStaticMesh(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum frustum)
        {
            var defaultShader = EngineManager.Instance.DefaultShader;
            var device = EngineManager.Instance.Graphics.GraphicsDevice;

            foreach (var subMesh in StaticMesh.SubMeshes)
            {
                int subMeshIndex = StaticMesh.SubMeshes.IndexOf(subMesh);

                if (!frustum.Intersects(subMesh.BoundingSphere.Transform(worldMatrix))) continue;

                device.SetVertexBuffer(subMesh.VertexBuffer);
                device.Indices = subMesh.IndexBuffer;

                var effect = GetOrCreateEffect(subMeshIndex, Materials, defaultShader);
                ApplyEffectParameters(effect, worldMatrix, viewMatrix, projectionMatrix, Materials?[subMeshIndex]);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subMesh.NumIndices / 3);
            }

            // Reset device state to avoid conflicts in subsequent renders
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

        private void ApplyEffectParameters(Effect effect, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Material material)
        {
            if (effect == null) return;

            LightManager.Instance.UpdateLights(ref effect);

            try
            {
                effect.Parameters["World"]?.SetValue(worldMatrix);
                effect.Parameters["View"]?.SetValue(viewMatrix);
                effect.Parameters["Projection"]?.SetValue(projectionMatrix);
            }
            catch
            {

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
