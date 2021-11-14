using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAnimationEvents : MonoBehaviour
{
    public event Action ShotLeftEvent;
    public event Action ShotRightEvent;

    public void ShotLeft() => ShotLeftEvent?.Invoke();
    public void ShotRight() => ShotRightEvent?.Invoke();
}
