using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscMenu : MonoBehaviour
{
    Canvas canvas;
    void Awake() => canvas = GetComponent<Canvas>();
    void Update()
    {
        if (!canvas.enabled && Time.timeScale < 1)
            Time.timeScale = 1f;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        canvas.enabled = !canvas.enabled;
        Time.timeScale = .2f;
    }
    public void BackToMenu() => SceneManager.LoadSceneAsync("MainMenu");
}