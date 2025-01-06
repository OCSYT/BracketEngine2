using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Engine.Core.ECS;
using Engine.Core.Physics;
using Engine.Core.Components;

namespace Engine.Core.Components.Physics
{
    public class RigidBody : Component
    {
        public BulletSharp.RigidBody BulletRigidBody { get; private set; }
        private DiscreteDynamicsWorld PhysicsWorld;
        public CollisionShape[] Shapes;
        private CompoundShape PhysicsShape;
        public bool IsStatic { get; set; }
        public bool Debug;
        private PhysicsDebugger Debugger;
        public float Mass { get; set; }
        public Vector3 Inertia { get; set; }
        public float Friction { get; set; }
        public float Restitution { get; set; }
        public int CollisionGroup { get; set; }
        public int CollisionMask { get; set; }

        private bool Initalized;

        public RigidBody(
            float mass = 70f,
            CollisionShape[] shapes = null,
            bool isStatic = false,
            bool debug = false,
            float friction = 0.5f,
            float restitution = 0.5f,
            int collisionGroup = 1,
            int collisionMask = 1
        )
        {
            PhysicsWorld = PhysicsManager.Instance.PhysicsWorld;
            Debug = debug;
            Mass = mass;
            Shapes = shapes;
            IsStatic = isStatic;
            Friction = friction;
            Restitution = restitution;
            CollisionGroup = collisionGroup;
            CollisionMask = collisionMask;
        }

        public void SetVelocityFactor(Vector3 freezePosition)
        {
            if (BulletRigidBody != null)
            {
                BulletRigidBody.LinearFactor = new BulletSharp.Math.Vector3(
                    freezePosition.X,
                    freezePosition.Y,
                    freezePosition.Z
                );
            }
        }

        public void SetAngularFactor(Vector3 freezeRotation)
        {
            if (BulletRigidBody != null)
            {
                BulletRigidBody.AngularFactor = new BulletSharp.Math.Vector3(
                    freezeRotation.X,
                    freezeRotation.Y,
                    freezeRotation.Z
                );
            }
        }

        public override void Start()
        {
            Debugger = new PhysicsDebugger();

            RebuildRigidBody();
        }

        public delegate void CollisionEventHandler(RigidBody self, RigidBody other);
        public event CollisionEventHandler OnCollisionEnter;
        public event CollisionEventHandler OnCollisionStay;
        public event CollisionEventHandler OnCollisionExit;

        private HashSet<RigidBody> currentlyColliding = new HashSet<RigidBody>();
        private HashSet<RigidBody> pendingEnterCollisions = new HashSet<RigidBody>();
        private HashSet<RigidBody> pendingExitCollisions = new HashSet<RigidBody>();

        private void CheckCollisions()
        {
            if (PhysicsWorld == null || BulletRigidBody == null)
                return;

            if (OnCollisionEnter == null && OnCollisionStay == null && OnCollisionExit == null)
                return;

            var dispatcher = PhysicsWorld.Dispatcher;

            if (dispatcher.NumManifolds == 0)
                return;

            pendingEnterCollisions.Clear();
            pendingExitCollisions.Clear();

            for (int i = 0; i < dispatcher.NumManifolds; i++)
            {
                var manifold = dispatcher.GetManifoldByIndexInternal(i);

                if (manifold.Body0 == BulletRigidBody || manifold.Body1 == BulletRigidBody)
                {
                    var otherBody =
                        manifold.Body0 == BulletRigidBody ? manifold.Body1 : manifold.Body0;

                    if (otherBody.UserObject is RigidBody otherRigidBody)
                    {
                        if (currentlyColliding.Add(otherRigidBody))
                            pendingEnterCollisions.Add(otherRigidBody);
                    }
                }
            }

            currentlyColliding.RemoveWhere(
                body =>
                {
                    bool isStillColliding = false;
                    for (int i = 0; i < dispatcher.NumManifolds; i++)
                    {
                        var manifold = dispatcher.GetManifoldByIndexInternal(i);
                        if (manifold.Body0 == BulletRigidBody || manifold.Body1 == BulletRigidBody)
                        {
                            var otherBody =
                                manifold.Body0 == BulletRigidBody ? manifold.Body1 : manifold.Body0;
                            if (otherBody.UserObject == body)
                            {
                                isStillColliding = true;
                                break;
                            }
                        }
                    }
                    if (!isStillColliding)
                    {
                        pendingExitCollisions.Add(body);
                        return true;
                    }
                    return false;
                }
            );

            foreach (var body in pendingEnterCollisions)
                OnCollisionEnter?.Invoke(this, body);

            foreach (var body in currentlyColliding.Except(pendingEnterCollisions))
                OnCollisionStay?.Invoke(this, body);

            foreach (var body in pendingExitCollisions)
                OnCollisionExit?.Invoke(this, body);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            if (BulletRigidBody != null)
            {
                UpdateTransform();
                CheckCollisions();
            }
        }

