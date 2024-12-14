using Engine.Core.Components.Rendering;
using Engine.Core.ECS;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using SharpFont;

namespace Engine.Core.Rendering
{
    public class RenderManager
    {
        private static RenderManager _instance;

        private RenderManager()
        {
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
        public Vector3 CalculateCameraPosition(Matrix viewMatrix)
        {
            Matrix invertedViewMatrix = Matrix.Invert(viewMatrix);
            return invertedViewMatrix.Translation;
        }

        public void Render(BasicEffect effect, Matrix viewMatrix, Matrix projectionMatrix, GameTime gameTime)
        {
            List<int> rendererEntities = ECSManager.Instance.GetEntitiesWithComponent<MeshRenderer>();
            List<MeshRenderer> renderers = new List<MeshRenderer>();

            foreach (int entity in rendererEntities)
            {
                renderers.AddRange(ECSManager.Instance.GetComponents<MeshRenderer>(ECSManager.Instance.GetEntityById(entity)));
            }

            renderers.Sort((a, b) =>
            {
                int sortOrderComparison = a.SortOrderTotal.CompareTo(b.SortOrderTotal);
                if (sortOrderComparison != 0)
                {
                    return sortOrderComparison;
                }
                else
                {
                    Vector3 CamPos = CalculateCameraPosition(viewMatrix);
                    float distanceA = Vector3.Distance(CamPos, a.Transform.Position);
                    float distanceB = Vector3.Distance(CamPos, b.Transform.Position);
                    return distanceB.CompareTo(distanceA);
                }
            });

            foreach (var renderer in renderers)
            {
                renderer.RenderMesh(effect, viewMatrix, projectionMatrix, gameTime);
            }
        }


    }
}
