using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Engine.Core.ECS;
using Engine.Core.Components;

namespace Engine.Core.Components.Rendering
{
    public class Camera : Component
    {
        public enum ProjectionType
        {
            Perspective,
            Orthographic
        }

        public float FieldOfView { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        private float AspectRatio { get; set; }

        public ProjectionType CurrentProjection { get; set; }

        public float OrthoWidth { get; set; }
        public float OrthoHeight { get; set; }

        public AudioListener Listener { get; private set; }

        public Camera()
        {
            FieldOfView = MathHelper.ToRadians(90);
            NearClip = 0.1f;
            FarClip = 1000f;
            OrthoWidth = 10f;
            OrthoHeight = 10f;
            CurrentProjection = ProjectionType.Perspective;

            Listener = new AudioListener();
        }

        public Matrix GetViewMatrix()
        {
            var transform = ECSManager.Instance.GetComponent<Transform>(EntityId);
            if (transform == null)
            {
                Listener.Position = Vector3.Zero;
                Listener.Up = Vector3.Up;
                Listener.Forward = Vector3.Forward;
                return Matrix.CreateLookAt(new Vector3(0, 0, 0), Vector3.Zero, Vector3.Up);
            }

            Matrix rotationMatrix = Matrix.CreateFromQuaternion(transform.Rotation);
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 up = Vector3.Transform(Vector3.Up, rotationMatrix);
            Vector3 target = transform.Position + forward;

            Listener.Position = transform.Position;
            Listener.Up = transform.Up;
            Listener.Forward = transform.Forward;

            return Matrix.CreateLookAt(transform.Position, target, up);
        }

        public Matrix GetProjectionMatrix()
        {
            if (CurrentProjection == ProjectionType.Perspective)
            {
                return Matrix.CreatePerspectiveFieldOfView(
                    FieldOfView,
                    AspectRatio,
                    NearClip,
                    FarClip
                );
            }
            else
            {
                return Matrix.CreateOrthographic(OrthoWidth, OrthoHeight, NearClip, FarClip);
            }
        }

        public void SwitchProjection()
        {
            CurrentProjection =
                CurrentProjection == ProjectionType.Perspective
                    ? ProjectionType.Orthographic
                    : ProjectionType.Perspective;
        }

        public override void Render(
            BasicEffect effect,
            Matrix viewMatrix,
            Matrix projectionMatrix,
            GameTime gameTime
        )
        {
            AspectRatio =
                (float)EngineManager.Instance.GraphicsDevice.Viewport.Width
                / EngineManager.Instance.GraphicsDevice.Viewport.Height;
            effect.View = GetViewMatrix();
            effect.Projection = GetProjectionMatrix();
        }
    }
}
