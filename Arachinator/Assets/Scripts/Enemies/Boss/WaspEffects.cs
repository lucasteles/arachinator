using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class WaspEffects : MonoBehaviour
{
    WaspEye[] eyes;

    void Awake()
    {
        eyes = GetComponentsInChildren<WaspEye>();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator OpenEyes()
    {
        var routiness = eyes
            .Select(eye => StartCoroutine(eye.OpenEyes()))
            .ToArray();

        foreach (var r in routiness)
            yield return r;
    }
}
