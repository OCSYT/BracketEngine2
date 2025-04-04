using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Core.EC;
using Engine.Core.Physics;
using Engine.Core;
using Engine.Core.Components;
using Engine.Core.Components.Physics;
using Engine.Core.Components.Rendering;
using static Engine.Core.Physics.PhysicsManager;

namespace Engine.Components
{
    public class PlayerController : Component
    {
        public RigidBody Body;
        public Camera CameraObj;
        public float Sensitivity = 1;
        private float MouseX = 0;
        private float MouseY = 0;
        private Transform CamTransform;
        private Vector3 AirVelocity = Vector3.Zero;
        public float Speed = 50;
        public float Jump = 5;
        private Vector3 ForwardDir = Vector3.Forward;
        private Vector3 RightDir = Vector3.Right;
        public float MaxVelocity = 50;
        public float YVel = 0;
        public float Height = 5;
        public int CollisionMask;
        public int CollisionGroup;
        public PlayerController()
        {

        }
        public PlayerController(RigidBody Body, Camera CameraObj, float Sensitivity = 1, float Speed = 50, float Jump = 5, float MaxVelocity = 100, float Height = 5, int CollisionMask = 1, int CollisionGroup = 1)
        {
            this.Body = Body;
            this.CameraObj = CameraObj;
            this.Sensitivity = Sensitivity;
            this.Speed = Speed;
            this.Jump = Jump;
            this.MaxVelocity = MaxVelocity;
            this.Height = Height;
            this.CollisionGroup = CollisionGroup;
            this.CollisionMask = CollisionMask;
        }

        public override void Start()
        {
            CamTransform = CameraObj.Transform;
            Body.SetGravity(Vector3.Zero);
            Body.SetFriction(0f);
            Body.SetRestitution(0f);
            Body.SetAngularFactor(Vector3.Zero);
        }

        public override void MainUpdate(GameTime gameTime)
        {
            if (!EngineManager.Instance.IsActive) return;

            GraphicsDeviceManager Graphics = EngineManager.Instance.Graphics;

            Vector2 CurrentMousePosition = Mouse.GetState().Position.ToVector2();
            Vector2 MouseDelta = CurrentMousePosition - new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height / 2);

            MouseX -= MouseDelta.X * (Sensitivity / 10) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MouseY = MathHelper.Clamp(MouseY - MouseDelta.Y * (Sensitivity / 10) * (float)gameTime.ElapsedGameTime.TotalSeconds, MathHelper.ToRadians(-90), MathHelper.ToRadians(90));

            CamTransform.Rotation = Quaternion.CreateFromYawPitchRoll(MouseX, MouseY, 0);
            Quaternion BodyDir = Quaternion.CreateFromYawPitchRoll(MouseX, 0, 0);
            ForwardDir = Vector3.Transform(Vector3.Forward, BodyDir);
            RightDir = Vector3.Transform(Vector3.Right, BodyDir);
        }

        public override void FixedUpdate(GameTime GameTime)
        {

            PhysicsManager.HitResult HitResult =
                PhysicsManager.Instance.Raycast(Transform.Position,
                Transform.Position + Vector3.Down * (Height), CollisionGroup, CollisionMask);

            KeyboardState State = Keyboard.GetState();

            Vector3 LocalVel = Vector3.Zero;
            if (EngineManager.Instance.IsActive)
            {
                EngineManager.Instance.LockMouse();
                if (State.IsKeyDown(Keys.W))
                {
                    LocalVel += ForwardDir * Speed;
                }
                if (State.IsKeyDown(Keys.S))
                {
                    LocalVel -= ForwardDir * Speed;
                }
                if (State.IsKeyDown(Keys.A))
                {
                    LocalVel -= RightDir * Speed;
                }
                if (State.IsKeyDown(Keys.D))
                {
                    LocalVel += RightDir * Speed;
                }
            }

            if (HitResult.HasHit)
            {
                AirVelocity = LocalVel;
            }
            else
            {
                LocalVel = AirVelocity + LocalVel / 2;
                AirVelocity *= .995f;
            }

            // Clamp velocity to MaxVelocity.
            if (LocalVel.Length() > MaxVelocity)
            {
                LocalVel.Normalize();
                LocalVel *= MaxVelocity;
            }

        
            if (HitResult.HasHit)
            {
                Vector3 NewPos = new Vector3(Transform.Position.X, HitResult.HitPoint.Y + Height, Transform.Position.Z);
                Transform.Position = (NewPos);
                YVel = 0;
                if (State.IsKeyDown(Keys.Space))
                {
                    YVel += 10 * Jump;
                }
            }
            else
            {
                YVel += PhysicsManager.Instance.Gravity.Y/20* Jump;
            }

            LocalVel += Vector3.Up * YVel;
            Body.SetVelocity(LocalVel);
            
            CamTransform.Position = Transform.Position + (LocalVel * (float)GameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
