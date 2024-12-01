﻿using System;
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
            ECSManager.Instance.RemoveEntity(EntityId);
        }
        public void SetEntityId(int entityId)
        {
            EntityId = entityId;
        }

    }
}
