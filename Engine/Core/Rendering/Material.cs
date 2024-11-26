using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Core.Rendering
{
    public class Material
    {
        public Texture2D DiffuseTexture { get; set; }
        public Effect Shader { get; set; }
        public Color DiffuseColor { get; set; } = Color.White;
        public Color Specular { get; set; } = Color.Black;
        public Color Emissive { get; set; } = Color.Black;
        public Dictionary<string, object> ShaderParams { get; set; } = new Dictionary<string, object>();
        public static Material Default { get; } = new Material();
        public float Alpha { get; set; } = 1;
        public bool Transparent { get; set; } = false;

        public Material()
        {
        }

        public Material(Texture2D diffuseTexture,
                        Color diffuseColor = default,
                        Color specular = default,
                        Color emissive = default,
                        Effect shader = null,
                        float alpha = 1,
                        bool transparent = false)
        {
            DiffuseTexture = diffuseTexture;
            DiffuseColor = diffuseColor == default ? Color.White : diffuseColor;
            Specular = specular == default ? Color.Black : specular;
            Emissive = emissive == default ? Color.Black : emissive;
            Shader = shader;
            Alpha = alpha;
            Transparent = transparent;
        }

        public void ApplyEffectParameters(Effect effect, BasicEffect basicEffect, bool fallback)
        {
            if (effect == null) return;

            EngineManager.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            if (!fallback)
            {
                try
                {
                    Matrix worldViewProjection = basicEffect.World * basicEffect.View * basicEffect.Projection;
                    effect.Parameters["WorldViewProjection"]?.SetValue(worldViewProjection);
                }
                catch { }

                try
                {
                    foreach (KeyValuePair<string, object> kvp in ShaderParams)
                    {
                        var paramValue = kvp.Value;
                        var paramType = paramValue.GetType();

                        if (paramType == typeof(Matrix))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((Matrix)paramValue);
                        }
                        else if (paramType == typeof(Texture2D))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((Texture2D)paramValue);
                        }
                        else if (paramType == typeof(Vector4))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((Vector4)paramValue);
                        }
                        else if (paramType == typeof(Vector3))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((Vector3)paramValue);
                        }
                        else if (paramType == typeof(float))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((float)paramValue);
                        }
                        else if (paramType == typeof(int))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((int)paramValue);
                        }
                        else if (paramType == typeof(bool))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((bool)paramValue);
                        }
                        else if (paramType == typeof(TextureCube))
                        {
                            effect.Parameters[kvp.Key]?.SetValue((TextureCube)paramValue);
                        }
                        else if (paramType.IsArray)
                        {
                            var elementType = paramType.GetElementType();

                            if (elementType == typeof(float))
                            {
                                effect.Parameters[kvp.Key]?.SetValue((float[])paramValue);
                            }
                            else if (elementType == typeof(Vector3))
                            {
                                effect.Parameters[kvp.Key]?.SetValue((Vector3[])paramValue);
                            }
                            else if (elementType == typeof(Vector4))
                            {
                                effect.Parameters[kvp.Key]?.SetValue((Vector4[])paramValue);
                            }
                            else if (elementType == typeof(Matrix))
                            {
                                effect.Parameters[kvp.Key]?.SetValue((Matrix[])paramValue);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                BasicEffect basicEffectInstance = effect as BasicEffect;
                if (Transparent)
                {
                    basicEffectInstance.Alpha = Alpha;
                }
                else
                {
                    basicEffectInstance.Alpha = 1;
                }

                basicEffectInstance.EmissiveColor = Emissive.ToVector3();
                basicEffectInstance.SpecularColor = Specular.ToVector3();
                basicEffectInstance.DiffuseColor = DiffuseColor.ToVector3();
                if (DiffuseTexture != null)
                {
                    basicEffectInstance.TextureEnabled = true;
                    basicEffectInstance.Texture = DiffuseTexture;
                }
                else
                {
                    basicEffectInstance.TextureEnabled = false;
                }
            }
        }
    }
}
