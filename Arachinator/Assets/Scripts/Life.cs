using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] float maxLife;
    float currentLife;
    bool dead;

    public float CurrentLife => currentLife;
    public bool IsDead => dead;
    public event Action onDeath;

    void Start()
    {
        currentLife = maxLife;
        dead = false;
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

    public void Subtract(float amount) => currentLife -= amount;
    public void Add(float amount) => currentLife += amount;

}