        public void RebuildRigidBody()
        {
            if (BulletRigidBody != null)
            {
                RemoveFromWorld();
            }

            BulletSharp.Math.Matrix initialTransform = BulletSharp.Math.Matrix.AffineTransformation(
                1.0f,
                new BulletSharp.Math.Quaternion(
                    Transform.Rotation.X,
                    Transform.Rotation.Y,
                    Transform.Rotation.Z,
                    Transform.Rotation.W
                ),
                new BulletSharp.Math.Vector3(
                    Transform.Position.X,
                    Transform.Position.Y,
                    Transform.Position.Z
                )
            );
            BulletSharp.Math.Vector3 calculatedInertia = BulletSharp.Math.Vector3.Zero;

            PhysicsShape = new CompoundShape();

            foreach (CollisionShape shape in Shapes)
            {
                shape.LocalScaling = new BulletSharp.Math.Vector3(
                    Transform.Scale.X,
                    Transform.Scale.Y,
                    Transform.Scale.Z
                );

                if (!IsStatic)
                {
                    calculatedInertia += shape.CalculateLocalInertia(Mass);
                }
                PhysicsShape.AddChildShape(BulletSharp.Math.Matrix.Identity, shape);
            }

            RigidBodyConstructionInfo rigidBodyInfo = new RigidBodyConstructionInfo(
                IsStatic ? 0 : Mass,
                new DefaultMotionState(initialTransform),
                PhysicsShape,
                new BulletSharp.Math.Vector3(Inertia.X, Inertia.Y,  Inertia.Z)
                    == BulletSharp.Math.Vector3.Zero
                  ? calculatedInertia
                  : new BulletSharp.Math.Vector3(Inertia.X, Inertia.Y, Inertia.Z)
            );

            BulletRigidBody = new BulletSharp.RigidBody(rigidBodyInfo)
            {
                RollingFriction = Friction,
                SpinningFriction = Friction,
                Friction = Friction,
                Restitution = Restitution
            };
            BulletRigidBody.SetAnisotropicFriction(BulletSharp.Math.Vector3.One *  Friction, AnisotropicFrictionFlags.RollingFriction);

            BulletRigidBody.UserObject = this;
            BulletRigidBody.SetSleepingThresholds(0, 0);
            Initalized = false;
            PhysicsWorld.AddRigidBody(BulletRigidBody, CollisionGroup, CollisionMask);
            Initalized = true;
        }

        public void SetFriction(float f)
        {
            if (BulletRigidBody == null || PhysicsShape == null) return;

            BulletRigidBody.RollingFriction = f;
            BulletRigidBody.SpinningFriction = f;
            BulletRigidBody.Friction = f;
            BulletRigidBody.SetAnisotropicFriction(BulletSharp.Math.Vector3.One * Friction, AnisotropicFrictionFlags.RollingFriction);

            Friction = f;
        }

        public void SetIntertia(BulletSharp.Math.Vector3 i)
        {
            if (BulletRigidBody == null) return;
            BulletRigidBody.SetMassProps(Mass, i);
            Inertia = new Vector3(i.X, i.Y, i.Z);
        }

