using Microsoft.Xna.Framework;
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
        public Vector3 Direction { get; private set; } = Vector3.Down;
        public float Range { get; set; } = 10.0f;
        public float SpotAngle { get; set; } = MathHelper.PiOver4;
        public Vector3 Position { get; private set; } = Vector3.Zero;


        public LightComponent()
        {

        }

        public override void Start()
        {
            LightManager.Instance.RegisterLight(this);
        }

        // Parameterized constructor without position or direction
        public LightComponent(
            LightType lightType,
            Color color,
            float intensity,
            float range = 10.0f
        )
        {
            LightType = lightType;
            Color = color;
            Intensity = intensity;
            Range = range;

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
    }
}
