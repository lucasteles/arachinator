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

    Spring spring;
    LineRenderer lineRenderer;

    Vector3? hitPosition = null;

    SpringJoint springJoint;


    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.SetPositions(Array.Empty<Vector3>());
        spring = new Spring();
    }

    void Hide()
    {
        lineRenderer.positionCount = 0;
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
            //Invoke(nameof(Hide), .5f);
        }

    }

    void DrawWeb()
    {
        if (!hitPosition.HasValue)
            return;

        var to = Vector3.Lerp(shotPoint.position, hitPosition.Value, 1);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, shotPoint.position);
        lineRenderer.SetPosition(1, to);
    }
}
