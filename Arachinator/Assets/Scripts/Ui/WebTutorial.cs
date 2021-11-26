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
            if (Input.GetButton("Fire1") || Input.GetButton("Fire2"))
                canvas.enabled = false;
        }
    }
}