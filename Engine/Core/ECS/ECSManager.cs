using Engine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Core.ECS { 
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
    public class ECSManager
    {
        private static readonly Lazy<ECSManager> _lazyInstance = new Lazy<ECSManager>(() => new ECSManager());
        public static ECSManager Instance => _lazyInstance.Value;

        private readonly List<int> _entities = new();
        private int _nextEntityId;

        private readonly Dictionary<Type, List<IComponentLifecycle>> _lifecycleComponents = new();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, object>> _components = new();
        private readonly ConcurrentDictionary<int, float> _timedRemovals = new();

        private readonly HashSet<IComponentLifecycle> _startQueuedComponents = new();

        private ECSManager() { }

        public int CreateEntity()
        {
            int entityId = _nextEntityId++;
            _entities.Add(entityId);
            AddComponent(entityId, new Transform());
            return entityId;
        }

        public void RemoveEntity(int entityId)
        {
            Parallel.ForEach(_components.Values, componentType =>
            {
                if (componentType.TryRemove(entityId, out var component) && component is IComponentLifecycle lifecycleComponent)
                {
                    if (_lifecycleComponents.ContainsKey(component.GetType()))
                    {
                        _lifecycleComponents[component.GetType()].Remove(lifecycleComponent);
                    }
                    lifecycleComponent.OnDestroy();
                }
            });
            _entities.Remove(entityId);
        }

        public void RemoveEntityTimed(int entityId, float seconds)
        {
            _timedRemovals.TryAdd(entityId, seconds);
        }
        public void AddComponent<T>(int entityId, T component) where T : Component
        {
            var type = typeof(T);
            if (!_components.ContainsKey(type))
            {
                _components[type] = new ConcurrentDictionary<int, object>();
            }

            _components[type][entityId] = component;
            component.SetEntityId(entityId);
            component.Awake();


            if (component is IComponentLifecycle lifecycleComponentStart)
            {
                _startQueuedComponents.Add(lifecycleComponentStart);
            }

            if (component is IComponentLifecycle lifecycleComponent)
            {
                if (!_lifecycleComponents.ContainsKey(type))
                {
                    _lifecycleComponents[type] = new List<IComponentLifecycle>();
                }
                _lifecycleComponents[type].Add(lifecycleComponent);
            }
        }


        public T GetComponent<T>(int entityId) where T : class
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var entityComponents) && entityComponents.TryGetValue(entityId, out var component))
            {
                return component as T;
            }

            throw new Exception($"Entity {entityId} does not have a component of type {type.Name}");
        }

        public bool HasComponent<T>(int entityId)
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var entityComponents) && entityComponents.ContainsKey(entityId);
        }

        public void RemoveComponent<T>(int entityId)
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var entityComponents))
            {
                if (entityComponents.TryRemove(entityId, out var component) && component is IComponentLifecycle lifecycleComponent)
                {
                    if (_lifecycleComponents.ContainsKey(type))
                    {
                        _lifecycleComponents[type].Remove(lifecycleComponent);
                    }
                }
            }
        }

        public List<int> GetEntitiesWithComponent<T>()
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var entityComponents)
                ? entityComponents.Keys.ToList()
                : new List<int>();
        }

        public void CallFixedUpdateOnComponents(GameTime gameTime)
        {
            ProcessTimedRemovals(gameTime);

            Parallel.ForEach(_lifecycleComponents.Values.SelectMany(lifecycleList => lifecycleList), lifecycle =>
            {
                lifecycle.FixedUpdate(gameTime);
            });
        }

        public void CallMainUpdateOnComponents(GameTime gameTime)
        {
            foreach (var lifecycleComponent in _startQueuedComponents)
            {
                lifecycleComponent.Start();
            }
            _startQueuedComponents.Clear();

            Parallel.ForEach(_lifecycleComponents.Values.SelectMany(lifecycleList => lifecycleList), lifecycle =>
            {
                lifecycle.MainUpdate(gameTime);
            });
        }

        public void CallRenderOnComponents(BasicEffect basicEffect, Matrix viewMatrix, Matrix projectionMatrix, GameTime GameTime)
        {
            foreach (var lifecycle in _lifecycleComponents.Values.SelectMany(lifecycleList => lifecycleList))
            {
                lifecycle.Render(basicEffect, viewMatrix, projectionMatrix, GameTime);
            }
        }
        public void CallDrawGUIOnComponents(GameTime GameTime)
        {
            foreach (var lifecycle in _lifecycleComponents.Values.SelectMany(lifecycleList => lifecycleList))
            {
                lifecycle.DrawGUI(GameTime);
            }
        }




        private void ProcessTimedRemovals(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Parallel.ForEach(_timedRemovals.ToList(), kvp =>
            {
                if (kvp.Value <= elapsedSeconds)
                {
                    RemoveEntity(kvp.Key);
                    _timedRemovals.TryRemove(kvp.Key, out _);
                }
                else
                {
                    _timedRemovals[kvp.Key] = kvp.Value - elapsedSeconds;
                }
            });
        }
    }
}
