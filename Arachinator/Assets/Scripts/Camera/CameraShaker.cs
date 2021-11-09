using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Cameras.Effects
{
    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance;

        Vector3 originalPosition;
        float timeShaking;
        Camera mainCamera;
        CameraShakeData data;

        void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);

            Instance = this;
            mainCamera = Camera.main;
            originalPosition = mainCamera.transform.localPosition;
        }

        void Update()
        {
            if (timeShaking >= data.timeToShake) return;

            timeShaking += Time.unscaledDeltaTime;

            var x = Random.Range(-1, 2) * data.magnitude ;
            var y = Random.Range(-1, 2) * data.magnitude ;

            var newPos = originalPosition + new Vector3(x, y);
            mainCamera.transform.localPosition = newPos;
            data.DecreaseMagnitude();

            if (timeShaking >= data.timeToShake)
            {
                mainCamera.transform.localPosition = originalPosition;
                timeShaking = 0;
                data.timeToShake = 0;
            }
        }

        public void Shake(CameraShakeData data)
        {
            timeShaking = 0;
            this.data = data;
        }
    }

    [Serializable]
    public struct CameraShakeData
    {
        private float declineRate;
        public float timeToShake;
        public float magnitude;

        private float DeclineRate
        {
            get
            {
                if (declineRate == 0)
                    declineRate = magnitude / timeToShake;

                return declineRate;
            }
        }

        public void DecreaseMagnitude()
        {
            magnitude -= DeclineRate * Time.deltaTime;
        }
    }
}