using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [Header("Scene Movements")]
    public string previousScene;
    public bool useEntrances = true;
    private RoomEntrance entrance;

    [Header("Dying")]
    public Vector3 respawnPosition;
    public GameObject blackScreen;
    public float blackScreenLerpDuration = 0.1f;
    public float blackScreenDuration = 0.75f;

    private void Start()
    {
        previousScene = GameData.Instance.previousSceneName;

        if (useEntrances)
        {
            FindEntranceAndEnter();
        }
        useEntrances = true;
    }

    private void FindEntranceAndEnter()
    {
        //Making a list of all entrances
        List<Transform> roomEntrances = GameObject.Find("Entrances").GetComponentsInChildren<Transform>().ToList<Transform>();
        roomEntrances.Remove(roomEntrances[0]); //Remove parent element (L unity bro literally why tf)

        if (roomEntrances.Count == 0)
        {
            return;
        }

        //print(SceneManager.GetActiveScene().name);
        if (previousScene == "") //No listed previous scene for some reason
        {
            //Checking if any entrances are the cave entrance
            foreach (Transform entranceTransform in roomEntrances)
            {
                RoomEntrance currentEntrance = entranceTransform.GetComponent<RoomEntrance>();

                if (currentEntrance.caveStart == true)
                {
                    currentEntrance.checkForPlayer = false;
                    gameObject.transform.position = currentEntrance.gameObject.transform.position;
                    currentEntrance.Enter(gameObject);
                    Destroy(currentEntrance.gameObject, 2);

                    return;
                }
            }

            //If not
            RoomEntrance entrance = roomEntrances[0].GetComponent<RoomEntrance>(); //defaults to first element in list

            //Enters the room
            entrance.checkForPlayer = false;
            gameObject.transform.position = entrance.gameObject.transform.position;
            entrance.Enter(gameObject);

            return;
        }

        //Checking each entrance for matching connected scenes
        foreach (Transform entranceTransform in roomEntrances)
        {
            RoomEntrance entrance = entranceTransform.GetComponent<RoomEntrance>();

            if (entrance.caveStart == true)
            {
                continue;
            }

            if (entrance.connectedSceneName == previousScene)
            {
                entrance.checkForPlayer = false;
                gameObject.transform.position = entrance.gameObject.transform.position;
                entrance.Enter(gameObject);

                break;
            }
        }
    }

    public IEnumerator Die(Vector2 playerVelocity)
    {

        PlayerMovement playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerMovement.SetManualVelocity(playerVelocity, blackScreenLerpDuration, false);

        blackScreen.SetActive(true);

        blackScreen.LeanAlpha(1, blackScreenLerpDuration);
        yield return new WaitForSeconds(blackScreenDuration - 0.1f);

        transform.position = respawnPosition;
        playerMovement.SetManualVelocity(new Vector2(0, 0), blackScreenLerpDuration, false);
        yield return new WaitForSeconds(0.1f);

        blackScreen.LeanAlpha(0, blackScreenLerpDuration);
        playerMovement.EnableMovement();
    }

    #region Save and Load

    public void Save(ref PlayerSaveData data)
    {
        data.sceneName = SceneManager.GetActiveScene().name;
    }

    public void Load(PlayerSaveData data)
    {
        SceneManager.LoadScene(data.sceneName);
        GameData.Instance.previousSceneName = "";
        print(GameData.Instance.previousSceneName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SaveSystem.Save();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SaveSystem.Load();
        }
    }

    #endregion
}

[System.Serializable]
public struct PlayerSaveData
{
    public string sceneName;
}
