using Behaviours;

namespace Data
{
    public class PrefabsDatabase
    {
        public ChunkBehaviour ChunkBehaviour { get; }
        public PlayerBehaviour PlayerBehaviour { get; }
        public WorldBehaviour WorldBehaviour { get; }

        public PrefabsDatabase(
            ChunkBehaviour chunkBehaviour,
            PlayerBehaviour playerBehaviour,
            WorldBehaviour worldBehaviour)
        {
            ChunkBehaviour = chunkBehaviour;
            PlayerBehaviour = playerBehaviour;
            WorldBehaviour = worldBehaviour;
        }
    }
}