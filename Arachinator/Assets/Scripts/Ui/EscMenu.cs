using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscMenu : MonoBehaviour
{
    Canvas canvas;
    [SerializeField] private Button NoButton;
    public void OnTriggerMenu(InputAction.CallbackContext context)
    {
        NoButton.Select();
        canvas.enabled = !canvas.enabled;
        Time.timeScale = .2f;
    }

    void Awake() => canvas = GetComponent<Canvas>();
    void Update()
    {
        if (!canvas.enabled && Time.timeScale < 1)
            Time.timeScale = 1f;
    }
    public void BackToMenu() => SceneManager.LoadSceneAsync("MainMenu");
}