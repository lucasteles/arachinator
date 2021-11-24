using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cooldown
{
    [SerializeField]float cooldownTime;
    float timer;
    public Cooldown(float cooldownTime)
    {
        this.cooldownTime = cooldownTime;
        timer = Time.time;
    }

    public bool CanDo() => Time.time >= timer;

    public void Reset() => timer = Time.time + cooldownTime;

    public static implicit operator bool(Cooldown c)
        => c.CanDo();
}
