using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WebPistol : MonoBehaviour
{
    [SerializeField]Transform shotPoint;
    [SerializeField]float maxDistance;
    [SerializeField]float coodownTime;
    [SerializeField]Rigidbody rigidybody;
    [SerializeField]Movement movement;
    [SerializeField]float impulseForce;
    Cooldown cooldown;
    Vector3? hitPosition = null;
    public Vector3 ShotPoint => shotPoint.position;
    public Vector3? Target => hitPosition;

    void Start()
    {
        cooldown = new Cooldown(coodownTime);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2") && cooldown)
        {
            StartWeb();
            cooldown.Reset();
        }
    }

    void StartWeb()
    {
        if (Physics.Raycast(shotPoint.position, -shotPoint.forward, out var hit, maxDistance))
        {
            hitPosition = hit.point;
            Invoke(nameof(Hide), 0.3f);
            movement.Lock(.2f);
            rigidybody.velocity = Vector3.zero;
            rigidybody.AddForce(impulseForce * -transform.forward);
        }

    }

    void Hide()
    {
        hitPosition = null;
    }

    public bool TargetDefined() => hitPosition.HasValue;
}
