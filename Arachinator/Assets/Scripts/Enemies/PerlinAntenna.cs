using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PerlinAntenna : MonoBehaviour
{
    [SerializeField] Vector3 minRotation;
    [SerializeField] Vector3 maxRotation;
    [Range(0,1)] [SerializeField] float speed;

    [SerializeField] Quaternion defaultRot;
    bool next = true;
    float seed;

    void Awake()
    {
        defaultRot = transform.localRotation;
        seed = minRotation.magnitude + maxRotation.magnitude;
    }

    void Update()
    {
        if (!next) return;
        next = false;

       StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        var currentRotation = transform.localRotation;
        var perlinFactor =
            Mathf.PerlinNoise(Time.time * seed, Time.time * seed) * (maxRotation - minRotation)
            + minRotation;

        var vectorRot = new Vector3(
            perlinFactor.x == 0f ? defaultRot.eulerAngles.x : Mathf.Clamp(perlinFactor.x, minRotation.x, maxRotation.x),
            perlinFactor.y == 0f ? defaultRot.eulerAngles.y : Mathf.Clamp(perlinFactor.y, minRotation.y, maxRotation.y),
            perlinFactor.z == 0f ? defaultRot.eulerAngles.z : Mathf.Clamp(perlinFactor.z, minRotation.z, maxRotation.z));

        var nextRotation = Quaternion.Euler(vectorRot);

        for (var i = 0f; i < 1f; i+=speed)
        {
            transform.localRotation = Quaternion.Lerp(currentRotation, nextRotation, i);
            yield return null;
        }

        next = true;
    }
}

