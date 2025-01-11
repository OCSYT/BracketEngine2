using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Core.Rendering
{
    public class Material
    {
        public struct VertexPositionNormalTextureColor : IVertexType
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public Color Color;

            public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Color color)
            {
                Position = position;
                Normal = normal;
                TextureCoordinate = textureCoordinate;
                Color = color;
            }

            VertexDeclaration IVertexType.VertexDeclaration =>
                new VertexDeclaration(new VertexElement[]
                {
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0)
                });
        }

        public Texture2D BaseColorTexture { get; set; }
        public Texture2D EmissionColorTexture { get; set; }
        public Texture2D MetallicTexture { get; set; }
        public Texture2D RoughnessTexture { get; set; }
        public Texture2D NormalTexture { get; set; }
        public Texture2D AmbientOcclusionTexture { get; set; }
        public Effect Shader { get; set; }

        public Color BaseColor { get; set; } = Color.White;
        public Color EmissionColor { get; set; } = Color.Black;
        public float Alpha { get; set; } = 1;
        public bool VertexColors { get; set; } = false;
        public bool Transparent { get; set; } = false;

        public int SortOrder;
        public bool Lighting = true;
        public DepthStencilState DepthStencilState { get; set; } = null;
        public BlendState BlendState { get; set; } = null;
        public Dictionary<string, object> ShaderParams { get; set; } = new Dictionary<string, object>();
        public static Material Default { get; } = new Material();
        public float MetallicIntensity { get; set; } = 0.0f;
        public float RoughnessIntensity { get; set; } = .5f;
        public Material()
        {
        }

        public Material(Texture2D baseColorTexture = null,
                        Color baseColor = default,
                        Texture2D emissionColorTexture = null,
                        Color emissionColor = default,
                        float metallicIntensity  = 0f,
                        Texture2D metallicTexture = null,
                        float roughnessIntensity = 0.5f,
                        Texture2D roughnessTexture = null,
                        Texture2D normalTexture = null,
                        Texture2D ambientOcclusionTexture = null,
                        float alpha = 1,
                        bool lighting = true,
                        bool transparent = false, bool vertexColors = false, Effect shader = null)
        {
            BaseColorTexture = baseColorTexture;
            MetallicTexture = metallicTexture;
            RoughnessTexture = roughnessTexture;
            NormalTexture = normalTexture;
            AmbientOcclusionTexture = ambientOcclusionTexture;
            Shader = shader;
            BaseColor = baseColor == default ? Color.White : baseColor;
            Alpha = alpha;
            Transparent = transparent;
            VertexColors = vertexColors;
            EmissionColorTexture = emissionColorTexture;
            EmissionColor = emissionColor;
            MetallicIntensity = metallicIntensity;
            RoughnessIntensity = roughnessIntensity;
            Lighting = lighting;
        }

        public void ApplyEffectParameters(Effect effect, bool fallback)
        {
            if (effect == null) return;

            // Configure blend and depth states
            if (SortOrder == 0 || SortOrder == 1)
            {
                SortOrder = Transparent ? 1 : 0;
            }

            EngineManager.Instance.GraphicsDevice.DepthStencilState =
                DepthStencilState ?? DepthStencilState.Default;

            EngineManager.Instance.GraphicsDevice.BlendState =
                BlendState ?? (Transparent ? BlendState.AlphaBlend : BlendState.Opaque);

            // Apply effect parameters
            if (!fallback)
            {
                foreach (var kvp in ShaderParams)
                {
                    var paramValue = kvp.Value;
                    var paramType = paramValue.GetType();

                    if (paramType == typeof(Matrix))
                        effect.Parameters[kvp.Key]?.SetValue((Matrix)paramValue);
                    else if (paramType == typeof(Texture2D))
                        effect.Parameters[kvp.Key]?.SetValue((Texture2D)paramValue);
                    else if (paramType == typeof(TextureCube))
                        effect.Parameters[kvp.Key]?.SetValue((TextureCube)paramValue);
                    else if (paramType == typeof(Vector4))
                        effect.Parameters[kvp.Key]?.SetValue((Vector4)paramValue);
                    else if (paramType == typeof(Vector3))
                        effect.Parameters[kvp.Key]?.SetValue((Vector3)paramValue);
                    else if (paramType == typeof(Vector2))
                        effect.Parameters[kvp.Key]?.SetValue((Vector2)paramValue);
                    else if (paramType == typeof(float))
                        effect.Parameters[kvp.Key]?.SetValue((float)paramValue);
                    else if (paramType == typeof(int))
                        effect.Parameters[kvp.Key]?.SetValue((int)paramValue);
                    else if (paramType == typeof(bool))
                        effect.Parameters[kvp.Key]?.SetValue((bool)paramValue);
                    else if (paramType.IsArray)
                    {
                        var elementType = paramType.GetElementType();
                        if (elementType == typeof(float))
                            effect.Parameters[kvp.Key]?.SetValue((float[])paramValue);
                        else if (elementType == typeof(Vector3))
                            effect.Parameters[kvp.Key]?.SetValue((Vector3[])paramValue);
                    }
                }
            }
            else
            {
                // Fallback parameter setting
                effect.Parameters["VertexColors"]?.SetValue(VertexColors ? 1 : 0);
                effect.Parameters["Lighting"]?.SetValue(Lighting ? 1 : 0);
                effect.Parameters["Alpha"]?.SetValue(Alpha);
                effect.Parameters["MetallicIntensity"]?.SetValue(MetallicIntensity);
                effect.Parameters["RoughnessIntensity"]?.SetValue(RoughnessIntensity);

                effect.Parameters["BaseColorTexture"]?.SetValue(BaseColorTexture ?? EngineManager.Instance.WhiteTex);
                effect.Parameters["MetallicTexture"]?.SetValue(MetallicTexture ?? EngineManager.Instance.WhiteTex);
                effect.Parameters["RoughnessTexture"]?.SetValue(RoughnessTexture ?? EngineManager.Instance.WhiteTex);
                effect.Parameters["NormalTexture"]?.SetValue(NormalTexture ?? EngineManager.Instance.NormalTex);
                effect.Parameters["AmbientOcclusionTexture"]?.SetValue(AmbientOcclusionTexture ?? EngineManager.Instance.WhiteTex);
                effect.Parameters["EnvironmentMap"]?.SetValue(LightManager.Instance.EnvironmentMap ?? EngineManager.Instance.CubeTex );
                effect.Parameters["BaseColor"]?.SetValue(BaseColor.ToVector4());
                effect.Parameters["EmissionColor"]?.SetValue(EmissionColor.ToVector4());
                effect.Parameters["EmissionColorTexture"]?.SetValue(EmissionColorTexture ?? EngineManager.Instance.WhiteTex);
            }
        }
    }
}
