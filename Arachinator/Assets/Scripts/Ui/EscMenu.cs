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
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        canvas.enabled = !canvas.enabled;
    }
    public void BackToMenu() => SceneManager.LoadSceneAsync("MainMenu");
}