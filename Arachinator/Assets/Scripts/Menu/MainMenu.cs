using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        var asyncOperation = SceneManager.LoadSceneAsync("level-1");
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            LoadingText.text = "Loading progress: " + Mathf.Round(asyncOperation.progress * 100) + "%";
            if (asyncOperation.progress >= 0.9f)
            {
                if (Enviroment.IsMobile)
                    asyncOperation.allowSceneActivation = true;
                else
                {
                    LoadingText.text = "Press any key to start";
                    var myAction = new InputAction(binding: "/*/<button>");
                    myAction.performed += (context) => asyncOperation.allowSceneActivation = true;
                    myAction.Enable();
                }
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