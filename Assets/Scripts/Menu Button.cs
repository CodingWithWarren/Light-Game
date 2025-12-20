using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    PlayerSaveData playerSaveData;

    private Image blackScreen;
    private float blackScreenLerpTime = 1;
    public string defaultSceneName;

    private void Awake()
    {
        playerSaveData = SaveSystem.LoadForMenu();
        blackScreen = transform.parent.Find("Black Screen").GetComponent<Image>();
    }

    public void StartGame()
    {
        StartCoroutine(LoadIntoScene());
    }

    private IEnumerator LoadIntoScene()
    {
        blackScreen.gameObject.SetActive(true);
        LeanTween.value(0, 1, blackScreenLerpTime)
            .setOnUpdate((float currentValue) =>
            {
                Color color = blackScreen.color;
                color.a = currentValue;
                blackScreen.color = color;
            });


        yield return new WaitForSeconds(blackScreenLerpTime);

        if (playerSaveData.sceneName != "")
        {
            SceneManager.LoadScene(playerSaveData.sceneName);
        }
        else
        {
            SceneManager.LoadScene(defaultSceneName);
        }
    }
}
