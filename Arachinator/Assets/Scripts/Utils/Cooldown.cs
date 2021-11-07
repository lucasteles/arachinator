using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooldown
{
    float cooldown;
    float timer;
    public Cooldown(float cooldown)
    {
        this.cooldown = cooldown;
        timer = Time.time;
    }

    public bool CanDo() => Time.time >= timer;

    public void Reset() => timer = Time.time + cooldown;

    public static implicit operator bool(Cooldown c)
        => c.CanDo();
}
