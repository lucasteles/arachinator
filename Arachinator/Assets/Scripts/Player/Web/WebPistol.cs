using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WebPistol : MonoBehaviour
{
    [SerializeField]Transform shotPoint;
    [SerializeField]float maxDistance;
    Vector3? hitPosition = null;
    public Vector3 ShotPoint => shotPoint.position;
    public Vector3? Target => hitPosition;

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
            StartWeb();
    }

    void StartWeb()
    {
        if (Physics.Raycast(shotPoint.position, -shotPoint.forward, out var hit, maxDistance))
        {
            hitPosition = hit.point;
            Invoke(nameof(Hide), 1f);
        }

    }

    void Hide()
    {
        hitPosition = null;
    }

    public bool TargetDefined() => hitPosition.HasValue;
}
