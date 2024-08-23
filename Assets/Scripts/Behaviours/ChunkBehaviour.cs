using Data;
using UnityEngine;

namespace Behaviours
{
    public class ChunkBehaviour : MonoBehaviour
    {
        public ChunkData Data { get; private set; }

        private Material _material;

        public void Init(ChunkData chunkData)
        {
            Data = chunkData;
            // transform.position = chunkData.Position;
            transform.position = new Vector3(chunkData.Position.x, chunkData.Height, chunkData.Position.z);
        }
    }
}