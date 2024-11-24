﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Core.ECS;
using Engine.Core.Physics;
using Engine.Core.Components;
using Engine.Core.Components.Physics;
using Engine.Core;

namespace Engine.Components
{
    public class PlayerController : Component
    {
        private RigidBody Body;
        private Camera CameraObj;
        public float Sensitivity = 1;
        private float MouseX = 0;
        private float MouseY = 0;
        private Transform LocalTransform;
        private Transform CamTransform;
        private Vector3 AirVelocity = Vector3.Zero;
        public float Speed = 50;
        public float Jump = 5;
        private Vector3 ForwardDir = Vector3.Forward;
        private Vector3 RightDir = Vector3.Right;
        public float MaxVelocity = 50;

        public PlayerController(RigidBody Body, Camera CameraObj, float Sensitivity = 1, float Speed = 50, float Jump = 5, float MaxVelocity = 100)
        {
            this.Body = Body;
            this.CameraObj = CameraObj;
            this.Sensitivity = Sensitivity;
            this.Speed = Speed;
            this.Jump = Jump;
            this.MaxVelocity = MaxVelocity; // Initialize MaxVelocity.
        }

        public override void Start()
        {
            CamTransform = ECSManager.Instance.GetComponent<Transform>(CameraObj.EntityId);
            LocalTransform = ECSManager.Instance.GetComponent<Transform>(EntityId);
            Body.BulletRigidBody.Friction = 0f;
            Body.BulletRigidBody.Restitution = 0f;
            Body.SetAngularFactor(Vector3.Zero);
        }

        public override void MainUpdate(GameTime gameTime)
        {
            if (!EngineManager.Instance.IsActive) return;

            GraphicsDeviceManager Graphics = EngineManager.Instance.Graphics;

            Vector2 CurrentMousePosition = Mouse.GetState().Position.ToVector2();
            Vector2 MouseDelta = CurrentMousePosition - new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height / 2);

            MouseX -= MouseDelta.X * (Sensitivity / 100000);
            MouseY = MathHelper.Clamp(MouseY - MouseDelta.Y * (Sensitivity / 100000), MathHelper.ToRadians(-90), MathHelper.ToRadians(90));

            CamTransform.Rotation = Quaternion.CreateFromYawPitchRoll(MouseX, MouseY, 0);
            Quaternion BodyDir = Quaternion.CreateFromYawPitchRoll(MouseX, 0, 0);
            ForwardDir = Vector3.Transform(Vector3.Forward, BodyDir);
            RightDir = Vector3.Transform(Vector3.Right, BodyDir);
        }

        public override void FixedUpdate(GameTime gameTime)
        {

            EngineManager.Instance.LockMouse();

            PhysicsManager.HitResult HitResult =
                PhysicsManager.Instance.Raycast(LocalTransform.Position,
                LocalTransform.Position + Vector3.Down * 5, PhysicsManager.Instance.CreateCollisionMask([2], true),
                PhysicsManager.Instance.CreateCollisionMask([1], true));

            KeyboardState State = Keyboard.GetState();

            Vector3 LocalVel = Vector3.Zero;
            if (EngineManager.Instance.IsActive)
            {
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

            LocalVel += Vector3.Up * Body.BulletRigidBody.LinearVelocity.Y;
            Body.BulletRigidBody.LinearVelocity =
                new BulletSharp.Math.Vector3(LocalVel.X, LocalVel.Y, LocalVel.Z);

            if (HitResult.HasHit)
            {
                if (State.IsKeyDown(Keys.Space))
                {
                    Body.ApplyImpulse(Vector3.Up * -PhysicsManager.Instance.Gravity.Y / 2 * 100);
                }
            }
            Body.ApplyImpulse(-Vector3.Up * -PhysicsManager.Instance.Gravity.Y / 2 * 100 / Jump);

            CamTransform.Position = LocalTransform.Position + Vector3.Up * 2;
        }
    }
}