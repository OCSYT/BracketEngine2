using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Engine.Core.Components.Rendering;
using Engine.Core.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core.Rendering
{
    public class LightManager
    {
        private const int MaxDirectionalLights = 8;
        private const int MaxPointLights = 8;

        private static LightManager _instance;

        public static LightManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LightManager();
                }
                return _instance;
            }
        }

        public List<LightComponent> DirectionalLights { get; } = new List<LightComponent>();
        public List<LightComponent> PointLights { get; } = new List<LightComponent>();

        private readonly object _directionalLightsLock = new object();
        private readonly object _pointLightsLock = new object();

        private LightManager() { }

        public void RegisterLight(LightComponent light)
        {
            lock (_directionalLightsLock)
            {
                if (light.LightType == LightType.Directional && !DirectionalLights.Contains(light) && DirectionalLights.Count < MaxDirectionalLights)
                {
                    DirectionalLights.Add(light);
                }
            }

            lock (_pointLightsLock)
            {
                if (light.LightType == LightType.Point && !PointLights.Contains(light) && PointLights.Count < MaxPointLights)
                {
                    PointLights.Add(light);
                }
            }
        }

        public void UnregisterLight(LightComponent light)
        {
            lock (_directionalLightsLock)
            {
                if (light.LightType == LightType.Directional)
                {
                    DirectionalLights.Remove(light);
                }
            }

            lock (_pointLightsLock)
            {
                if (light.LightType == LightType.Point)
                {
                    PointLights.Remove(light);
                }
            }
        }

        public void UpdateLights()
        {
            var entitiesWithRenderers = ECSManager.Instance.GetEntitiesWithComponent<MeshRenderer>();

            // Ensure thread safety when modifying shared light data
            lock (_directionalLightsLock)
            {
                lock (_pointLightsLock)
                {
                    Parallel.ForEach(entitiesWithRenderers, renderEntity =>
                    {
                        MeshRenderer renderer = ECSManager.Instance.GetComponent<MeshRenderer>(renderEntity);
                        if (renderer != null)
                        {
                            foreach (Effect effect in renderer.EffectCache.Values)
                            {
                                try
                                {
                                    // Copy directional light data safely
                                    Vector3[] directions = new Vector3[MaxDirectionalLights];
                                    float[] intensities = new float[MaxDirectionalLights];
                                    Vector3[] colors = new Vector3[MaxDirectionalLights];
                                    for (int i = 0; i < DirectionalLights.Count && i < MaxDirectionalLights; i++)
                                    {
                                        var light = DirectionalLights[i];
                                        directions[i] = light.Direction;
                                        intensities[i] = light.Intensity;
                                        colors[i] = light.Color.ToVector3();
                                    }

                                    // Copy point light data safely
                                    Vector3[] pointPositions = new Vector3[MaxPointLights];
                                    float[] pointIntensities = new float[MaxPointLights];
                                    Vector3[] pointColors = new Vector3[MaxPointLights];
                                    for (int i = 0; i < PointLights.Count && i < MaxPointLights; i++)
                                    {
                                        var light = PointLights[i];
                                        pointPositions[i] = light.Position;
                                        pointIntensities[i] = light.Intensity;
                                        pointColors[i] = light.Color.ToVector3();
                                    }

                                    // Set shader parameters
                                    effect.Parameters["dirLightDirection"].SetValue(directions);
                                    effect.Parameters["dirLightIntensity"].SetValue(intensities);
                                    effect.Parameters["dirLightColor"].SetValue(colors);

                                    effect.Parameters["pointLightPositions"].SetValue(pointPositions);
                                    effect.Parameters["pointLightIntensities"].SetValue(pointIntensities);
                                    effect.Parameters["pointLightColors"].SetValue(pointColors);
                                }
                                catch (Exception ex)
                                {
                                    // Consider logging the exception for debugging
                                    Console.WriteLine($"Error updating lights: {ex.Message}");
                                }
                            }
                        }
                    });
                }
            }
        }
    }
}
