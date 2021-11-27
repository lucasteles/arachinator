using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ShinyMaterial : MonoBehaviour
{
    [SerializeField]float step;
    [SerializeField]float power;
    [SerializeField]Material currentMaterial;
    [SerializeField]Color onColor;
    [SerializeField]Color offColor;
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        currentMaterial.color = offColor;
    }

    public void Shiny()
    {
        StartCoroutine(ShinyCoroutine());
    }

    IEnumerator ShinyCoroutine()
    {
        var color = currentMaterial.GetVector(EmissionColor);
        currentMaterial.color = onColor;
        for (var k = 0; k < 3; k++)
        for (var i = 0f; i < 1; i+=step)
        {
            currentMaterial.SetVector(EmissionColor, color * Mathf.Lerp(1, power, Utils.SimpleCurve(i)));
            yield return null;
        }


    }
}

