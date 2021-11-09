using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 0.15F;
    [SerializeField] Vector3 offset;

    [Header("Limites")]
    [SerializeField] bool enableBoundary = false;
    [SerializeField] float maxHorizontal = 100f;
    [SerializeField] float minHorizontal = -100f;
    [SerializeField] float maxVertical = 100f;
    [SerializeField] float minVertical = -100f;

    Vector3 velocity = Vector3.zero;
    void FixedUpdate()
    {
       var desiredPosition = target.position + offset;

       if(enableBoundary)
       {
           desiredPosition.x = Mathf.Clamp(desiredPosition.x, minHorizontal, maxHorizontal);
           desiredPosition.z = Mathf.Clamp(desiredPosition.z, minVertical, maxVertical);
       }

       transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
