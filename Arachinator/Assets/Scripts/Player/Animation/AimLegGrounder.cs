using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLegGrounder : MonoBehaviour
{
    [SerializeField]LayerMask mask;
    [SerializeField]GameObject origin;

    void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        if(Physics.Raycast(origin.transform.position, -transform.up, out var hit, 2f, mask))
            transform.position = hit.point + new Vector3(0f, 0.2f, 0f);
    }
}
