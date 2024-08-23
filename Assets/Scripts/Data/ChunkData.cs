using Enums;
using UnityEngine;

namespace Data
{
    public class ChunkData
    {
        public Vector2Int Index { get; }
        public Vector3 Position { get; }
        public EBlockType[,,] Blocks { get; }

        public float Height;


        public ChunkData(Vector2Int index, Vector3 position)
        {
            Index = index;
            Position = position;
            Blocks = new EBlockType[ChunkConstants.LENGTH, ChunkConstants.HEIGHT, ChunkConstants.LENGTH];
        }
    }
}