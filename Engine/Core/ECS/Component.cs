using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core.ECS
{
    public class Component : IComponentLifecycle
    {
        public int EntityId { get; private set; }
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

        public virtual void Draw(Effect effect, Matrix viewMatrix, Matrix projectionMatrix)
        {
        }

        public virtual void OnDestroy()
        {
        }

        public void SetEntityId(int entityId)
        {
            EntityId = entityId;
        }

    }
}
