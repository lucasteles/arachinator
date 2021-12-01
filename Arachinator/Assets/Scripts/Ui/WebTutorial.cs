using System;
using UnityEngine;

namespace Assets.Scripts.Ui.WebTutorial
{
    public class WebTutorial : MonoBehaviour
    {
        Canvas canvas;
        void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        public void Show()
        {
            canvas.enabled = true;
            if (Environment.IsMobile)
                ActivateButtom.Instance.Show("OK!");
        }

        void Update()
        {
            if (!canvas.enabled) return;
            if (Environment.IsMobile)
            {
                if (!ActivateButtom.Instance.Pressed) return;
                ActivateButtom.Instance.Hide();
                canvas.enabled = false;
            }
            else
                if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0)
                    canvas.enabled = false;
        }
    }
}