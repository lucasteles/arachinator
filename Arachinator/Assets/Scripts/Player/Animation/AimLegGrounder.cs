using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLegGrounder : MonoBehaviour
{
    [SerializeField]LayerMask mask;
    [SerializeField]GameObject origin;

    Vector3 backupPos;
    bool isKinnematic;
    void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        if (isKinnematic) return;
        if(Physics.Raycast(transform.position, -transform.up, out var hit, 2f, mask))
            transform.position =  new Vector3(transform.position.x, hit.point.y + 0.2f, transform.position.z);
    }

    public void EnableKinematic()
    {
        backupPos = transform.localPosition;
        isKinnematic = true;
    }

    public void DisableKinematic()
    {
        transform.localPosition = backupPos;
        isKinnematic = false;
    }
}
