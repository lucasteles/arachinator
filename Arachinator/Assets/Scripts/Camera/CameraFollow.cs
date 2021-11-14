using System;
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
    [SerializeField] LayerMask floor;
    [SerializeField] float maxCameraDistance = 10;
    [SerializeField] float zoomSpeed = 1;
    [SerializeField] float currentCameraDistance = 0;
    Camera camera;
    Vector3 velocity = Vector3.zero;
    void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    void FixedUpdate()
    {
        var screenCenterOffset= Vector3.zero;
        var ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, floor))
          screenCenterOffset = target.position - new Vector3(hit.point.x, 0, hit.point.z);

        var cameraDistance = Vector3.up * currentCameraDistance;
        var desiredPosition = target.position + screenCenterOffset + offset + cameraDistance;

       if(enableBoundary)
       {
           desiredPosition.x = Mathf.Clamp(desiredPosition.x, minHorizontal, maxHorizontal);
           desiredPosition.z = Mathf.Clamp(desiredPosition.z, minVertical, maxVertical);
       }

       currentCameraDistance -= Input.mouseScrollDelta.y * zoomSpeed;
       currentCameraDistance = Mathf.Clamp(currentCameraDistance, 0, maxCameraDistance);
       transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
