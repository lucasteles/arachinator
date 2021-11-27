using UnityEngine;
using UnityEngine.SceneManagement;  

public class MainMenu : MonoBehaviour
{
    public void PlayGame() => SceneManager.LoadScene("level-1");
    public void CloseGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}