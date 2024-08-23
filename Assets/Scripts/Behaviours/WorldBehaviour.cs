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

        private Vector2Int _currentChunkIndex;
        private readonly List<ChunkData> _allChunks = new();
        private readonly List<ChunkBehaviour> _loadedChunks = new();
        private FastNoiseLite _noise;
        private readonly HashSet<Vector2Int> _chunksToLoad = new();
        private readonly HashSet<Vector2Int> _chunksToUnload = new();

        private const float ChunkUpdateInterval = 0.5f;
        private Vector3 _lastPlayerPosition;
        private float _timeSinceLastUpdate;

        private void Awake()
        {
            _noise = new FastNoiseLite(GameData.Settings.Seed);
            pool.ExpandPool();
            UpdateChunks();

            StartCoroutine(UnloadChunksCoroutine());
            StartCoroutine(LoadChunksCoroutine());
        }

        private void Update()
        {
            _timeSinceLastUpdate += Time.deltaTime;
            var currentPosition = GameData.Player.Pose.position;

            if (_timeSinceLastUpdate >= ChunkUpdateInterval || Vector3.Distance(_lastPlayerPosition, currentPosition) > ChunkConstants.LENGTH_FLOAT)
            {
                _lastPlayerPosition = currentPosition;
                _timeSinceLastUpdate = 0f;

                var chunkX = Mathf.FloorToInt(currentPosition.x / ChunkConstants.LENGTH);
                var chunkZ = Mathf.FloorToInt(currentPosition.z / ChunkConstants.LENGTH);
                var chunkIndex = new Vector2Int(chunkX, chunkZ);

                if (chunkIndex != _currentChunkIndex)
                {
                    _currentChunkIndex = chunkIndex;
                    UpdateChunks();
                }
            }
        }

        private void UpdateChunks()
        {
            _chunksToLoad.Clear();
            _chunksToUnload.Clear();

            var renderDistance = GameData.Settings.RenderDistance;
            var renderDiameter = renderDistance * 2;
            var renderDistanceSquared = renderDistance * renderDistance;

            List<Vector2Int> chunksToLoad = new();
            var leftBottomIndex = _currentChunkIndex - new Vector2Int(renderDistance, renderDistance);
            var delta = Vector2Int.zero;
            for (delta.y = 0; delta.y <= renderDiameter; ++delta.y)
            {
                for (delta.x = 0; delta.x <= renderDiameter; ++delta.x)
                {
                    var index = leftBottomIndex + delta;
                    var shift = _currentChunkIndex - index;
                    if (shift.sqrMagnitude < renderDistanceSquared)
                        chunksToLoad.Add(index);
                }
            }

            var loadedChunks = _loadedChunks.Select(entry => entry.Data.Index).ToHashSet();
            var chunksToLoadSet = chunksToLoad.ToHashSet();
            var chunksToUnload = loadedChunks.Where(entry => !chunksToLoadSet.Contains(entry)).OrderByDescending(entry => (entry - _currentChunkIndex).sqrMagnitude).ToList();
            _chunksToUnload.UnionWith(chunksToUnload);

            chunksToLoad = chunksToLoad.Where(entry => !loadedChunks.Contains(entry)).OrderBy(entry => (entry - _currentChunkIndex).sqrMagnitude).ToList();
            _chunksToLoad.UnionWith(chunksToLoad);
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

        private IEnumerator UnloadChunksCoroutine()
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

        private IEnumerator LoadChunksCoroutine()
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
    }
}
