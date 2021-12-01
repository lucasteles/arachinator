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

        public void Hide()
        {
            if (!canvas.enabled) return;
            canvas.enabled = false;
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
        }
    }
}