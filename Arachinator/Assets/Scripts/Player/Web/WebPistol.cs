using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WebPistol : MonoBehaviour
{
    [SerializeField]Transform shotPoint;
    [SerializeField]float maxDistance;
    [SerializeField]int quality;
    [SerializeField]int dumper;
    [SerializeField]int velocity;
    [SerializeField]int strength;
    [SerializeField]int waveCount;
    [SerializeField]int waveHeight;
    [SerializeField]AnimationCurve curve;


    Spring spring;
    LineRenderer lineRenderer;

    Vector3? hitPosition = null;
    Vector3 currentGraplingPosition;
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.SetPositions(Array.Empty<Vector3>());
        spring = new Spring();
        spring.Value = 0;
    }

    void Hide()
    {
        lineRenderer.positionCount = 0;
        spring.Reset();
        hitPosition = null;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
            StartWeb();
    }

    void LateUpdate()
    {
        DrawWeb();
    }

    void StartWeb()
    {
        if (Physics.Raycast(shotPoint.position, -shotPoint.forward, out var hit, maxDistance))
        {
            hitPosition = hit.point;
            //Invoke(nameof(Hide), 1f);
        }

    }

    void DrawWeb()
    {
        if (!hitPosition.HasValue)
            return;

        print("hope");
        spring.Damper = dumper;
        spring.Strength = strength;
        spring.Velocity = velocity;
        spring.Update(Time.deltaTime);

        var shotPointPos = shotPoint.position;
        var up = Quaternion.LookRotation((hitPosition.Value - shotPointPos).normalized) * Vector3.up;
        currentGraplingPosition = Vector3.Lerp(shotPoint.position, hitPosition.Value, 12 * Time.deltaTime);

        lineRenderer.positionCount = quality + 1;
        for (var i = 0; i <= quality; i++)
        {
            var delta = 1 / (float)quality;
            var offset = up * waveHeight *
                         Mathf.Sin(delta * waveCount * Mathf.PI)
                         * spring.Value * curve.Evaluate(delta);
            lineRenderer.SetPosition(i, Vector3.Lerp(shotPointPos,  currentGraplingPosition, delta) + offset);
        }
    }
}
