using System.Collections.Generic;
using System.Linq;
using Behaviours;
using Data;
using UnityEngine;

namespace Pools
{
    public class ChunkBehaviourPool : MonoBehaviour
    {
        private readonly Stack<ChunkBehaviour> _pool = new();
        private int _poolSize;
        private int _poolIncrement = 10;


        public void ExpandPool()
        {
            for (var i = 0; i < _poolIncrement; ++i)
            {
                var chunk = Instantiate(GameData.PrefabsDatabase.ChunkBehaviour, gameObject.transform);
                chunk.gameObject.SetActive(false);
                _pool.Push(chunk);
            }

            _poolSize += _poolIncrement;
        }

        public ChunkBehaviour Spawn()
        {
            if (!_pool.Any())
            {
                ExpandPool();
            }

            return _pool.Pop();
        }

        public void Despawn(ChunkBehaviour chunkBehaviour)
        {
            chunkBehaviour.gameObject.transform.SetParent(gameObject.transform);
            _pool.Push(chunkBehaviour);
        }
    }
}