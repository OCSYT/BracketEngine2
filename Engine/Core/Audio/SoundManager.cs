using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System;
using Engine.Core.Components;

namespace Engine.Core.Audio
{
    public class SoundManager
    {
        private static readonly SoundManager _instance = new SoundManager();
        private Dictionary<string, SoundEffect> _soundEffects;
        private Dictionary<string, List<SoundEffectInstance>> _activeSounds;
        private Dictionary<string, List<Transform>> _soundTransforms;
        private Dictionary<string, List<AudioEmitter>> _soundEmitters;
        private Dictionary<string, List<float>> _maxDistances;
        private Dictionary<string, List<float>> _originalVolumes;
        private AudioListener _audioListener;
        private static readonly object AudioLock = new object();
        private const int MaxSounds = 128;
        private int CurrentSounds = 0;

        public static SoundManager Instance => _instance;

        private SoundManager()
        {
            _soundEffects = new Dictionary<string, SoundEffect>();
            _activeSounds = new Dictionary<string, List<SoundEffectInstance>>();
            _soundTransforms = new Dictionary<string, List<Transform>>();
            _soundEmitters = new Dictionary<string, List<AudioEmitter>>();
            _maxDistances = new Dictionary<string, List<float>>();
            _originalVolumes = new Dictionary<string, List<float>>();
        }

        public SoundEffect LoadSound(ContentManager content, string soundName, float maxDistance = 100f)
        {
            lock (AudioLock)
            {
                if (!_soundEffects.ContainsKey(soundName))
                {
                    var soundEffect = content.Load<SoundEffect>(soundName);
                    _soundEffects[soundName] = soundEffect;
                    _maxDistances[soundName] = new List<float> { maxDistance };
                    _originalVolumes[soundName] = new List<float>();
                }
                return _soundEffects[soundName];
            }
        }

        // Play a 3D sound
        public SoundEffectInstance Play3DSound(string soundName, Transform transform, AudioListener listener, float maxDistance = 100f, float volume = 1f, float pitch = 0f)
        {
            return PlaySound(soundName, transform, listener, maxDistance, volume, pitch, is3D: true);
        }

        // Play a 2D sound
        public SoundEffectInstance Play2DSound(string soundName, float volume = 1f, float pitch = 0f)
        {
            return PlaySound(soundName, null, null, 0f, volume, pitch, is3D: false);
        }

        private SoundEffectInstance PlaySound(string soundName, Transform transform, AudioListener listener, float maxDistance, float volume, float pitch, bool is3D)
        {
            lock (AudioLock)
            {
                if (CurrentSounds + 1 > MaxSounds)
                {
                    return null; // Prevent playing sound if max limit is reached
                }

                if (_soundEffects.ContainsKey(soundName))
                {
                    _audioListener = listener;
                    var soundEffect = _soundEffects[soundName];
                    var soundInstance = soundEffect.CreateInstance();
                    soundInstance.Pitch = pitch;
                    soundInstance.IsLooped = false;

                    AudioEmitter emitter = null;

                    if (is3D && transform != null && listener != null)
                    {
                        emitter = new AudioEmitter { Position = transform.Position };
                        emitter.Up = Vector3.Up;
                        emitter.Forward = Vector3.Forward;
                        soundInstance.Apply3D(listener, emitter);
                    }

                    soundInstance.Volume = 0;
                    soundInstance.Play();

                    if (!_activeSounds.ContainsKey(soundName))
                    {
                        _activeSounds[soundName] = new List<SoundEffectInstance>();
                        _soundTransforms[soundName] = new List<Transform>();
                        _soundEmitters[soundName] = new List<AudioEmitter>();
                        _maxDistances[soundName] = new List<float>();
                        _originalVolumes[soundName] = new List<float>();
                    }

                    _activeSounds[soundName].Add(soundInstance);
                    if (is3D)
                    {
                        _soundTransforms[soundName].Add(transform);
                        _soundEmitters[soundName].Add(emitter);
                        _maxDistances[soundName].Add(maxDistance);
                    }
                    else
                    {
                        _soundTransforms[soundName].Add(null);
                        _soundEmitters[soundName].Add(null);
                        _maxDistances[soundName].Add(0f);
                    }

                    _originalVolumes[soundName].Add(volume);

                    CurrentSounds++;
                    return soundInstance;
                }

                return null;
            }
        }

        public void Update()
        {
            lock (AudioLock)
            {
                var stoppedSounds = new List<string>();

                foreach (var soundName in _activeSounds.Keys)
                {
                    if (!_soundTransforms.ContainsKey(soundName) ||
                        !_soundEmitters.ContainsKey(soundName) ||
                        !_maxDistances.ContainsKey(soundName) ||
                        !_originalVolumes.ContainsKey(soundName))
                    {
                        continue;
                    }

                    var soundInstances = _activeSounds[soundName];
                    var transforms = _soundTransforms[soundName];
                    var emitters = _soundEmitters[soundName];
                    var maxDistances = _maxDistances[soundName];
                    var originalVolumes = _originalVolumes[soundName];

                    for (int i = soundInstances.Count - 1; i >= 0; i--)
                    {
                        var soundInstance = soundInstances[i];
                        var maxDistance = maxDistances[i];
                        var originalVolume = originalVolumes[i];
                        bool is3DSound = emitters[i] != null;

                        if (is3DSound)
                        {
                            float distance = Vector3.Distance(transforms[i].Position, _audioListener.Position);

                            if (distance > maxDistance)
                            {
                                soundInstance.Volume = 0f;
                            }
                            else
                            {
                                soundInstance.Volume = originalVolume * (1f - distance / maxDistance);
                                soundInstance.Volume = MathHelper.Clamp(soundInstance.Volume, 0f, originalVolume);
                            }
                        }
                        else
                        {
                            soundInstance.Volume = originalVolume; // 2D sounds keep the original volume
                        }

                        if (soundInstance.State == SoundState.Stopped)
                        {
                            soundInstance.Stop();
                            soundInstance.Dispose();
                            soundInstances.RemoveAt(i);
                            transforms.RemoveAt(i);
                            emitters.RemoveAt(i);
                            maxDistances.RemoveAt(i);
                            originalVolumes.RemoveAt(i);
                            CurrentSounds--;
                        }
                    }

                    if (soundInstances.Count == 0)
                    {
                        stoppedSounds.Add(soundName);
                    }
                }

                foreach (var soundName in stoppedSounds)
                {
                    _activeSounds.Remove(soundName);
                    _soundTransforms.Remove(soundName);
                    _soundEmitters.Remove(soundName);
                    _maxDistances.Remove(soundName);
                    _originalVolumes.Remove(soundName);
                }
            }
        }

        public void StopSound(string soundName = null)
        {
            lock (AudioLock)
            {
                if (soundName == null)
                {
                    foreach (var soundNameKey in _activeSounds.Keys)
                    {
                        StopSound(soundNameKey);
                    }
                }
                else
                {
                    if (_activeSounds.ContainsKey(soundName))
                    {
                        foreach (var soundInstance in _activeSounds[soundName])
                        {
                            soundInstance.Stop();
                            soundInstance.Dispose();
                            CurrentSounds--;
                        }
                        _activeSounds[soundName].Clear();
                    }
                }
            }
        }

        public void Cleanup()
        {
            lock (AudioLock)
            {
                foreach (var soundName in _activeSounds.Keys)
                {
                    StopSound(soundName);
                }

                _soundEffects.Clear();
                _activeSounds.Clear();
                _soundTransforms.Clear();
                _soundEmitters.Clear();
                _maxDistances.Clear();
                _originalVolumes.Clear();
            }
        }
    }
}
