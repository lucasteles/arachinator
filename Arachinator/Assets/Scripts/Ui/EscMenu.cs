using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscMenu : MonoBehaviour
{
    Canvas canvas;
    public void OnTriggerMenu(InputAction.CallbackContext context)
    {
        canvas.enabled = !canvas.enabled;

        if (!canvas.enabled && Time.timeScale < 1)
            Time.timeScale = 1f;
        else
            Time.timeScale = .2f;
    }

    void Awake() => canvas = GetComponent<Canvas>();
    void Update()
    {
    }
    public void BackToMenu() => SceneManager.LoadSceneAsync("MainMenu");
}