        public void SetMass(float m)
        {
            if (BulletRigidBody == null) return;
            BulletRigidBody.SetMassProps(m, BulletRigidBody.LocalInertia);
            Mass = m;
        }

        public void SetRestitution(float f)
        {
            if (BulletRigidBody == null) return;
            BulletRigidBody.Restitution = f;
            Restitution = f;
        }

        public void SetVelocity(BulletSharp.Math.Vector3 v)
        {
            if (BulletRigidBody == null) return;
            BulletRigidBody.LinearVelocity = v;
        }

        public void SetAngularVelocity(BulletSharp.Math.Vector3 av)
        {
            if (BulletRigidBody == null) return;
            BulletRigidBody.AngularVelocity = av;
        }



        private void UpdateTransform()
        {
            BulletSharp.Math.Matrix worldTransform = BulletRigidBody.WorldTransform;
            Transform.Position = new Vector3(
                worldTransform.M41,
                worldTransform.M42,
                worldTransform.M43
            );
            Transform.Rotation = BulletToQuaternion(worldTransform);
        }

        private void RemoveFromWorld()
        {
            if (PhysicsWorld != null && BulletRigidBody != null)
            {
                PhysicsWorld.RemoveRigidBody(BulletRigidBody);
                BulletRigidBody.Dispose();
                BulletRigidBody = null;
            }
        }

        public void ApplyForce(Vector3 force)
        {
            if (Initalized && BulletRigidBody != null)
            {
                BulletRigidBody.ApplyCentralForce(
                    new BulletSharp.Math.Vector3(force.X, force.Y, force.Z)
                );
            }
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            if (Initalized && BulletRigidBody != null)
            {
                BulletRigidBody.ApplyCentralImpulse(
                    new BulletSharp.Math.Vector3(impulse.X, impulse.Y, impulse.Z)
                );
            }
        }

        public void ApplyTorque(Vector3 torque)
        {
            if (Initalized && BulletRigidBody != null)
            {
                BulletRigidBody.ApplyTorque(
                    new BulletSharp.Math.Vector3(torque.X, torque.Y, torque.Z)
                );
            }
        }

        public override void Render(
            BasicEffect effect,
            Matrix viewMatrix,
            Matrix projectionMatrix,
            GameTime gameTime
        )
        {
            if (Debug && BulletRigidBody != null && Debugger != null)
            {
                DrawDebugShapes(viewMatrix, projectionMatrix);
            }
        }

        private void DrawDebugShapes(Matrix viewMatrix, Matrix projectionMatrix)
        {
            Matrix worldMatrixNormalized = Transform.GetWorldMatrixNormalized();
            foreach (CollisionShape Shape in Shapes)
            {
                Debugger.DrawCollisionShape(
                    Shape,
                    Transform,
                    worldMatrixNormalized,
                    viewMatrix,
                    projectionMatrix,
                    BulletRigidBody.IsStaticOrKinematicObject
                );
            }
        }

        public override void OnDestroy()
        {
            OnCollisionEnter = null;
            OnCollisionStay = null;
            OnCollisionExit = null;
            currentlyColliding.Clear();
            RemoveFromWorld();
            if (Shapes != null)
            {
                foreach (var shape in Shapes)
                {
                    shape?.Dispose();
                }
                Shapes = null;
            }
            Debugger = null;
        }

        private static Quaternion BulletToQuaternion(BulletSharp.Math.Matrix bulletMatrix)
        {
            Matrix matrix = new Matrix(
                bulletMatrix.M11,
                bulletMatrix.M12,
                bulletMatrix.M13,
                0,
                bulletMatrix.M21,
                bulletMatrix.M22,
                bulletMatrix.M23,
                0,
                bulletMatrix.M31,
                bulletMatrix.M32,
                bulletMatrix.M33,
                0,
                0,
                0,
                0,
                1
            );
            return Quaternion.CreateFromRotationMatrix(matrix);
        }
    }
}
