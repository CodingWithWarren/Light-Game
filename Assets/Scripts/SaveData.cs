using UnityEngine;
using System.IO;
using Unity.VisualScripting.FullSerializer;
using Unity.VisualScripting;

public class SaveSystem
{
    private static SaveData _saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData PlayerData;
        public EnvironmentData EnvironmentData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        Debug.Log(saveFile);
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData));
    }

    public static void Load()
    {
        string saveContents = File.ReadAllText(SaveFileName());

        _saveData = JsonUtility.FromJson<SaveData>(saveContents);
        HandleLoadData();
    }

    public static PlayerSaveData LoadForMenu()
    {
        string saveContents = File.ReadAllText(SaveFileName());
        _saveData = JsonUtility.FromJson<SaveData>(saveContents);
        return _saveData.PlayerData;
    }

    private static void HandleSaveData()
    {
        GameObject.Find("Player").GetComponent<PlayerManager>().Save(ref _saveData.PlayerData);
        GameData.Instance.Save(ref _saveData.EnvironmentData);
    }

    private static void HandleLoadData()
    {
        GameData.Instance.Load(_saveData.EnvironmentData);
        GameObject.Find("Player").GetComponent<PlayerManager>().Load(_saveData.PlayerData);
    }
}
