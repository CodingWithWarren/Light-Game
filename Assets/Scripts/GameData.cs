using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    [Header("Player")]
    public string previousSceneName;

    [Header("Environment")]
    public Dictionary<string, List<FlareData>> flarePositionDictionary { get; private set; } = new Dictionary<string, List<FlareData>>();
    public GameObject flarePrefab;

    private void Awake()
    {
        //Singleton stuff
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SaveSystem.Save();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SaveSystem.Load();
        }
    }

    public void SaveFlarePositionsForCurrentScene()
    {
        //value for dictionary
        List<FlareData> flareDataList = new List<FlareData>();

        //getting all objects under the parent "flares"
        Transform[] allFlares = GameObject.Find("Flares").GetComponentsInChildren<Transform>();

        //iterating through every object in allFlares
        foreach (Transform flare in allFlares)
        {
            if (flare.gameObject.layer == LayerMask.NameToLayer("Flare")) //checks whether the object is actually a flare (L unity)
            {
                //create the FlareData object
                FlareData flareDataObject = new FlareData(flare.position, flare.rotation);
                flareDataList.Add(flareDataObject);
            }
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        flarePositionDictionary[currentSceneName] = flareDataList;
    }

    public void LoadSavedFlares()
    {
        foreach (string name in flarePositionDictionary.Keys)
        {
            if (name == SceneManager.GetActiveScene().name)
            {
                foreach (FlareData flareData in flarePositionDictionary[name])
                {
                    Instantiate(flarePrefab, flareData.position, flareData.rotation, GameObject.Find("Flares").GetComponent<Transform>());
                }
            }
        }
    }

    #region Save and Load

    public void Save(ref EnvironmentData data)
    {
        //make sure all flare positions are up to date
        SaveFlarePositionsForCurrentScene();

        //instantiate a pseudo dictionary for the JSON file
        List<FlareDictionaryObject> flareDictionaryObjectsList = new List<FlareDictionaryObject>();

        //populate the pseudo dictionary with values from the actual dictionary
        foreach (KeyValuePair<string, List<FlareData>> entry in flarePositionDictionary)
        {
            FlareDictionaryObject newElement = new FlareDictionaryObject(SceneManager.GetActiveScene().name, entry.Value);

            flareDictionaryObjectsList.Add(newElement);
        }

        //save it to saveData
        data.flarePositions = flareDictionaryObjectsList;
        print(data.flarePositions);
    }

    public void Load(EnvironmentData data)
    {
        //make sure dictionary in code is empty to avoid errors
        flarePositionDictionary.Clear();

        //populate the dictionary
        foreach (FlareDictionaryObject flareDictObject in data.flarePositions)
        {
            flarePositionDictionary.Add(flareDictObject.sceneName, flareDictObject.flareData);
        }
    }
    #endregion

    #region On Application Quit

    private void OnApplicationQuit()
    {
        SaveFlarePositionsForCurrentScene();
        SaveSystem.Save();
    }

    #endregion
}

#region Custom Classes for Saving and Loading
[System.Serializable]
public struct EnvironmentData
{
    public List<FlareDictionaryObject> flarePositions;
}

[System.Serializable]
public class FlareDictionaryObject
{
    public string sceneName;
    public List<FlareData> flareData;

    public FlareDictionaryObject(string sceneName, List<FlareData> flareData)
    {
        this.sceneName = sceneName;
        this.flareData = flareData;
    }
}

[System.Serializable]
public class FlareData
{
    public Vector3 position;
    public Quaternion rotation;

    public FlareData(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
#endregion