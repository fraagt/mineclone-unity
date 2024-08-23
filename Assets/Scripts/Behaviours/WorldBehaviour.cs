using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Auburn.FastNoiseLite;
using Data;
using Enums;
using Pools;
using UnityEngine;

namespace Behaviours
{
    public class WorldBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform chunkContainer;
        [SerializeField] private ChunkBehaviourPool pool;

        private Vector2Int currentChunkIndex = Vector2Int.zero;
        private List<ChunkData> _allChunks = new();
        private List<ChunkBehaviour> _loadedChunks = new();
        private FastNoiseLite _noise;
        private readonly List<Vector2Int> _chunksToLoad = new();
        private readonly List<Vector2Int> _chunksToUnload = new();

        private void Awake()
        {
            _noise = new FastNoiseLite(GameData.Settings.Seed);
            pool.ExpandPool();
            UpdateChunks();

            StartCoroutine(UnloadChunks());
            StartCoroutine(LoadChunks());
        }

        private IEnumerator UnloadChunks()
        {
            while (true)
            {
                if (_chunksToUnload.Count != 0)
                {
                    var chunksToUnload = Mathf.Min(10, _chunksToUnload.Count);
                    for (var i = 0; i < chunksToUnload; i++)
                    {
                        var chunkIndex = _chunksToUnload.First();
                        var chunk = _loadedChunks.FirstOrDefault(entry => entry.Data.Index == chunkIndex);
                        if (chunk)
                            HideChunk(chunk);

                        _chunksToUnload.Remove(chunkIndex);
                    }
                }

                yield return null;
            }
        }

        private IEnumerator LoadChunks()
        {
            while (true)
            {
                if (_chunksToLoad.Count != 0)
                {
                    var chunksToLoad = Mathf.Min(10, _chunksToLoad.Count);
                    for (var i = 0; i < chunksToLoad; i++)
                    {
                        var chunkIndex = _chunksToLoad.First();
                        var chunkData = _allChunks.FirstOrDefault(entry => entry.Index == chunkIndex);
                        if (chunkData == null)
                        {
                            chunkData = GenChunk(chunkIndex);
                            _allChunks.Add(chunkData);
                        }
                        
                        ShowChunk(chunkData);

                        _chunksToLoad.Remove(chunkIndex);
                    }
                }

                yield return null;
            }
        }


        private void Update()
        {
            var currentPosition = GameData.Player.Pose.position;

            var chunkX = Mathf.FloorToInt(currentPosition.x / ChunkConstants.LENGTH);
            var chunkZ = Mathf.FloorToInt(currentPosition.z / ChunkConstants.LENGTH);
            var chunkIndex = new Vector2Int(chunkX, chunkZ);

            var isMoved = chunkIndex != currentChunkIndex;

            if (!isMoved)
            {
                return;
            }
            currentChunkIndex = chunkIndex;
            
            UpdateChunks();
        }

        private void UpdateChunks()
        {
            _chunksToLoad.Clear();
            _chunksToUnload.Clear();

            var renderDistance = GameData.Settings.RenderDistance;
            var renderDiameter = renderDistance * 2;

            var chunksToLoad = new List<Vector2Int>();

            var leftBottomIndex = currentChunkIndex - new Vector2Int(renderDistance, renderDistance);
            var delta = Vector2Int.zero;
            for (delta.y = 0; delta.y <= renderDiameter; ++delta.y)
            {
                for (delta.x = 0; delta.x <= renderDiameter; ++delta.x)
                {
                    var index = leftBottomIndex + delta;
                    var shift = currentChunkIndex - index;
                    if (shift.magnitude < renderDistance)
                        chunksToLoad.Add(index);
                }
            }

            var loadedChunks = _loadedChunks.Select(entry => entry.Data.Index).ToList();
            var chunksToLoadSet = chunksToLoad.ToHashSet();
            var chunksToUnload = loadedChunks.Where(entry => !chunksToLoadSet.Contains(entry));
            chunksToUnload = chunksToUnload.OrderByDescending(entry => (entry - currentChunkIndex).magnitude).ToList();
            _chunksToUnload.AddRange(chunksToUnload);

            chunksToLoad = chunksToLoad.Where(entry => !loadedChunks.Contains(entry)).ToList();
            chunksToLoad = chunksToLoad.OrderBy(entry => (entry - currentChunkIndex).magnitude).ToList();
            _chunksToLoad.AddRange(chunksToLoad);
        }

        private ChunkData GenChunk(Vector2Int index)
        {
            var chunkPosition = new Vector3(
                index.x * ChunkConstants.LENGTH_FLOAT,
                0.0f,
                index.y * ChunkConstants.LENGTH_FLOAT
            );
            var chunkData = new ChunkData(index, chunkPosition);

            var heightLevel = _noise.GetNoise(index.x * 4, index.y * 4);
            heightLevel = (heightLevel + 1.0f) / 2.0f;
            heightLevel *= 50;
            var height = (int)heightLevel;
            chunkData.Height = heightLevel;

            for (var chunkY = 0; chunkY < ChunkConstants.LENGTH; ++chunkY)
            {
                for (var chunkX = 0; chunkX < ChunkConstants.LENGTH; ++chunkX)
                {
                    for (var chunkZ = 0; chunkZ < ChunkConstants.LENGTH; ++chunkZ)
                    {
                        chunkData.Blocks[chunkX, chunkY, chunkZ] =
                            chunkY < height ? EBlockType.Dirt : EBlockType.Air;
                    }
                }
            }

            return chunkData;
        }

        private void ShowChunk(ChunkData chunkData)
        {
            var chunk = pool.Spawn();
            chunk.gameObject.transform.SetParent(chunkContainer);
            chunk.gameObject.SetActive(true);
            chunk.Init(chunkData);
            _loadedChunks.Add(chunk);
        }

        private void HideChunk(ChunkBehaviour chunk)
        {
            _loadedChunks.Remove(chunk);
            chunk.gameObject.SetActive(false);
            chunk.gameObject.transform.SetParent(null);
            pool.Despawn(chunk);
        }
    }
}