using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetleTrackeEvent : MonoBehaviour
{
    public event Action onTracke;
    public void OnTracke() => onTracke?.Invoke();
}
