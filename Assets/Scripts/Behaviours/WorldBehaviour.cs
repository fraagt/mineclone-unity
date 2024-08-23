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
        private readonly List<ChunkData> _loadedChunks = new();
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

            var isExceedOneChunk = (_lastPlayerPosition - currentPosition).sqrMagnitude >
                                   ChunkConstants.LENGTH_SQUARE_FLOAT;
            if (_timeSinceLastUpdate >= ChunkUpdateInterval || isExceedOneChunk)
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

            var loadedChunks = _loadedChunks.Select(entry => entry.Index).ToHashSet();
            var chunksToLoadSet = chunksToLoad.ToHashSet();
            var chunksToUnload = loadedChunks.Where(entry => !chunksToLoadSet.Contains(entry))
                .OrderByDescending(entry => (entry - _currentChunkIndex).sqrMagnitude).ToList();
            _chunksToUnload.UnionWith(chunksToUnload);

            chunksToLoad = chunksToLoad.Where(entry => !loadedChunks.Contains(entry))
                .OrderBy(entry => (entry - _currentChunkIndex).sqrMagnitude).ToList();
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

            for (var chunkX = 0; chunkX < ChunkConstants.LENGTH; ++chunkX)
            {
                for (var chunkZ = 0; chunkZ < ChunkConstants.LENGTH; ++chunkZ)
                {
                    var heightLevel = _noise.GetNoise(index.x * ChunkConstants.LENGTH + chunkX,
                        index.y * ChunkConstants.LENGTH + chunkZ);
                    heightLevel = (heightLevel + 1.0f) / 2.0f;
                    heightLevel *= 32.0f;
                    heightLevel += 32.0f;
                    var height = (int)heightLevel;

                    for (var chunkY = 0; chunkY < ChunkConstants.HEIGHT; ++chunkY)
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
            var chunkBehaviour = pool.Spawn(chunkContainer);
            chunkBehaviour.SetPosition(chunkData.Position);
            // chunkBehaviour.SpawnCubes(chunkData.Blocks);
            chunkBehaviour.SetMesh(chunkData.Mesh);
            chunkData.ChunkBehaviour = chunkBehaviour;
            _loadedChunks.Add(chunkData);
        }

        private void HideChunk(ChunkData chunk)
        {
            _loadedChunks.Remove(chunk);
            var chunkBehaviour = chunk.ChunkBehaviour;
            chunk.ChunkBehaviour = null;
            pool.Despawn(chunkBehaviour);
        }

        private IEnumerator UnloadChunksCoroutine()
        {
            while (true)
            {
                if (_chunksToUnload.Count != 0)
                {
                    var chunksToUnload = Mathf.Min(15, _chunksToUnload.Count);
                    for (var i = 0; i < chunksToUnload; i++)
                    {
                        var chunkIndex = _chunksToUnload.First();
                        var chunk = _loadedChunks.FirstOrDefault(entry => entry.Index == chunkIndex);
                        if (chunk != null)
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
                    var chunksToLoad = Mathf.Min(20, _chunksToLoad.Count);
                    for (var i = 0; i < chunksToLoad; i++)
                    {
                        var chunkIndex = _chunksToLoad.First();
                        var chunkData = _allChunks.FirstOrDefault(entry => entry.Index == chunkIndex);
                        if (chunkData == null)
                        {
                            chunkData = GenChunk(chunkIndex);
                            _allChunks.Add(chunkData);
                        }

                        if (!chunkData.Mesh)
                        {
                            GenChunkMesh(chunkData);
                        }

                        ShowChunk(chunkData);

                        _chunksToLoad.Remove(chunkIndex);
                    }
                }

                yield return null;
            }
        }

        private void GenChunkMesh(ChunkData chunkData)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();

            for (var y = 0; y < ChunkConstants.HEIGHT; ++y)
            {
                for (var x = 0; x < ChunkConstants.LENGTH; ++x)
                {
                    for (var z = 0; z < ChunkConstants.LENGTH; ++z)
                    {
                        var blockType = chunkData.Blocks[x, y, z];

                        if (blockType == EBlockType.Air)
                            continue;

                        var y_ = y + 1;
                        var x_ = x + 1;
                        var z_ = z + 1;

                        var topBlockType = y == ChunkConstants.HEIGHT - 1
                            ? EBlockType.None
                            : chunkData.Blocks[x, y_, z];
                        var bottomBlockType = y == 0
                            ? EBlockType.None
                            : chunkData.Blocks[x, y - 1, z];
                        var leftBlockType = x == 0
                            ? GetNeighbourChunk(chunkData, Vector2Int.left).Blocks[ChunkConstants.LENGTH - 1, y, z]
                            : chunkData.Blocks[x - 1, y, z];
                        var rightBlockType = x == ChunkConstants.LENGTH - 1
                            ? GetNeighbourChunk(chunkData, Vector2Int.right).Blocks[0, y, z]
                            : chunkData.Blocks[x_, y, z];
                        var frontBlockType = z == ChunkConstants.LENGTH - 1
                            ? GetNeighbourChunk(chunkData, Vector2Int.up).Blocks[x, y, 0]
                            : chunkData.Blocks[x, y, z_];
                        var backBlockType = z == 0
                            ? GetNeighbourChunk(chunkData, Vector2Int.down).Blocks[x, y, ChunkConstants.LENGTH - 1]
                            : chunkData.Blocks[x, y, z - 1];

                        if (topBlockType == EBlockType.Air)
                        {
                            AddFace(
                                new Vector3(x, y_, z),
                                new Vector3(x, y_, z_),
                                new Vector3(x_, y_, z_),
                                new Vector3(x_, y_, z),
                                Vector3.up,
                                ref vertices, ref triangles, ref uv, ref normals);
                        }

                        if (bottomBlockType == EBlockType.Air)
                        {
                            AddFace(
                                new Vector3(x, y, z_),
                                new Vector3(x, y, z),
                                new Vector3(x_, y, z),
                                new Vector3(x_, y, z_),
                                Vector3.down,
                                ref vertices, ref triangles, ref uv, ref normals);
                        }

                        if (leftBlockType == EBlockType.Air)
                        {
                            AddFace(
                                new Vector3(x, y, z_),
                                new Vector3(x, y_, z_),
                                new Vector3(x, y_, z),
                                new Vector3(x, y, z),
                                Vector3.left,
                                ref vertices, ref triangles, ref uv, ref normals);
                        }

                        if (rightBlockType == EBlockType.Air)
                        {
                            AddFace(
                                new Vector3(x_, y, z),
                                new Vector3(x_, y_, z),
                                new Vector3(x_, y_, z_),
                                new Vector3(x_, y, z_),
                                Vector3.right,
                                ref vertices, ref triangles, ref uv, ref normals);
                        }

                        if (frontBlockType == EBlockType.Air)
                        {
                            AddFace(
                                new Vector3(x_, y, z_),
                                new Vector3(x_, y_, z_),
                                new Vector3(x, y_, z_),
                                new Vector3(x, y, z_),
                                Vector3.forward,
                                ref vertices, ref triangles, ref uv, ref normals);
                        }

                        if (backBlockType == EBlockType.Air)
                        {
                            AddFace(
                                new Vector3(x, y, z),
                                new Vector3(x, y_, z),
                                new Vector3(x_, y_, z),
                                new Vector3(x_, y, z),
                                Vector3.back,
                                ref vertices, ref triangles, ref uv, ref normals);
                        }
                    }
                }
            }

            chunkData.Mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uv.ToArray(),
                normals = normals.ToArray(),
                triangles = triangles.ToArray()
            };
        }

        private void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal,
            ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs, ref List<Vector3> normals)
        {
            var index = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 0);
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));
            uvs.Add(new Vector2(1.0f, 1.0f));
            uvs.Add(new Vector2(1.0f, 0.0f));
        }

        private ChunkData GetNeighbourChunk(ChunkData chunkData, Vector2Int direction)
        {
            var neighbourChunkIndex = chunkData.Index + direction;

            var neighbourChunk = _allChunks.FirstOrDefault(entry => entry.Index == neighbourChunkIndex);

            if (neighbourChunk == null)
            {
                neighbourChunk = GenChunk(neighbourChunkIndex);
                _allChunks.Add(neighbourChunk);
            }

            return neighbourChunk;
        }
    }
}