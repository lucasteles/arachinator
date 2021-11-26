using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspEye : MonoBehaviour
{
    [SerializeField] Material waspEye;
    [SerializeField] Material waspEyeClosed;
    [SerializeField] float speed;

    Renderer renderer;
    Material currentMaterial;
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        renderer.sharedMaterial = currentMaterial = new Material(waspEyeClosed);
    }

    public IEnumerator OpenEyes()
    {
        var currentColor = waspEyeClosed.color;
        var currentEmission = waspEyeClosed.GetVector(EmissionColor);
        var targetColor = waspEye.color;
        var targetEmission = waspEye.GetVector(EmissionColor);

        yield return new WaitForSeconds(.8f);
        for (var i = 0f; i <= 1; i+=speed)
        {
            currentMaterial.color = Color.Lerp(currentColor, targetColor, i);
            currentMaterial.SetVector(EmissionColor, Vector4.Lerp(currentEmission, targetEmission, i));
            yield return null;
        }
        renderer.sharedMaterial = waspEye;
    }


}
