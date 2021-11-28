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
        var currentEmission = waspEyeClosed.GetColor(EmissionColor);
        var targetColor = waspEye.color;
        var targetEmission = waspEye.GetColor(EmissionColor);

        for (var i = 0f; i <= 1; i+=speed)
        {
            currentMaterial.color = Color.Lerp(currentColor, targetColor, i);
            currentMaterial.SetColor(EmissionColor, Color.Lerp(currentEmission, targetEmission, i));
            yield return null;
        }
        renderer.sharedMaterial = waspEye;
        yield return new WaitForSeconds(.8f);
    }


    public IEnumerator CloseEyes()
    {
        var currentColor = waspEye.color;
        var currentEmission = waspEye.GetColor(EmissionColor);
        var targetColor = waspEyeClosed.color;
        var targetEmission = waspEyeClosed.GetColor(EmissionColor);

        for (var i = 0f; i <= 1; i+=speed)
        {
            currentMaterial.color = Color.Lerp(currentColor, targetColor, i);
            currentMaterial.SetColor(EmissionColor, Color.Lerp(currentEmission, targetEmission, i));
            yield return null;
        }
        renderer.sharedMaterial = waspEyeClosed;
        yield return new WaitForSeconds(.8f);
    }
}
