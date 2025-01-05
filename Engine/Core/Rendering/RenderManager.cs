using Engine.Core.Components.Rendering;
using Engine.Core.ECS;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using SharpFont;

namespace Engine.Core.Rendering
{
    public class RenderManager
    {
        private static RenderManager _instance;
        private Vector3 CachedCameraPosition;
        private Matrix LastViewMatrix;
        private List<int> CachedRendererEntities;
        private List<MeshRenderer> CachedRenderers;
        private int LastEntityCount;
        private List<MeshRenderer> SortedRenderers;

        private RenderManager()
        {
            CachedRendererEntities = new List<int>();
            CachedRenderers = new List<MeshRenderer>();
            SortedRenderers = new List<MeshRenderer>();
            LastEntityCount = 0;
        }

        public static RenderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RenderManager();
                }
                return _instance;
            }
        }

        private Vector3 CalculateCameraPosition(Matrix viewMatrix)
        {
            if (LastViewMatrix != viewMatrix)
            {
                LastViewMatrix = viewMatrix;
                CachedCameraPosition = Matrix.Invert(viewMatrix).Translation;
            }
            return CachedCameraPosition;
        }

        private void CacheRendererEntitiesAndComponents()
        {
            List<int> currentRendererEntities = ECSManager.Instance.GetEntitiesWithComponent<MeshRenderer>();

            if (currentRendererEntities.Count != LastEntityCount || !AreEntitiesEqual(currentRendererEntities))
            {
                CachedRendererEntities.Clear();
                CachedRendererEntities.AddRange(currentRendererEntities);

                CachedRenderers.Clear();

                Parallel.ForEach(CachedRendererEntities, entity =>
                {
                    var components = ECSManager.Instance.GetComponents<MeshRenderer>(ECSManager.Instance.GetEntityById(entity));
                    lock (CachedRenderers)
                    {
                        CachedRenderers.AddRange(components);
                    }
                });

                LastEntityCount = currentRendererEntities.Count;
            }
        }

        private bool AreEntitiesEqual(List<int> currentEntities)
        {
            if (currentEntities.Count != CachedRendererEntities.Count)
                return false;

            return !currentEntities.Where((t, i) => t != CachedRendererEntities[i]).Any();
        }

        public void Render(BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {

            var SortTask = Task.Run(() =>
            {
                Vector3 cameraPosition = CalculateCameraPosition(viewMatrix);

                CacheRendererEntitiesAndComponents();


                var renderersToSort = new List<MeshRenderer>(CachedRenderers);

                renderersToSort.Sort((a, b) =>
                {
                    int sortOrderComparison = a.SortOrderTotal.CompareTo(b.SortOrderTotal);
                    if (sortOrderComparison != 0)
                    {
                        return sortOrderComparison;
                    }
                    else
                    {
                        float distanceA = Vector3.DistanceSquared(cameraPosition, a.Transform.Position);
                        float distanceB = Vector3.DistanceSquared(cameraPosition, b.Transform.Position);
                        return distanceB.CompareTo(distanceA);
                    }
                });

                lock (SortedRenderers)
                {
                    SortedRenderers = renderersToSort;
                }
            });
            SortTask.Wait();


            foreach (var renderer in SortedRenderers)
            {
                renderer.RenderMesh(effect, viewMatrix, projectionMatrix, gameTime);
            }
        }
    }
}
