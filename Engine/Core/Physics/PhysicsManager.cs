﻿using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;

namespace Engine.Core.Physics
{
    public sealed class PhysicsManager
    {
        private static readonly Lazy<PhysicsManager> _instance = new Lazy<PhysicsManager>(() => new PhysicsManager());

        public DiscreteDynamicsWorld PhysicsWorld { get; private set; }

        public static PhysicsManager Instance => _instance.Value;


        public Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);


        private PhysicsManager()
        {
            var collisionConfiguration = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfiguration);
            var broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            PhysicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfiguration);
            Vector3 Gravity = new Vector3(0, -9.81f, 0);
            PhysicsWorld.SetGravity(ref Gravity);
        }

        private void StepPhysics(float FixedTimeStep)
        {
            PhysicsWorld.StepSimulation(FixedTimeStep);
        }

        public void PhysicsUpdate(float FixedTimeStep)
        {
            if (PhysicsWorld != null)
            {
                Vector3 Gravity = Instance.Gravity;
                PhysicsWorld.SetGravity(ref Gravity);
            }
            StepPhysics(FixedTimeStep);
        }


        public struct HitResult
        {
            public bool HasHit { get; set; }
            public Microsoft.Xna.Framework.Vector3 HitPoint { get; set; }
            public Microsoft.Xna.Framework.Vector3 HitNormal { get; set; }

            public HitResult(bool hasHit, Microsoft.Xna.Framework.Vector3 hitPoint, Microsoft.Xna.Framework.Vector3 hitNormal)
            {
                HasHit = hasHit;
                HitPoint = hitPoint;
                HitNormal = hitNormal;
            }
        }

        public HitResult Raycast(Microsoft.Xna.Framework.Vector3 rayFrom, Microsoft.Xna.Framework.Vector3 rayTo, int collisonGroup, int collisionMask)
        {
            Vector3 rayFromBullet = new Vector3(rayFrom.X, rayFrom.Y, rayFrom.Z);
            Vector3 rayToBullet = new Vector3(rayTo.X, rayTo.Y, rayTo.Z);
            ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(ref rayFromBullet, ref rayToBullet);
            rayCallback.CollisionFilterGroup = collisonGroup;
            rayCallback.CollisionFilterMask = collisionMask;

            PhysicsWorld.RayTest(rayFromBullet, rayToBullet, rayCallback);

            if (rayCallback.HasHit)
            {
                var hitPoint = new Microsoft.Xna.Framework.Vector3(
                    rayCallback.HitPointWorld.X,
                    rayCallback.HitPointWorld.Y,
                    rayCallback.HitPointWorld.Z);

                var hitNormal = new Microsoft.Xna.Framework.Vector3(
                    rayCallback.HitNormalWorld.X,
                    rayCallback.HitNormalWorld.Y,
                    rayCallback.HitNormalWorld.Z);

                return new HitResult(true, hitPoint, hitNormal);
            }

            return new HitResult(false, Microsoft.Xna.Framework.Vector3.Zero, Microsoft.Xna.Framework.Vector3.Zero);
        }
        public int CreateCollisionMask(int[] collisionGroups, bool include)
        {
            int mask = 0;

            foreach (int group in collisionGroups)
            {
                if (include)
                {
                    mask |= 1 << group;
                }
                else
                {
                    mask &= ~(1 << group);
                }
            }

            return mask;
        }

    }
}