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
        public Texture2D EmissionTexture { get; set; }
        public Effect Shader { get; set; }
        public Color DiffuseColor { get; set; } = Color.White;
        public Color EmissionColor { get; set; } = Color.Black;
        public Dictionary<string, object> ShaderParams { get; set; } = new Dictionary<string, object>();
        public static Material Default { get; } = new Material();
        public float Alpha { get; set; } = 1;
        public bool Lighting { get; set; } = true;
        public bool Transparent = false;
        public int SortOrder;
        public DepthStencilState DepthStencilState { get; set; } = DepthStencilState.Default;
        public Material()
        {
        }

        public Material(Texture2D diffuseTexture = null,
                        Color diffuseColor = default,
                        Color ambientColor = default,
                        Texture2D emissionTexture = null,
                        Color emissive = default,
                        Effect shader = null,
                        float alpha = 1,
                        bool transparent = false, bool lighting = true)
        {
            DiffuseTexture = diffuseTexture;
            EmissionTexture = emissionTexture;
            DiffuseColor = diffuseColor == default ? Color.White : diffuseColor;
            EmissionColor = emissive == default ? Color.Black : emissive;
            Shader = shader;
            Alpha = alpha;
            Lighting = lighting;
        }

        public void ApplyEffectParameters(Effect effect, bool fallback)
        {
            if (effect == null) return;
            if (SortOrder == 0 || SortOrder == 1)
            {
                if (Transparent)
                {
                    SortOrder = 1;
                }
                else
                {
                    SortOrder = 0;
                }
            }
            EngineManager.Instance.GraphicsDevice.DepthStencilState = DepthStencilState;
            if (Transparent)
            {
                EngineManager.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
            else
            {
                EngineManager.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            }
            if (!fallback)
            {
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
                effect.Parameters["Lighting"]?.SetValue(Lighting ? 1 : 0);
                effect.Parameters["Alpha"]?.SetValue(Alpha);
                if (DiffuseTexture != null)
                {
                    effect.Parameters["DiffuseTexture"]?.SetValue(DiffuseTexture);
                }
                else
                {
                    effect.Parameters["DiffuseTexture"]?.SetValue(EngineManager.Instance.WhiteTex);
                }
                if (EmissionTexture != null)
                {
                    effect.Parameters["EmissionTexture"]?.SetValue(EmissionTexture);
                }
                else
                {
                    effect.Parameters["EmissionTexture"]?.SetValue(EngineManager.Instance.WhiteTex);
                }
                effect.Parameters["DiffuseColor"]?.SetValue(DiffuseColor.ToVector4());
                effect.Parameters["EmissionColor"]?.SetValue(EmissionColor.ToVector4());

            }
        }

    }
}
