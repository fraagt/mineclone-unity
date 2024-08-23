using ScriptableObjects.Data;
using UnityEngine;
using Utils;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = AssetMenuNames.ScriptableObjects + "/PrefabsDatabase", fileName = "PrefabsDatabase", order = 0)]
    public class PrefabsDatabaseSO : ScriptableObject
    {
        [SerializeField] private PrefabsDatabaseVo prefabsDatabaseVo;

        public PrefabsDatabaseVo PrefabsDatabaseVo => prefabsDatabaseVo;
    }
}