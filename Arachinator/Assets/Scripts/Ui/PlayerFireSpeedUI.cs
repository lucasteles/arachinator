using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Ui.PlayerFireSpeedUI
{
    public class PlayerFireSpeedUI : MonoBehaviour
    {
        [SerializeField] float currentSpeed;
        RawImage[] speedIcons;
    
        public void SetCurrentSpeed(float fireSpeed)
        {
            currentSpeed = fireSpeed;
        }

        private void Awake() =>
            speedIcons = GetComponentsInChildren<RawImage>();

        void Update()
        {
            for (int i = 0; i < speedIcons.Length; i++)
            {
                speedIcons[i].enabled = i < Mathf.RoundToInt(currentSpeed);
            }
        }
    }
}