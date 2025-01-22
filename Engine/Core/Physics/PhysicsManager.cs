using BulletSharp;
using BulletSharp.Math;
using Engine.Core.Components;
using Engine.Core.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.Core.Physics
{
    public class PhysicsManager
    {
        private static readonly Lazy<PhysicsManager> _Instance = new Lazy<PhysicsManager>(
            () => new PhysicsManager()
        );

        public DiscreteDynamicsWorld PhysicsWorld { get; private set; }

        public static PhysicsManager Instance => _Instance.Value;

        public Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);

        private PhysicsManager()
        {
            var CollisionConfiguration = new DefaultCollisionConfiguration();
            var Dispatcher = new CollisionDispatcher(CollisionConfiguration);
            var Broadphase = new DbvtBroadphase();
            var Solver = new SequentialImpulseConstraintSolver();
            PhysicsWorld = new DiscreteDynamicsWorld(
                Dispatcher,
                Broadphase,
                Solver,
                CollisionConfiguration
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
                bool HasHit,
                Microsoft.Xna.Framework.Vector3 HitPoint,
                Microsoft.Xna.Framework.Vector3 HitNormal,
                CollisionObject HitObject
            )
            {
                this.HasHit = HasHit;
                this.HitPoint = HitPoint;
                this.HitNormal = HitNormal;
                this.HitObject = HitObject;
            }
        }

        public HitResult Raycast(
            Microsoft.Xna.Framework.Vector3 RayFrom,
            Microsoft.Xna.Framework.Vector3 RayTo,
            int CollisonGroup,
            int CollisionMask
        )
        {
            Vector3 RayFromBullet = new Vector3(RayFrom.X, RayFrom.Y, RayFrom.Z);
            Vector3 RayToBullet = new Vector3(RayTo.X, RayTo.Y, RayTo.Z);
            ClosestRayResultCallback RayCallback = new ClosestRayResultCallback(
                ref RayFromBullet,
                ref RayToBullet
            );
            RayCallback.CollisionFilterGroup = CollisonGroup;
            RayCallback.CollisionFilterMask = CollisionMask;

            PhysicsWorld.RayTest(RayFromBullet, RayToBullet, RayCallback);

            if (RayCallback.HasHit)
            {
                var HitPoint = new Microsoft.Xna.Framework.Vector3(
                    RayCallback.HitPointWorld.X,
                    RayCallback.HitPointWorld.Y,
                    RayCallback.HitPointWorld.Z
                );

                var HitNormal = new Microsoft.Xna.Framework.Vector3(
                    RayCallback.HitNormalWorld.X,
                    RayCallback.HitNormalWorld.Y,
                    RayCallback.HitNormalWorld.Z
                );
                return new HitResult(true, HitPoint, HitNormal, RayCallback.CollisionObject);
            }

            return new HitResult(
                false,
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Zero,
                null
            );
        }

        public static int CreateCollisionMask(int[] CollisionGroups, bool Include = true)
        {
            int Mask = 0;

            foreach (int Group in CollisionGroups)
            {
                if (Include)
                {
                    Mask |= 1 << Group;
                }
                else
                {
                    Mask &= ~(1 << Group);
                }
            }

            return Mask;
        }

        public static CollisionShape CreateCollisionShapeFromStaticMesh(
            StaticMesh StaticMesh, Transform Transform,
            Microsoft.Xna.Framework.Vector3? Scale = null,
            bool Convex = true
        )
        {
            List<Vector3> Vertices = ExtractVerticesFromStaticMesh(StaticMesh);
            List<int> Indices = ExtractIndicesFromStaticMesh(StaticMesh);

            Microsoft.Xna.Framework.Vector3 ScalingFactor = Scale * Transform.Scale ?? Microsoft.Xna.Framework.Vector3.One * Transform.Scale;

            if (Convex)
            {
                ConvexHullShape ConvexShape = new ConvexHullShape();
                foreach (var Vertex in Vertices)
                {
                    var ScaledVertex = new Vector3(
                        Vertex.X * ScalingFactor.X / Transform.Scale.X,
                        Vertex.Y * ScalingFactor.Y / Transform.Scale.Y,
                        Vertex.Z * ScalingFactor.Z / Transform.Scale.Z
                    );
                    ConvexShape.AddPoint(ScaledVertex, false);
                }
                ConvexShape.RecalcLocalAabb();
                return ConvexShape;
            }
            else
            {
                TriangleMesh TriangleMesh = new TriangleMesh();

                Task ProcessMesh = Task.Run(() =>
                {
                    for (int I = 0; I < Indices.Count; I += 3)
                    {
                        Vector3 ScaledVertex1 = new Vector3(
                            Vertices[Indices[I]].X * ScalingFactor.X,
                            Vertices[Indices[I]].Y * ScalingFactor.Y,
                            Vertices[Indices[I]].Z * ScalingFactor.Z
                        );
                        Vector3 ScaledVertex2 = new Vector3(
                            Vertices[Indices[I + 1]].X * ScalingFactor.X,
                            Vertices[Indices[I + 1]].Y * ScalingFactor.Y,
                            Vertices[Indices[I + 1]].Z * ScalingFactor.Z
                        );
                        Vector3 ScaledVertex3 = new Vector3(
                            Vertices[Indices[I + 2]].X * ScalingFactor.X,
                            Vertices[Indices[I + 2]].Y * ScalingFactor.Y,
                            Vertices[Indices[I + 2]].Z * ScalingFactor.Z
                        );

                        TriangleMesh.AddTriangle(ScaledVertex1, ScaledVertex2, ScaledVertex3, true);
                    }
                });
                ProcessMesh.Wait();

                GImpactMeshShape GImpactShape = new GImpactMeshShape(TriangleMesh);
                GImpactShape.UpdateBound();

                return GImpactShape;
            }
        }

        private static List<Vector3> ExtractVerticesFromStaticMesh(StaticMesh StaticMesh)
        {
            List<Vector3> Vertices = new List<Vector3>();

            foreach (StaticMesh.SubMesh SubMesh in StaticMesh.SubMeshes)
            {
                VertexPosition[] SubmeshVertices = new VertexPosition[SubMesh.NumVertices];

                SubMesh.VertexBuffer.GetData(SubmeshVertices);

                foreach (var Vertex in SubmeshVertices)
                {
                    Vertices.Add(
                        new Vector3(Vertex.Position.X, Vertex.Position.Y, Vertex.Position.Z)
                    );
                }
            }
            return Vertices;
        }

        private static List<int> ExtractIndicesFromStaticMesh(StaticMesh StaticMesh)
        {
            List<int> Indices = new List<int>();

            foreach (StaticMesh.SubMesh SubMesh in StaticMesh.SubMeshes)
            {
                ushort[] SubmeshIndices = new ushort[SubMesh.IndexBuffer.IndexCount];
                SubMesh.IndexBuffer.GetData(SubmeshIndices);

                foreach (var Index in SubmeshIndices)
                {
                    Indices.Add(Index);
                }
            }

            return Indices;
        }

        public static CollisionShape CreateCollisionShapeFromModel(
            Model Model,
            Transform Transform,
            Microsoft.Xna.Framework.Vector3? Scale = null,
            bool Convex = true
        )
        {
            Microsoft.Xna.Framework.Vector3 ScalingFactor = Scale * Transform.Scale ?? Microsoft.Xna.Framework.Vector3.One * Transform.Scale;

            List<Vector3> Vertices = ExtractVerticesFromModel(Model);
            List<int> Indices = ExtractIndicesFromModel(Model);

            if (Convex)
            {
                ConvexHullShape ConvexShape = new ConvexHullShape();
                foreach (var Vertex in Vertices)
                {
                    var ScaledVertex = new Vector3(
                        Vertex.X * ScalingFactor.X / Transform.Scale.X,
                        Vertex.Y * ScalingFactor.Y / Transform.Scale.Y,
                        Vertex.Z * ScalingFactor.Z / Transform.Scale.Z
                    );
                    ConvexShape.AddPoint(ScaledVertex, false);
                }
                ConvexShape.RecalcLocalAabb();
                return ConvexShape;
            }
            else
            {
                TriangleMesh TriangleMesh = new TriangleMesh();

                Task ProcessMesh = Task.Run(() =>
                {
                    for (int I = 0; I < Indices.Count; I += 3)
                    {
                        Vector3 ScaledVertex1 = new Vector3(
                            Vertices[Indices[I]].X * ScalingFactor.X,
                            Vertices[Indices[I]].Y * ScalingFactor.Y,
                            Vertices[Indices[I]].Z * ScalingFactor.Z
                        );
                        Vector3 ScaledVertex2 = new Vector3(
                            Vertices[Indices[I + 1]].X * ScalingFactor.X,
                            Vertices[Indices[I + 1]].Y * ScalingFactor.Y,
                            Vertices[Indices[I + 1]].Z * ScalingFactor.Z
                        );
                        Vector3 ScaledVertex3 = new Vector3(
                            Vertices[Indices[I + 2]].X * ScalingFactor.X,
                            Vertices[Indices[I + 2]].Y * ScalingFactor.Y,
                            Vertices[Indices[I + 2]].Z * ScalingFactor.Z
                        );

                        TriangleMesh.AddTriangle(ScaledVertex1, ScaledVertex2, ScaledVertex3, true);
                    }
                });
                ProcessMesh.Wait();

                GImpactMeshShape GImpactShape = new GImpactMeshShape(TriangleMesh);
                GImpactShape.UpdateBound();

                return GImpactShape;
            }
        }

        private static List<Vector3> ExtractVerticesFromModel(Model Model)
        {
            List<Vector3> Vertices = new List<Vector3>();
            foreach (ModelMesh Mesh in Model.Meshes)
            {
                foreach (ModelMeshPart Part in Mesh.MeshParts)
                {
                    VertexPositionNormalTexture[] VertexData = new VertexPositionNormalTexture[
                        Part.NumVertices
                    ];
                    Part.VertexBuffer.GetData(VertexData);

                    foreach (var Vertex in VertexData)
                    {
                        Vertices.Add(
                            new Vector3(Vertex.Position.X, Vertex.Position.Y, Vertex.Position.Z)
                        );
                    }
                }
            }
            return Vertices;
        }

        private static List<int> ExtractIndicesFromModel(Model Model)
        {
            List<int> Indices = new List<int>();

            foreach (ModelMesh Mesh in Model.Meshes)
            {
                foreach (ModelMeshPart Part in Mesh.MeshParts)
                {
                    ushort[] IndexData = new ushort[Part.IndexBuffer.IndexCount];
                    Part.IndexBuffer.GetData(IndexData);

                    foreach (var Index in IndexData)
                    {
                        Indices.Add(Index);
                    }
                }
            }

            return Indices;
        }
    }
}
