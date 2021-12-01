using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 0.15F;
    [SerializeField] Vector3 offset;

    [Header("Limites")]
    [SerializeField] LayerMask floor;
    [SerializeField] float maxCameraDistance = 10;
    [SerializeField] float zoomSpeed = 1;
    [SerializeField] float currentCameraDistance = 0;
    [SerializeField] float maxLook = 6;
    [SerializeField] bl_Joystick firestick;
    Camera myCamera;
    Vector3 velocity = Vector3.zero;

    bool isShooting;

    public void Shoot(InputAction.CallbackContext context)
    {
        isShooting = context.started;
    }

    LookAtMouse player;
    public bool IsLocket { get; set; }

    void Awake()
    {
        myCamera = GetComponentInChildren<Camera>();
        player = target.GetComponent<LookAtMouse>();
    }

    float mobileZoonSeed = 0;
    public void StartZoonIn() => mobileZoonSeed = -zoomSpeed;
    public void StartZoonOut() => mobileZoonSeed = zoomSpeed;
    public void StopZoon() => mobileZoonSeed = 0;

    void FixedUpdate()
    {

        var screenCenterOffset= Vector3.zero;
        var ray = myCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, floor))
          screenCenterOffset = new Vector3(hit.point.x, 0, hit.point.z);
        else
        {
            var plan = new Plane(Vector3.up, Vector3.up * -1);
            if (plan.Raycast(ray, out var dist))
            {
                var point = ray.GetPoint(dist);
                screenCenterOffset = new Vector3(point.x, 0, point.z);
            }
        }

        var cameraDistance = Vector3.up * currentCameraDistance;
        var targetPosition = target.position;

        if (Pressed() && !IsLocket)
        {
            var mouseAimOffset = (player.CurrentMousePosition - target.position) / 2f;
            if (Vector3.SqrMagnitude(mouseAimOffset) >= Math.Pow(maxLook, 2))
                mouseAimOffset = mouseAimOffset.normalized * maxLook;
            targetPosition +=  mouseAimOffset;

        }

        bool Pressed()
        {
            if (Environment.IsMobile && firestick != null)
                return Utils.PressedJoyStick(firestick);
            return isShooting;
        }

        var desiredPosition = targetPosition + (targetPosition - screenCenterOffset) + offset + cameraDistance;

        if (Environment.IsMobile && mobileZoonSeed != 0)
        {
            currentCameraDistance += mobileZoonSeed * Time.deltaTime * 2.5f;
        }
        else
        {
          //  currentCameraDistance -= Input.mouseScrollDelta.y * zoomSpeed;
        }

        currentCameraDistance = Mathf.Clamp(currentCameraDistance, 0, maxCameraDistance);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
    public void SetTarget(Transform target) => this.target = target;
}
