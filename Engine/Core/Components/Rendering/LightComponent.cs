using System;
using Microsoft.Xna.Framework;
using Engine.Core.Rendering;
using Engine.Core.EC;

namespace Engine.Core.Components.Rendering
{
    public enum LightType
    {
        Directional,
        Point,
        Spot
    }

    public class LightComponent : Component
    {
        public LightType LightType { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Intensity { get; set; } = 1.0f;
        public Vector3 Direction { get; private set; } = Vector3.Down;
        public float Range { get; set; } = 10.0f;
        public float SpotAngle { get; set; } = MathHelper.PiOver4; // Kept for backward compatibility
        public float InnerConeAngle { get; set; } = MathHelper.ToDegrees(MathHelper.PiOver4); // Inner cone in degrees (45° default)
        public float OuterConeAngle { get; set; } = MathHelper.ToDegrees(MathHelper.PiOver4 * 1.5f); // Outer cone in degrees (67.5° default)
        public Vector3 Position { get; private set; } = Vector3.Zero;

        public LightComponent()
        {
            LightManager.Instance.RegisterLight(this);
        }

        // Parameterized constructor for all light types
        public LightComponent(
            LightType lightType,
            Color color,
            float intensity,
            float range = 10.0f,
            float innerConeAngle = 45.0f, // Default inner cone angle in degrees
            float outerConeAngle = 67.5f  // Default outer cone angle in degrees
        )
        {
            LightType = lightType;
            Color = color;
            Intensity = intensity;
            Range = range;
            InnerConeAngle = innerConeAngle;
            OuterConeAngle = outerConeAngle;

            LightManager.Instance.RegisterLight(this);
        }

        public override void Start()
        {
            LightManager.Instance.RegisterLight(this);
        }

        public override void MainUpdate(GameTime gameTime)
        {
            Position = Transform.Position;
            Direction = Transform.Forward;
        }

        public override void OnDestroy()
        {
            LightManager.Instance.UnregisterLight(this);
        }

        // Helper method to get cosine of inner cone angle for shader
        public float GetInnerConeAngleCosine()
        {
            return (float)Math.Cos(MathHelper.ToRadians(InnerConeAngle));
        }

        // Helper method to get cosine of outer cone angle for shader
        public float GetOuterConeAngleCosine()
        {
            return (float)Math.Cos(MathHelper.ToRadians(OuterConeAngle));
        }
    }
}