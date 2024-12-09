﻿using Engine.Core.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Linq;
namespace Engine.Core.ECS
{

    public interface IComponentLifecycle
    {
        void Awake();
        void Start();
        void MainUpdate(GameTime gameTime);
        void FixedUpdate(GameTime gameTime);
        void Render(BasicEffect basicEffect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime);
        void DrawGUI(GameTime gameTime);
        void OnDestroy();
    }

    public struct Entity
    {
        public int ID;
        public Transform Transform;
    }
    public class ECSManager
    {
        private static readonly Lazy<ECSManager> _lazyInstance = new Lazy<ECSManager>(() => new ECSManager());
        public static ECSManager Instance => _lazyInstance.Value;

        private readonly List<int> _entities = new();
        private int _nextEntityId;

        // Cache dictionaries to avoid repeated lookups
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, object>> _components = new();
        private readonly ConcurrentDictionary<Type, List<IComponentLifecycle>> _lifecycleComponents = new();
        private readonly ConcurrentDictionary<Entity, float> _timedRemovals = new();

        private List<IComponentLifecycle> _startQueuedComponents = new();

        private ECSManager() { }

        public Entity CreateEntity()
        {
            int entityId = _nextEntityId++;
            _entities.Add(entityId);
            Entity entity = new Entity { ID = entityId, Transform = new Transform() };
            AddComponent(entity, entity.Transform);
            return entity;
        }

        public void RemoveEntity(Entity entity)
        {
            foreach (var componentType in _components.Values)
            {
                if (componentType.TryRemove(entity.ID, out var component) && component is IComponentLifecycle lifecycleComponent)
                {
                    _lifecycleComponents[lifecycleComponent.GetType()].Remove(lifecycleComponent);
                    lifecycleComponent.OnDestroy();
                }
            }
            _entities.Remove(entity.ID);
        }

        public void RemoveEntityTimed(Entity entity, float seconds)
        {
            _timedRemovals.TryAdd(entity, seconds);
        }

        public void AddComponent<T>(Entity entity, T component) where T : Component
        {
            var type = typeof(T);
            if (!_components.ContainsKey(type))
            {
                _components[type] = new ConcurrentDictionary<int, object>();
                _lifecycleComponents[type] = new List<IComponentLifecycle>();
            }

            _components[type][entity.ID] = component;
            component.SetEntity(entity);
            component.SetTransform(entity.Transform);
            component.Awake();

            if (component is IComponentLifecycle lifecycleComponent)
            {
                _lifecycleComponents[type].Add(lifecycleComponent);
                if (_startQueuedComponents.Contains(lifecycleComponent) == false)
                {
                    _startQueuedComponents.Add(lifecycleComponent);
                }
            }
        }

        public T GetComponent<T>(Entity entity) where T : class
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var entityComponents) && entityComponents.TryGetValue(entity.ID, out var component))
            {
                return (T)component;
            }
            throw new Exception($"Entity {entity.ID} does not have a component of type {type.Name}");
        }

        public bool HasComponent<T>(Entity entity)
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var entityComponents) && entityComponents.ContainsKey(entity.ID);
        }

        public void RemoveComponent<T>(Entity entity)
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var entityComponents))
            {
                if (entityComponents.TryRemove(entity.ID, out var component) && component is IComponentLifecycle lifecycleComponent)
                {
                    _lifecycleComponents[type].Remove(lifecycleComponent);
                }
            }
        }

        public List<int> GetEntitiesWithComponent<T>()
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var entityComponents) ? entityComponents.Keys.ToList() : new List<int>();
        }

        public void CallFixedUpdateOnComponents(GameTime gameTime)
        {
            ProcessTimedRemovals(gameTime);
            foreach (var lifecycle in _lifecycleComponents.Values.SelectMany(l => l))
            {
                lifecycle.FixedUpdate(gameTime);
            }
        }

        public void CallMainUpdateOnComponents(GameTime gameTime)
        {
            foreach (var lifecycleComponent in _startQueuedComponents)
            {
                lifecycleComponent.Start();
            }
            _startQueuedComponents.Clear();

            foreach (var lifecycle in _lifecycleComponents.Values.SelectMany(l => l))
            {
                lifecycle.MainUpdate(gameTime);
            }
        }

        public void CallRenderOnComponents(BasicEffect basicEffect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            foreach (var lifecycle in _lifecycleComponents.Values.SelectMany(l => l))
            {
                lifecycle.Render(basicEffect, viewMatrix, projectionMatrix, gameTime);
            }
        }

        public void CallDrawGUIOnComponents(GameTime gameTime)
        {
            foreach (var lifecycle in _lifecycleComponents.Values.SelectMany(l => l))
            {
                lifecycle.DrawGUI(gameTime);
            }
        }

        public Entity GetEntityById(int entityId)
        {
            if (_entities.Contains(entityId))
            {
                return new Entity
                {
                    ID = entityId,
                    Transform = GetComponent<Transform>(new Entity { ID = entityId })
                };
            }
            throw new Exception($"Entity with ID {entityId} does not exist.");
        }

        private void ProcessTimedRemovals(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var kvp in _timedRemovals)
            {
                if (kvp.Value <= elapsedSeconds)
                {
                    RemoveEntity(kvp.Key);
                    _timedRemovals.TryRemove(kvp.Key, out _);
                }
                else
                {
                    _timedRemovals[kvp.Key] -= elapsedSeconds;
                }
            }
        }
    }
}