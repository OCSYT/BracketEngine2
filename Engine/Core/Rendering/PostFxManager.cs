using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Core.Rendering
{
    public class PostFxManager
    {
        private int count = 0;
        public Dictionary<int, Effect> PostFxList = new Dictionary<int, Effect>();
        public Dictionary<int, List<KeyValuePair<string, object>>> PostFXParameterList = new Dictionary<int, List<KeyValuePair<string, object>>>();

        private static PostFxManager _instance;
        public static PostFxManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PostFxManager();
                }
                return _instance;
            }
        }

        public int AddEffect(Effect effect, List<KeyValuePair<string, object>> PostFXParameters)
        {
            PostFxList.Add(count, effect);
            PostFXParameterList.Add(count, PostFXParameters);
            int prevcount = count;
            count++;
            return prevcount;
        }

        public void RemoveEffect(int ID)
        {
            if (!PostFxList.ContainsKey(ID) || !PostFXParameterList.ContainsKey(ID))
            {
                throw new ArgumentException($"Effect with ID {ID} does not exist.");
            }
            PostFxList.Remove(ID);
            PostFXParameterList.Remove(ID);
        }

        public void UpdateEffectParameters(int ID, List<KeyValuePair<string, object>> updatedParameters)
        {
            if (!PostFxList.ContainsKey(ID) || !PostFXParameterList.ContainsKey(ID))
            {
                throw new ArgumentException($"Effect with ID {ID} does not exist.");
            }
            PostFXParameterList[ID] = updatedParameters;
        }
    }
}
