using ScriptableObjects.Data;
using UnityEngine;
using Utils;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = AssetMenuNames.ScriptableObjects + "/Settings", fileName = "Settings", order = 0)]
    public class SettingsSO : ScriptableObject
    {
        [SerializeField] private SettingsVo settingsVo;

        public SettingsVo SettingsVo => settingsVo;
    }
}