using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Engine.Core.ECS;
namespace Engine.Core.Components
{
    public enum LightType
    {
        Directional
    }

    public class LightComponent : Component
    {
        public LightType LightType { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Intensity { get; set; } = 1.0f;
        public Vector3 Direction { get; set; } = Vector3.Down;
        public float Range { get; set; } = 10.0f;
        public float SpotAngle { get; set; } = MathHelper.PiOver4;

        public LightComponent()
        {

        }
        public LightComponent(LightType lightType, Color color, Vector3 direction, float intensity)
        {
            LightType = lightType;
            Color = color;
            Direction = direction;
            Intensity = intensity;
        }

        public override void Render(Effect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            if (effect is BasicEffect basicEffect)
            {
                basicEffect.LightingEnabled = true;

                // Check for directional lights
                if (LightType == LightType.Directional)
                {
                    // Check which directional light slot is available
                    for (int i = 0; i < 4; i++)
                    {
                        var lightSlot = GetDirectionalLightSlot(i, basicEffect);
                        if (lightSlot != null)
                        {
                            lightSlot.Enabled = true;
                            lightSlot.DiffuseColor = Color.ToVector3() * Intensity;
                            lightSlot.Direction = Direction;
                            break;
                        }
                    }
                }
            }
        }

        // Utility to get a specific directional light slot from BasicEffect
        private DirectionalLight GetDirectionalLightSlot(int slotIndex, BasicEffect effect)
        {
            switch (slotIndex)
            {
                case 0: return effect.DirectionalLight0;
                case 1: return effect.DirectionalLight1;
                case 2: return effect.DirectionalLight2;
                default: return null;
            }
        }
    }
}