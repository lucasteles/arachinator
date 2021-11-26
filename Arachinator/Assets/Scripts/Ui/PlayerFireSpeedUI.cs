using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Ui.PlayerFireSpeedUI
{
    public class PlayerFireSpeedUI : MonoBehaviour
    {
        [SerializeField] float currentSpeed;
        RawImage[] speedIcons;

        private void UpdateIcons()
        {
            for (int i = 0; i < speedIcons.Length; i++)
            {
                speedIcons[i].enabled = i < Mathf.RoundToInt(currentSpeed);
            }
        }
    
        public void SetCurrentSpeed(float fireSpeed)
        {
            currentSpeed = fireSpeed;
            UpdateIcons();
        }

        private void Awake() =>
            speedIcons = GetComponentsInChildren<RawImage>();

        private void Start() => 
            UpdateIcons();
    }
}