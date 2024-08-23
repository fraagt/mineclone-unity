using System;
using Behaviours;
using Data;

namespace ScriptableObjects.Data
{
    [Serializable]
    public class PrefabsDatabaseVo
    {
        public ChunkBehaviour ChunkBehaviour;
        public PlayerBehaviour PlayerBehaviour;
        public WorldBehaviour WorldBehaviour;

        public PrefabsDatabase ToPrefabsDatabase()
        {
            return new PrefabsDatabase(
                ChunkBehaviour, 
                PlayerBehaviour, 
                WorldBehaviour);
        }
    }
}