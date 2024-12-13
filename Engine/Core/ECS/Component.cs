using System;
using Engine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core.ECS
{
    public class Component : IComponentLifecycle, IDisposable
    {
        public Entity Entity { get; private set; }
        public Transform Transform { get; private set; }
        public virtual void Awake()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void FixedUpdate(GameTime gameTime)
        {
        }

        public virtual void MainUpdate(GameTime gameTime)
        {
        }
        public virtual void DrawGUI(GameTime gameTime)
        {
        }
        public virtual void Render(BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
        }


        public virtual void OnDestroy()
        {
        }


        public void DestroyEntity()
        {
            ECSManager.Instance.RemoveEntity(Entity);
        }
        public void SetEntity(Entity EntityInstance)
        {
            Entity = EntityInstance;
        }
        public void SetTransform(Transform TransformInstance)
        {
            Transform = TransformInstance;
        }

        public void Dispose()
        {
            OnDestroy();
        }
    }
}
