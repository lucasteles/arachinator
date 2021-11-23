using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyWall : MonoBehaviour
{
    [SerializeField] AudioClip sound;
    [SerializeField] float effectStep = .4f;

    Material material;
    static readonly int Fresnel = Shader.PropertyToID("_FresnelPower");

    void Awake()
    {
        gameObject.SetActive(false);
        var renderer = GetComponent<Renderer>();
        material = new Material(renderer.sharedMaterial);
        renderer.sharedMaterial = material;
    }

    void OnEnable()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(sound);
        StartCoroutine(Fadein());
    }

    void OnDisable()
    {
        if (material == null) return;
        CameraAudioSource.Instance.AudioSource.PlayOneShot(sound);
    }

    IEnumerator Fadein()
    {
        for (var i = 0f; i <= 3; i+=effectStep)
        {
            material.SetFloat(Fresnel, i);
            yield return null;
        }
    }
    IEnumerator Fadeout()
    {
        for (var i = 3f; i >= 0; i-=effectStep)
        {
            material.SetFloat(Fresnel, i);
            yield return null;
        }
    }
}
