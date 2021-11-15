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
    [SerializeField] float maxLook = 6;
    Camera camera;
    Vector3 velocity = Vector3.zero;

    LookAtMouse player;
    void Awake()
    {
        camera = GetComponentInChildren<Camera>();
        player = target.GetComponent<LookAtMouse>();
    }

    void FixedUpdate()
    {
        var mouseAimOffset = (player.CurrentMousePosition - target.position) / 2f;

        if (Vector3.SqrMagnitude(mouseAimOffset) >= Math.Pow(maxLook, 2))
            mouseAimOffset = mouseAimOffset.normalized * maxLook;

        var targetPosition = target.position + mouseAimOffset;
        var screenCenterOffset= Vector3.zero;
        var ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, floor))
          screenCenterOffset = targetPosition - new Vector3(hit.point.x, 0, hit.point.z);

        var cameraDistance = Vector3.up * currentCameraDistance;
        var desiredPosition = targetPosition + screenCenterOffset + offset + cameraDistance;

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
