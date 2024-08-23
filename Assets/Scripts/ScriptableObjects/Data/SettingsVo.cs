using System;
using Data;

namespace ScriptableObjects.Data
{
    [Serializable]
    public class SettingsVo
    {
        public int RenderDistance;
        public int Seed;

        public Settings ToSettings()
        {
            return new Settings
            {
                RenderDistance = RenderDistance,
                Seed = Seed
            };
        }
    }
}