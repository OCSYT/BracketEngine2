using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Core.Rendering;
using Engine.Core.ECS;

namespace Engine.Core.Components.Rendering
{
    public enum LightType
    {
        Directional,
        Point
    }

    public class LightComponent : Component
    {
        public LightType LightType { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Intensity { get; set; } = 1.0f;
        public Vector3 Direction { get; set; } = Vector3.Down;
        public float Range { get; set; } = 10.0f;
        public float SpotAngle { get; set; } = MathHelper.PiOver4;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public LightComponent()
        {
            LightManager.Instance.RegisterLight(this);
        }

        public LightComponent(
            LightType lightType,
            Color color,
            Vector3 positionOrDirection,
            float intensity,
            float range = 10.0f
        )
        {
            LightType = lightType;
            Color = color;
            Intensity = intensity;

            if (lightType == LightType.Directional)
            {
                Direction = positionOrDirection;
            }
            else if (lightType == LightType.Point)
            {
                Position = positionOrDirection;
                Range = range;
            }

            LightManager.Instance.RegisterLight(this);
        }

        public override void OnDestroy()
        {
            LightManager.Instance.UnregisterLight(this);
        }
    }
}
