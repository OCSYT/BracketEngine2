using Microsoft.Xna.Framework;
using Engine.Core.EC;

namespace Engine.Core.Components
{
    public class ParentConstraint : Component
    {
        public Transform Parent;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        public Vector3 ScaleOffset;

        public Vector3 PositionLock;
        public Vector3 RotationLock;
        public Vector3 ScaleLock;

        private Transform SelfTransform;

        public ParentConstraint(
            Transform Parent,
            Vector3? PositionOffset = null,
            Vector3? RotationOffset = null,
            Vector3? ScaleOffset = null,
            Vector3? PositionLock = null,
            Vector3? RotationLock = null,
            Vector3? ScaleLock = null)
        {
            this.Parent = Parent;
            this.PositionOffset = PositionOffset ?? Vector3.Zero;
            this.RotationOffset = RotationOffset ?? Vector3.Zero;
            this.ScaleOffset = ScaleOffset ?? Vector3.Zero;

            this.PositionLock = PositionLock ?? Vector3.One;
            this.RotationLock = RotationLock ?? Vector3.One;
            this.ScaleLock = ScaleLock ?? Vector3.One;
        }

        public override void Start()
        {
            SelfTransform = ECManager.Instance.GetComponent<Transform>(Entity);
        }

        public override void MainUpdate(GameTime GameTime)
        {
            if (Parent == null || SelfTransform == null)
                return;

            Vector3 TargetPosition = Parent.Position + PositionOffset;
            Vector3 NewPosition = new Vector3(
                PositionLock.X != 0 ? TargetPosition.X : SelfTransform.Position.X,
                PositionLock.Y != 0 ? TargetPosition.Y : SelfTransform.Position.Y,
                PositionLock.Z != 0 ? TargetPosition.Z : SelfTransform.Position.Z
            );
            SelfTransform.Position = NewPosition;

            Quaternion OffsetRotation = Quaternion.CreateFromYawPitchRoll(
                MathHelper.ToRadians(RotationOffset.Y),
                MathHelper.ToRadians(RotationOffset.X),
                MathHelper.ToRadians(RotationOffset.Z)
            );

            Quaternion TargetRotation = Parent.Rotation * OffsetRotation;
            Quaternion NewRotation = RotationLock == Vector3.One
                ? TargetRotation
                : Quaternion.Slerp(SelfTransform.Rotation, TargetRotation, RotationLock.Length() / 3f);

            SelfTransform.Rotation = NewRotation;

            Vector3 TargetScale = Parent.Scale + ScaleOffset;
            Vector3 NewScale = new Vector3(
                ScaleLock.X != 0 ? TargetScale.X : SelfTransform.Scale.X,
                ScaleLock.Y != 0 ? TargetScale.Y : SelfTransform.Scale.Y,
                ScaleLock.Z != 0 ? TargetScale.Z : SelfTransform.Scale.Z
            );
            SelfTransform.Scale = NewScale;
        }
    }
}