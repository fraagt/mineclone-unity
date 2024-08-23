using Enums;
using UnityEngine;

namespace Behaviours
{
    public class ChunkBehaviour : MonoBehaviour
    {
        [SerializeField] private new Transform transform;
        [SerializeField] private new GameObject gameObject;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Transform cubePrefab;

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
        }

        public void SpawnCubes(EBlockType[,,] chunkDataBlocks)
        {
            for (var y = 0; y < ChunkConstants.HEIGHT; ++y)
            {
                for (var x = 0; x < ChunkConstants.LENGTH; ++x)
                {
                    for (var z = 0; z < ChunkConstants.LENGTH; ++z)
                    {
                        if (chunkDataBlocks[x, y, z] != EBlockType.Air)
                        {
                            var trans = Instantiate(cubePrefab, transform.position + new Vector3(x, y, z),
                                Quaternion.identity, transform);
                        }
                    }
                }
            }
        }

        public void Despawn()
        {
            foreach (Transform child in transform)
            {
                if (child.name != "Mesh")
                    Destroy(child.gameObject);
            }
        }
    }
}