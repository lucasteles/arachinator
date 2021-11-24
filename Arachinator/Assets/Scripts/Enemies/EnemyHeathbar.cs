using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class EnemyHeathbar : MonoBehaviour
{
    [SerializeField] float timeToChangeSlider;
    [SerializeField] Life life;
    [SerializeField] CanvasGroup canvas;
    float timeChangingSlider;

    float maxHealth;
    float currentHealth;
    Slider slider;
    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        canvas.alpha = 0;
        maxHealth = life.MaxLife;
        currentHealth = life.CurrentLife;
        slider.maxValue = maxHealth;
        life.onLifeChange += OnLifeChange;
    }
    void OnDestroy() => life.onLifeChange -= OnLifeChange;

    void OnLifeChange(float currentLife, float maxLife)
    {
        StopAllCoroutines();
        currentHealth = currentLife;
        slider.maxValue = maxHealth = maxLife;
        StartCoroutine(Show());
    }
    IEnumerator Show()
    {
        var speed = .05f;
        for (var i = canvas.alpha; i <= 1; i+=speed)
        {
            canvas.alpha = i;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        for (var i = canvas.alpha; i >= 0; i-=speed)
        {
            canvas.alpha = i;
            yield return null;
        }
    }

    void Update()
    {
        if (slider.value == currentHealth) return;
        timeChangingSlider += Time.deltaTime;
        this.slider.value = Mathf.Lerp(this.slider.value, this.currentHealth, timeChangingSlider / timeToChangeSlider);
    }
}
