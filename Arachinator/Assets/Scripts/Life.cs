using System;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] float maxLife;
    float currentLife;
    bool dead;

    public float CurrentLife => currentLife;
    public bool IsDead => dead;
    public float MaxLife =>  maxLife;

    public event Action onDeath;
    public event Action<float, float> onLifeChange;

    void Start() => Reset();

    public void SetMaxLife(float val)
    {
        maxLife = val;
        InvokeEvent();
    }

    void Update()
    {
        if (currentLife <= 0 && !dead)
        {
            currentLife = 0;
            dead = true;
            onDeath?.Invoke();
        }
    }

    void InvokeEvent() => onLifeChange?.Invoke(currentLife, maxLife);

    public void Subtract(float amount)
    {
        currentLife -= amount;
        InvokeEvent();
    }

    public void Add(float amount)
    {
        currentLife += amount;
        InvokeEvent();
    }

    public void Reset()
    {
        currentLife = maxLife;
        dead = false;
        InvokeEvent();
    }

    public bool IsFull() => currentLife == maxLife;

    public void Die()
    {
        currentLife = 0;
    }
}
