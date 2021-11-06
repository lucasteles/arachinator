using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Spring {
    public float Strength { get; set; }
    public float Damper { get; set; }
    public float Target { get; set; }
    public float Velocity { get; set; }
    public float Value { get; set; }

    public void Update(float deltaTime) {
        var direction = Target - Value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(Target - Value) * Strength;
        Velocity += (force * direction - Velocity * Damper) * deltaTime;
        Value += Velocity * deltaTime;
    }

    public void Reset() {
        Velocity = 0f;
        Value = 0f;
    }

}
