using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Ui.HealthPoints
{
    public class PlayerHealthPointsUi : MonoBehaviour
    {
        [SerializeField] float timeToChangeSlider;
        float timeChangingSlider;

        float maxHealth;
        float currentHealth;
        Slider slider;

        void Awake()
        {
            this.slider = GetComponent<Slider>();
        }

        public void SetMaxHealth(float maxHealth)
        {
            this.maxHealth = this.currentHealth = maxHealth;
            this.slider.maxValue = this.maxHealth;
        }

        void Update()
        {
            if (this.slider.value == currentHealth) return;
            timeChangingSlider += Time.deltaTime;
            this.slider.value = Mathf.Lerp(this.slider.value, this.currentHealth, timeChangingSlider / timeToChangeSlider);
        }

        public void SetHealth(float amount)
        {
            timeChangingSlider = 0;
            currentHealth = amount;
        }
    }
}
