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
        var renderer = GetComponent<Renderer>();
        material = new Material(renderer.sharedMaterial);
        renderer.sharedMaterial = material;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(sound);
        StartCoroutine(Fadein());
    }

    IEnumerator Fadein()
    {
        for (var i = 0f; i <= 3; i+=effectStep)
        {
            material.SetFloat(Fresnel, i);
            yield return null;
        }
    }
}
