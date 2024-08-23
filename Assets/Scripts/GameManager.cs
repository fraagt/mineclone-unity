using Data;
using ScriptableObjects;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PrefabsDatabaseSO prefabsDatabaseSO;
    [SerializeField] private SettingsSO settingsSO;

    private void Awake()
    {
        InitGameData();
        LoadGame();
    }

    private void InitGameData()
    {
        GameData.Settings = settingsSO.SettingsVo.ToSettings();
        GameData.PrefabsDatabase = prefabsDatabaseSO.PrefabsDatabaseVo.ToPrefabsDatabase();
        GameData.Player = new Player
        {
            Pose = new Pose(Vector3.zero + Vector3.up * 50.0f, Quaternion.identity)
        };
        GameData.World = new World();
    }

    private void LoadGame()
    {
        var prefabsDatabase = GameData.PrefabsDatabase;
        Instantiate(prefabsDatabase.PlayerBehaviour);
        Instantiate(prefabsDatabase.WorldBehaviour);
    }
}