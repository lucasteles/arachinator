using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private TextMeshProUGUI LoadingText;

    public void PlayGame()
    {
        GetComponent<Canvas>().enabled = false;
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("level-1");
        asyncOperation.allowSceneActivation = false;
        
        while (!asyncOperation.isDone)
        {
            LoadingText.text = "Loading progress: " + Mathf.Round(asyncOperation.progress * 100) + "%";
            if (asyncOperation.progress >= 0.9f)
            {
                LoadingText.text = "Press any key to start";
                if (Input.anyKey)
                    asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public void CloseGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}