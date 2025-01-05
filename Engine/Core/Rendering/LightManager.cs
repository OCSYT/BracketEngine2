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
        private const int MaxDirectionalLights = 16;
        private const int MaxPointLights = 16;
        public Color AmbientColor = new Color(0.1f, 0.1f, 0.1f);
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

        public void UpdateLights(ref Effect effect)
        {

            // Ensure thread safety when modifying shared light data
            lock (_directionalLightsLock)
            {
                lock (_pointLightsLock)
                {
                    try
                    {
                        lock (_directionalLightsLock)
                        {
                            var dirLightCount = Math.Min(DirectionalLights.Count, MaxDirectionalLights);
                            var directions = new Vector3[dirLightCount];
                            var intensities = new float[dirLightCount];
                            var colors = new Vector3[dirLightCount];

                            Parallel.For(0, dirLightCount, i =>
                            {
                                var light = DirectionalLights[i];
                                directions[i] = light.Direction;
                                intensities[i] = light.Intensity;
                                colors[i] = light.Color.ToVector3();
                            });

                            effect.Parameters["AmbientColor"]?.SetValue(AmbientColor.ToVector4());
                            effect.Parameters["dirLightDirection"]?.SetValue(directions);
                            effect.Parameters["dirLightIntensity"]?.SetValue(intensities);
                            effect.Parameters["dirLightColor"]?.SetValue(colors);
                        }

                        lock (_pointLightsLock)
                        {
                            var pointLightCount = Math.Min(PointLights.Count, MaxPointLights);
                            var pointPositions = new Vector3[pointLightCount];
                            var pointIntensities = new float[pointLightCount];
                            var pointColors = new Vector3[pointLightCount];

                            Parallel.For(0, pointLightCount, i =>
                            {
                                var light = PointLights[i];
                                pointPositions[i] = light.Position;
                                pointIntensities[i] = light.Intensity;
                                pointColors[i] = light.Color.ToVector3();
                            });

                            effect.Parameters["pointLightPositions"]?.SetValue(pointPositions);
                            effect.Parameters["pointLightIntensities"]?.SetValue(pointIntensities);
                            effect.Parameters["pointLightColors"]?.SetValue(pointColors);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
