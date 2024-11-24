using Microsoft.Xna.Framework;
using Engine.Core.ECS;

namespace Engine.Core.Components
{
    public class Transform : Component
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
        }

        public Matrix GetWorldMatrixNormalized()
        {
            return Matrix.CreateScale(1, 1, 1) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
        }


        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.Forward, Rotation);
            }
        }

        public Vector3 Right
        {
            get
            {
                return Vector3.Transform(Vector3.Right, Rotation);
            }
        }

        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.Up, Rotation);
            }
        }
    }
}
