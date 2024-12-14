using Engine.Core.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Linq;
using Engine.Core.Rendering;

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
        private readonly ConcurrentDictionary<int, List<Component>> _components = new();
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
            if (_components.TryGetValue(entity.ID, out var entityComponents))
            {
                foreach (var type in _lifecycleComponents.Keys.ToList())
                {
                    RemoveComponentType(entity.ID, type);
                }

                _components.TryRemove(entity.ID, out _);
            }

            _entities.Remove(entity.ID);
            _timedRemovals.TryRemove(entity, out _);
        }

        public void RemoveEntityTimed(Entity entity, float seconds)
        {
            _timedRemovals.TryAdd(entity, seconds);
        }

        public void AddComponent<T>(Entity entity, T component) where T : Component
        {
            var type = typeof(T);

            // Check if the entity exists
            if (!_entities.Contains(entity.ID))
            {
                Console.WriteLine($"Entity with ID {entity.ID} does not exist.");
                return;
            }

            if (!_components.ContainsKey(entity.ID))
            {
                _components[entity.ID] = new List<Component>();
            }

            var componentList = _components[entity.ID];

            componentList.Add(component);

            component.SetEntity(entity);
            component.SetTransform(entity.Transform);
            component.Awake();

            if (component is IComponentLifecycle lifecycleComponent)
            {
                if (!_lifecycleComponents.ContainsKey(type))
                {
                    _lifecycleComponents[type] = new List<IComponentLifecycle>();
                }

                _lifecycleComponents[type].Add(lifecycleComponent);
                if (!_startQueuedComponents.Contains(lifecycleComponent))
                {
                    _startQueuedComponents.Add(lifecycleComponent);
                }
            }
        }

        public T GetComponent<T>(Entity entity) where T : class
        {
            var type = typeof(T);
            if (_components.TryGetValue(entity.ID, out var entityComponents))
            {
                foreach (var component in entityComponents)
                {
                    if (component is T typedComponent)
                    {
                        return typedComponent;
                    }
                }
            }
            Console.WriteLine($"Entity {entity.ID} does not have a component of type {type.Name}");
            return null;
        }


        public T[] GetComponents<T>(Entity entity) where T : class
        {
            var componentsList = new List<T>();
            if (_components.TryGetValue(entity.ID, out var entityComponents))
            {
                foreach (var component in entityComponents)
                {
                    if (component is T typedComponent)
                    {
                        componentsList.Add(typedComponent);
                    }
                }
            }

            return componentsList.ToArray();
        }

        public bool HasComponent<T>(Entity entity)
        {
            var type = typeof(T);
            return _components.TryGetValue(entity.ID, out var entityComponents) &&
                   entityComponents.Any(c => c.GetType() == type);
        }

        public void RemoveComponent<T>(Entity entity) where T : Component
        {
            var type = typeof(T);

            if (_components.TryGetValue(entity.ID, out var entityComponents))
            {
                var componentsToRemove = entityComponents
                    .Where(c => c.GetType() == type)
                    .ToList();

                foreach (var component in componentsToRemove)
                {
                    entityComponents.Remove(component);

                    if (component is IComponentLifecycle lifecycleComponent)
                    {
                        _lifecycleComponents[type].Remove(lifecycleComponent);
                        if (lifecycleComponent is IDisposable disposableComponent)
                        {
                            disposableComponent.Dispose();
                        }
                    }
                }
            }
        }

        private void RemoveComponentType(int entityId, Type componentType)
        {
            if (_components.TryGetValue(entityId, out var entityComponents))
            {
                var componentsToRemove = entityComponents
                    .Where(c => c.GetType() == componentType)
                    .ToList();

                foreach (var component in componentsToRemove)
                {
                    entityComponents.Remove(component);

                    if (component is IComponentLifecycle lifecycleComponent)
                    {
                        _lifecycleComponents[componentType].Remove(lifecycleComponent);
                        if (lifecycleComponent is IDisposable disposableComponent)
                        {
                            disposableComponent.Dispose();
                        }
                    }
                }
            }
        }


        public List<int> GetEntitiesWithComponent<T>()
        {
            var type = typeof(T);
            return _components.Where(kvp => kvp.Value.Any(c => c.GetType() == type))
                              .Select(kvp => kvp.Key)
                              .ToList();
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
            while (_startQueuedComponents.Count > 0)
            {
                var startComponentsSnapshot = _startQueuedComponents.ToList();
                _startQueuedComponents.Clear();

                foreach (var lifecycleComponent in startComponentsSnapshot)
                {
                    lifecycleComponent.Start();
                }
            }
            var lifecycleSnapshot = _lifecycleComponents.Values.SelectMany(l => l).ToList();
            foreach (var lifecycle in lifecycleSnapshot)
            {
                lifecycle.MainUpdate(gameTime);
            }
        }

        public void CallRenderOnComponents(BasicEffect basicEffect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            RenderManager.Instance.Render(basicEffect, viewMatrix, projectionMatrix, gameTime);
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
            Console.WriteLine($"Entity with ID {entityId} does not exist.");
            return default;
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
