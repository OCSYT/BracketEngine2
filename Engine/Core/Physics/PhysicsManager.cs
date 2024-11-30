using BulletSharp;
using BulletSharp.Math;
using Engine.Core.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Core.Physics
{
    public class PhysicsManager
    {
        private static readonly Lazy<PhysicsManager> _instance = new Lazy<PhysicsManager>(
            () => new PhysicsManager()
        );

        public DiscreteDynamicsWorld PhysicsWorld { get; private set; }

        public static PhysicsManager Instance => _instance.Value;

        public Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);

        private PhysicsManager()
        {
            var collisionConfiguration = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfiguration);
            var broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            PhysicsWorld = new DiscreteDynamicsWorld(
                dispatcher,
                broadphase,
                solver,
                collisionConfiguration
            );
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
            public CollisionObject HitObject { get; set; }

            public HitResult(
                bool hasHit,
                Microsoft.Xna.Framework.Vector3 hitPoint,
                Microsoft.Xna.Framework.Vector3 hitNormal,
                CollisionObject hitObject
            )
            {
                HasHit = hasHit;
                HitPoint = hitPoint;
                HitNormal = hitNormal;
                HitObject = hitObject;
            }
        }

        public HitResult Raycast(
            Microsoft.Xna.Framework.Vector3 rayFrom,
            Microsoft.Xna.Framework.Vector3 rayTo,
            int collisonGroup,
            int collisionMask
        )
        {
            Vector3 rayFromBullet = new Vector3(rayFrom.X, rayFrom.Y, rayFrom.Z);
            Vector3 rayToBullet = new Vector3(rayTo.X, rayTo.Y, rayTo.Z);
            ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(
                ref rayFromBullet,
                ref rayToBullet
            );
            rayCallback.CollisionFilterGroup = collisonGroup;
            rayCallback.CollisionFilterMask = collisionMask;

            PhysicsWorld.RayTest(rayFromBullet, rayToBullet, rayCallback);

            if (rayCallback.HasHit)
            {
                var hitPoint = new Microsoft.Xna.Framework.Vector3(
                    rayCallback.HitPointWorld.X,
                    rayCallback.HitPointWorld.Y,
                    rayCallback.HitPointWorld.Z
                );

                var hitNormal = new Microsoft.Xna.Framework.Vector3(
                    rayCallback.HitNormalWorld.X,
                    rayCallback.HitNormalWorld.Y,
                    rayCallback.HitNormalWorld.Z
                );
                return new HitResult(true, hitPoint, hitNormal, rayCallback.CollisionObject);
            }

            return new HitResult(
                false,
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Zero,
                null
            );
        }
        public static int CreateCollisionMask(int[] collisionGroups, bool include = true)
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

        public static CollisionShape CreateCollisionShapeFromStaticMesh(
            StaticMesh staticMesh,
            Microsoft.Xna.Framework.Vector3? scale = null,
            bool isConvex = true
        )
        {
            Microsoft.Xna.Framework.Vector3 scalingFactor =
                scale ?? Microsoft.Xna.Framework.Vector3.One;

            List<Vector3> vertices = new List<Vector3>();

            foreach (StaticMesh.SubMesh subMesh in staticMesh.SubMeshes)
            {
                VertexPosition[] submeshVertices = new VertexPosition[subMesh.NumVertices];

                subMesh.VertexBuffer.GetData(submeshVertices);

                foreach (var vertex in submeshVertices)
                {
                    vertices.Add(
                        new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z)
                    );
                }
            }

            if (isConvex)
            {
                ConvexHullShape convexShape = new ConvexHullShape();
                foreach (var vertex in vertices)
                {
                    var scaledVertex = new Vector3(
                        vertex.X * scalingFactor.X,
                        vertex.Y * scalingFactor.Y,
                        vertex.Z * scalingFactor.Z
                    );
                    convexShape.AddPoint(scaledVertex, false);
                }
                convexShape.RecalcLocalAabb();
                return convexShape;
            }
            else
            {
                TriangleMesh triangleMesh = new TriangleMesh();
                List<int> indices = new List<int>();

                foreach (StaticMesh.SubMesh subMesh in staticMesh.SubMeshes)
                {
                    int[] submeshIndices = new int[subMesh.NumIndices];
                    subMesh.IndexBuffer.GetData(submeshIndices);

                    foreach (var index in submeshIndices)
                    {
                        indices.Add(index);
                    }
                }


                for (int i = 0; i < indices.Count; i += 3)
                {
                    var v0 = vertices[indices[i]];
                    var v1 = vertices[indices[i + 1]];
                    var v2 = vertices[indices[i + 2]];

                    var scaledV0 = new Vector3(
                        v0.X * scalingFactor.X,
                        v0.Y * scalingFactor.Y,
                        v0.Z * scalingFactor.Z
                    );
                    var scaledV1 = new Vector3(
                        v1.X * scalingFactor.X,
                        v1.Y * scalingFactor.Y,
                        v1.Z * scalingFactor.Z
                    );
                    var scaledV2 = new Vector3(
                        v2.X * scalingFactor.X,
                        v2.Y * scalingFactor.Y,
                        v2.Z * scalingFactor.Z
                    );

                    triangleMesh.AddTriangle(scaledV0, scaledV1, scaledV2, true);
                }

                var shape = new BvhTriangleMeshShape(triangleMesh, true);

                return shape;
            }
        }

        public static CollisionShape CreateCollisionShapeFromModel(
            Model model,
            Microsoft.Xna.Framework.Vector3? scale = null,
            bool isConvex = true
        )
        {
            Microsoft.Xna.Framework.Vector3 scalingFactor =
                scale ?? Microsoft.Xna.Framework.Vector3.One;

            List<Vector3> vertices = ExtractVerticesFromModel(model);

            if (isConvex)
            {
                ConvexHullShape convexShape = new ConvexHullShape();
                foreach (var vertex in vertices)
                {
                    var scaledVertex = new Vector3(
                        vertex.X * scalingFactor.X,
                        vertex.Y * scalingFactor.Y,
                        vertex.Z * scalingFactor.Z
                    );
                    convexShape.AddPoint(scaledVertex, false);
                }
                convexShape.RecalcLocalAabb();
                return convexShape;
            }
            else
            {
                TriangleMesh triangleMesh = new TriangleMesh();
                List<int> indices = ExtractIndicesFromModel(model);

                for (int i = 0; i < indices.Count; i += 3)
                {
                    var v0 = vertices[indices[i]];
                    var v1 = vertices[indices[i + 1]];
                    var v2 = vertices[indices[i + 2]];

                    var scaledV0 = new Vector3(
                        v0.X * scalingFactor.X,
                        v0.Y * scalingFactor.Y,
                        v0.Z * scalingFactor.Z
                    );
                    var scaledV1 = new Vector3(
                        v1.X * scalingFactor.X,
                        v1.Y * scalingFactor.Y,
                        v1.Z * scalingFactor.Z
                    );
                    var scaledV2 = new Vector3(
                        v2.X * scalingFactor.X,
                        v2.Y * scalingFactor.Y,
                        v2.Z * scalingFactor.Z
                    );

                    triangleMesh.AddTriangle(scaledV0, scaledV1, scaledV2, true);
                }

                var shape = new BvhTriangleMeshShape(triangleMesh, true);

                return shape;
            }
        }

        private static List<Vector3> ExtractVerticesFromModel(Model model)
        {
            List<Vector3> vertices = new List<Vector3>();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[
                        part.NumVertices
                    ];
                    part.VertexBuffer.GetData(vertexData);

                    foreach (var vertex in vertexData)
                    {
                        vertices.Add(
                            new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z)
                        );
                    }
                }
            }
            return vertices;
        }

        private static List<int> ExtractIndicesFromModel(Model model)
        {
            List<int> indices = new List<int>();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    ushort[] indexData = new ushort[part.PrimitiveCount * 3];
                    part.IndexBuffer.GetData(indexData);

                    indices.AddRange(Array.ConvertAll(indexData, x => (int)x));
                }
            }
            return indices;
        }
    }
}
