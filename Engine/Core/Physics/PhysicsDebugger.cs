using BulletSharp;
using Engine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Core.Physics
{
    public class PhysicsDebugger
    {
        private GraphicsDevice GraphicsDevice;
        private BasicEffect Effect;

        public PhysicsDebugger()
        {
            GraphicsDevice = EngineManager.Instance.GraphicsDevice;
            Effect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        private void SetEffectMatrices(Matrix world, Matrix view, Matrix projection)
        {
            ApplyEffectPass();
            Effect.World = world;
            Effect.View = view;
            Effect.Projection = projection;
        }

        private void ApplyEffectPass()
        {
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
        }

        public void DrawSphere(
            float radius,
            Matrix world,
            Matrix view,
            Matrix projection,
            Color color
        )
        {
            int segments = 16;
            VertexPositionColor[] vertices = new VertexPositionColor[segments * 2 * 3];
            int index = 0;

            for (int axis = 0; axis < 3; axis++)
            {
                for (int i = 0; i < segments; i++)
                {
                    float theta1 = MathHelper.TwoPi * i / segments;
                    float theta2 = MathHelper.TwoPi * (i + 1) / segments;

                    Vector3 v1 = Vector3.Zero;
                    Vector3 v2 = Vector3.Zero;

                    switch (axis)
                    {
                        case 0:
                            v1 = new Vector3(0, MathF.Sin(theta1), MathF.Cos(theta1)) * radius;
                            v2 = new Vector3(0, MathF.Sin(theta2), MathF.Cos(theta2)) * radius;
                            break;
                        case 1:
                            v1 = new Vector3(MathF.Sin(theta1), 0, MathF.Cos(theta1)) * radius;
                            v2 = new Vector3(MathF.Sin(theta2), 0, MathF.Cos(theta2)) * radius;
                            break;
                        case 2:
                            v1 = new Vector3(MathF.Sin(theta1), MathF.Cos(theta1), 0) * radius;
                            v2 = new Vector3(MathF.Sin(theta2), MathF.Cos(theta2), 0) * radius;
                            break;
                    }

                    vertices[index++] = new VertexPositionColor(v1, color);
                    vertices[index++] = new VertexPositionColor(v2, color);
                }
            }

            SetEffectMatrices(world, view, projection);
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, segments * 3);
        }

        public void DrawBox(
            Vector3 halfExtents,
            Matrix world,
            Matrix view,
            Matrix projection,
            Color color
        )
        {
            VertexPositionColor[] vertices = new VertexPositionColor[24];
            Vector3[] corners = new Vector3[8];

            for (int i = 0; i < 8; i++)
            {
                float x = (i & 1) == 0 ? -halfExtents.X : halfExtents.X;
                float y = (i & 2) == 0 ? -halfExtents.Y : halfExtents.Y;
                float z = (i & 4) == 0 ? -halfExtents.Z : halfExtents.Z;

                corners[i] = new Vector3(x, y, z);
            }

            int[] indices =
            {
                0,
                1,
                1,
                3,
                3,
                2,
                2,
                0,
                4,
                5,
                5,
                7,
                7,
                6,
                6,
                4,
                0,
                4,
                1,
                5,
                2,
                6,
                3,
                7
            };

            for (int i = 0; i < indices.Length; i += 2)
            {
                vertices[i] = new VertexPositionColor(corners[indices[i]], color);
                vertices[i + 1] = new VertexPositionColor(corners[indices[i + 1]], color);
            }

            SetEffectMatrices(world, view, projection);
            GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.LineList,
                vertices,
                0,
                vertices.Length / 2
            );
        }
        public void DrawCone(
            float radius,
            float height,
            Matrix world,
            Matrix view,
            Matrix projection,
            Color color
        )
        {
            int segments = 16;
            VertexPositionColor[] vertices = new VertexPositionColor[segments * 6];
            int index = 0;

            for (int i = 0; i < segments; i++)
            {
                float theta1 = MathHelper.TwoPi * i / segments;
                float theta2 = MathHelper.TwoPi * (i + 1) / segments;

                Vector3 top = new Vector3(0, height / 2, 0);
                Vector3 base1 = new Vector3(
                    MathF.Cos(theta1) * radius,
                    -height / 2,
                    MathF.Sin(theta1) * radius
                );
                Vector3 base2 = new Vector3(
                    MathF.Cos(theta2) * radius,
                    -height / 2,
                    MathF.Sin(theta2) * radius
                );

                vertices[index++] = new VertexPositionColor(top, color);
                vertices[index++] = new VertexPositionColor(base1, color);

                vertices[index++] = new VertexPositionColor(top, color);
                vertices[index++] = new VertexPositionColor(base2, color);

                vertices[index++] = new VertexPositionColor(base1, color);
                vertices[index++] = new VertexPositionColor(base2, color);
            }

            SetEffectMatrices(world, view, projection);
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, index / 2);
        }
        public void DrawCylinder(
            float radius,
            float height,
            Matrix world,
            Matrix view,
            Matrix projection,
            Color color
        )
        {
            int segments = 16;
            VertexPositionColor[] vertices = new VertexPositionColor[segments * 6];
            int index = 0;

            for (int i = 0; i < segments; i++)
            {
                float theta1 = MathHelper.TwoPi * i / segments;
                float theta2 = MathHelper.TwoPi * (i + 1) / segments;

                Vector3 top1 = new Vector3(
                    MathF.Cos(theta1) * radius,
                    height / 2,
                    MathF.Sin(theta1) * radius
                );
                Vector3 top2 = new Vector3(
                    MathF.Cos(theta2) * radius,
                    height / 2,
                    MathF.Sin(theta2) * radius
                );
                Vector3 bottom1 = new Vector3(
                    MathF.Cos(theta1) * radius,
                    -height / 2,
                    MathF.Sin(theta1) * radius
                );
                Vector3 bottom2 = new Vector3(
                    MathF.Cos(theta2) * radius,
                    -height / 2,
                    MathF.Sin(theta2) * radius
                );

                vertices[index++] = new VertexPositionColor(top1, color);
                vertices[index++] = new VertexPositionColor(top2, color);

                vertices[index++] = new VertexPositionColor(bottom1, color);
                vertices[index++] = new VertexPositionColor(bottom2, color);

                vertices[index++] = new VertexPositionColor(top1, color);
                vertices[index++] = new VertexPositionColor(bottom1, color);
            }

            SetEffectMatrices(world, view, projection);
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, index / 2);
        }
        public void DrawCapsule(
            float radius,
            float height,
            Matrix world,
            Matrix view,
            Matrix projection,
            Color color
        )
        {
            int segments = 16;
            int rings = segments / 2;
            int cylinderVertexCount = segments * 6;
            int hemisphereVertexCount = rings * segments * 6;
            int totalVertexCount = cylinderVertexCount + 2 * hemisphereVertexCount;

            VertexPositionColor[] vertices = new VertexPositionColor[totalVertexCount];
            int index = 0;

            float cylinderHeight = height - 2 * radius;

            for (int i = 0; i < segments; i++)
            {
                float theta1 = MathHelper.TwoPi * i / segments;
                float theta2 = MathHelper.TwoPi * (i + 1) / segments;

                Vector3 top1 = new Vector3(
                    MathF.Cos(theta1) * radius,
                    cylinderHeight / 2,
                    MathF.Sin(theta1) * radius
                );
                Vector3 top2 = new Vector3(
                    MathF.Cos(theta2) * radius,
                    cylinderHeight / 2,
                    MathF.Sin(theta2) * radius
                );
                Vector3 bottom1 = new Vector3(
                    MathF.Cos(theta1) * radius,
                    -cylinderHeight / 2,
                    MathF.Sin(theta1) * radius
                );
                Vector3 bottom2 = new Vector3(
                    MathF.Cos(theta2) * radius,
                    -cylinderHeight / 2,
                    MathF.Sin(theta2) * radius
                );

                vertices[index++] = new VertexPositionColor(top1, color);
                vertices[index++] = new VertexPositionColor(bottom1, color);

                vertices[index++] = new VertexPositionColor(top1, color);
                vertices[index++] = new VertexPositionColor(top2, color);

                vertices[index++] = new VertexPositionColor(bottom1, color);
                vertices[index++] = new VertexPositionColor(bottom2, color);
            }

            for (int i = 0; i < segments; i++)
            {
                float theta1 = MathHelper.TwoPi * i / segments;
                float theta2 = MathHelper.TwoPi * (i + 1) / segments;

                for (int j = 0; j < rings; j++)
                {
                    float phi1 = MathHelper.PiOver2 * j / rings;
                    float phi2 = MathHelper.PiOver2 * (j + 1) / rings;

                    Vector3 topCenter = new Vector3(0, cylinderHeight / 2, 0);

                    Vector3 top1 =
                        topCenter
                        + new Vector3(
                            radius * MathF.Sin(phi1) * MathF.Cos(theta1),
                            radius * MathF.Cos(phi1),
                            radius * MathF.Sin(phi1) * MathF.Sin(theta1)
                        );

                    Vector3 top2 =
                        topCenter
                        + new Vector3(
                            radius * MathF.Sin(phi1) * MathF.Cos(theta2),
                            radius * MathF.Cos(phi1),
                            radius * MathF.Sin(phi1) * MathF.Sin(theta2)
                        );

                    Vector3 top3 =
                        topCenter
                        + new Vector3(
                            radius * MathF.Sin(phi2) * MathF.Cos(theta1),
                            radius * MathF.Cos(phi2),
                            radius * MathF.Sin(phi2) * MathF.Sin(theta1)
                        );

                    Vector3 top4 =
                        topCenter
                        + new Vector3(
                            radius * MathF.Sin(phi2) * MathF.Cos(theta2),
                            radius * MathF.Cos(phi2),
                            radius * MathF.Sin(phi2) * MathF.Sin(theta2)
                        );

                    vertices[index++] = new VertexPositionColor(top1, color);
                    vertices[index++] = new VertexPositionColor(top2, color);
                    vertices[index++] = new VertexPositionColor(top3, color);
                    vertices[index++] = new VertexPositionColor(top3, color);
                    vertices[index++] = new VertexPositionColor(top2, color);
                    vertices[index++] = new VertexPositionColor(top4, color);

                    Vector3 bottomCenter = new Vector3(0, -cylinderHeight / 2, 0);

                    Vector3 bottom1 =
                        bottomCenter
                        + new Vector3(
                            radius * MathF.Sin(phi1) * MathF.Cos(theta1),
                            -radius * MathF.Cos(phi1),
                            radius * MathF.Sin(phi1) * MathF.Sin(theta1)
                        );

                    Vector3 bottom2 =
                        bottomCenter
                        + new Vector3(
                            radius * MathF.Sin(phi1) * MathF.Cos(theta2),
                            -radius * MathF.Cos(phi1),
                            radius * MathF.Sin(phi1) * MathF.Sin(theta2)
                        );

                    Vector3 bottom3 =
                        bottomCenter
                        + new Vector3(
                            radius * MathF.Sin(phi2) * MathF.Cos(theta1),
                            -radius * MathF.Cos(phi2),
                            radius * MathF.Sin(phi2) * MathF.Sin(theta1)
                        );

                    Vector3 bottom4 =
                        bottomCenter
                        + new Vector3(
                            radius * MathF.Sin(phi2) * MathF.Cos(theta2),
                            -radius * MathF.Cos(phi2),
                            radius * MathF.Sin(phi2) * MathF.Sin(theta2)
                        );

                    vertices[index++] = new VertexPositionColor(bottom1, color);
                    vertices[index++] = new VertexPositionColor(bottom2, color);
                    vertices[index++] = new VertexPositionColor(bottom3, color);
                    vertices[index++] = new VertexPositionColor(bottom3, color);
                    vertices[index++] = new VertexPositionColor(bottom2, color);
                    vertices[index++] = new VertexPositionColor(bottom4, color);
                }
            }

            SetEffectMatrices(world, view, projection);
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, index / 2);
        }

        public void DrawCollisionShape(
            CollisionShape shape,
            Transform TransformObj,
            Matrix world,
            Matrix view,
            Matrix projection,
            bool isStatic
        )
        {
            Color color = isStatic ? Color.Red : Color.Cyan;

            switch (shape)
            {
                case SphereShape sphere:
                    DrawSphere(sphere.Radius, world, view, projection, color);
                    break;

                case BoxShape box:
                    DrawBox(
                        new Vector3(
                            box.HalfExtentsWithoutMargin.X,
                            box.HalfExtentsWithoutMargin.Y,
                            box.HalfExtentsWithoutMargin.Z
                        ),
                        world,
                        view,
                        projection,
                        color
                    );
                    break;

                case CapsuleShape capsule:
                    DrawCapsule(
                        capsule.Radius,
                        capsule.HalfHeight * 2,
                        world,
                        view,
                        projection,
                        color
                    );
                    break;

                case CylinderShape cylinder:
                    DrawCylinder(
                        cylinder.Radius,
                        cylinder.HalfExtentsWithoutMargin.Y * 2,
                        world,
                        view,
                        projection,
                        color
                    );
                    break;

                case ConeShape cone:
                    DrawCone(cone.Radius, cone.Height / 2, world, view, projection, color);
                    break;

                case CompoundShape compoundShape:
                    for (int i = 0; i < compoundShape.NumChildShapes; i++)
                    {
                        CollisionShape childShape = compoundShape.GetChildShape(i);
                        DrawCollisionShape(
                            childShape,
                            TransformObj,
                            world,
                            view,
                            projection,
                            isStatic
                        );
                    }
                    break;
            }
        }
    }
}
