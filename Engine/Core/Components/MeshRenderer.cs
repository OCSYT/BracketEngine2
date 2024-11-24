using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Engine.Core.ECS;

namespace Engine.Core.Components
{
    public class MeshRenderer : Component
    {
        public Model Model { get; set; }
        public Material[] Materials { get; set; }
        private Dictionary<int, Effect> EffectCache = new Dictionary<int, Effect>();
        private Dictionary<int, Material> LastMaterialCache = new Dictionary<int, Material>();

        public MeshRenderer(Model model, Material[] materials = null)
        {
            Model = model;
            Materials = materials ?? new Material[model.Meshes.Count];
        }

        public override void Render(Effect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            BasicEffect basicEffect = effect as BasicEffect;
            var transform = ECSManager.Instance.GetComponent<Transform>(EntityId);
            if (transform == null) { return; }

            var worldMatrix = transform.GetWorldMatrix();
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;

            var viewProjectionMatrix = viewMatrix * projectionMatrix;
            var frustum = new BoundingFrustum(viewProjectionMatrix);

            foreach (var mesh in Model.Meshes)
            {
                var boundingSphere = mesh.BoundingSphere.Transform(worldMatrix);
                if (!frustum.Intersects(boundingSphere))
                {
                    continue;
                }

                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    var part = mesh.MeshParts[i];
                    Effect partEffect = null;

                    if (Materials != null && i < Materials.Length && Materials[i] != null)
                    {
                        var material = Materials[i];

                        if (!LastMaterialCache.ContainsKey(i) || LastMaterialCache[i] != material)
                        {
                            if (EffectCache.ContainsKey(i))
                            {
                                EffectCache.Remove(i);
                            }

                            if (material.Shader == null)
                            {
                                partEffect = effect.Clone();
                            }
                            else
                            {
                                partEffect = material.Shader.Clone();
                            }
                            EffectCache[i] = partEffect;

                            LastMaterialCache[i] = material;
                        }
                        else
                        {
                            partEffect = EffectCache[i];
                        }

                        part.Effect = partEffect;

                        if (material.Shader != null)
                        {
                            material.ApplyEffectParameters(part.Effect, basicEffect, false);
                        }
                        else
                        {
                            material.ApplyEffectParameters(part.Effect, basicEffect, true);
                        }
                    }
                    else
                    {
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
                        Material.Default.ApplyEffectParameters(part.Effect, basicEffect, true);
                    }

                    if (part.Effect is BasicEffect basicPartEffect)
                    {
                        basicPartEffect.World = worldMatrix;
                        basicPartEffect.View = viewMatrix;
                        basicPartEffect.Projection = projectionMatrix;
                    }
                }

                mesh.Draw();
            }
        }
    }
}
