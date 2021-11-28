using System;
using UnityEngine;

namespace Assets.Scripts.Ui.WebTutorial
{
    public class WebTutorial : MonoBehaviour
    {
        Canvas canvas;
        void Awake() => canvas = GetComponent<Canvas>();

        public void Show() =>
            canvas.enabled = true;

        void Update()
        {
            if (!canvas.enabled) return;
            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.Space))
                canvas.enabled = false;
        }
    }
}