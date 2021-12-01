using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtMouse : MonoBehaviour
{
    public float sensitivity = .5f;

    Camera mainCamera;
    [SerializeField]Transform gunPoint;
    [SerializeField] GameObject aim;
    [SerializeField] bl_Joystick firestick;
    Movement movement;
    Life life;
    Vector3 targetPosition;
    bool usingMouse;

    public Vector3 CurrentMousePosition { get; private set; }
    public Vector3 CurrentDirection { get; private set; }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (life && life.IsDead) return;
        var lookVector = context.ReadValue<Vector2>();
        if(lookVector.magnitude < sensitivity) return;
        usingMouse = context.control.device == Mouse.current;
        if(usingMouse)
            targetPosition = Mouse.current.position.ReadValue();
        else
            targetPosition =  lookVector.normalized ;
    }

    void Awake()
    {
        mainCamera = Camera.main;
        movement = GetComponent<Movement>();
        life = GetComponent<Life>();
        if (Environment.IsMobile)
            aim.SetActive(false);
    }

    void Update()
    {
        if (life && life.IsDead) return;

        if (Environment.IsMobile && firestick!=null)
        {
            var h = (firestick.Horizontal);
            var v = (firestick.Vertical);
            var dir = new Vector3(h, 0, v).normalized;
            var lookPoint = transform.position + dir * 2;
            CurrentMousePosition = transform.position + dir * 10;;
            movement.LookAt(lookPoint);
            return;
        }

        if (usingMouse)
        {
            var ray = mainCamera.ScreenPointToRay(targetPosition);
            var plan = new Plane(Vector3.up, Vector3.up * gunPoint.position.y);
            if (!plan.Raycast(ray, out var dist)) return;
            var point = ray.GetPoint(dist);
            CurrentMousePosition = point;
            movement.LookAt(point);
            Debug.DrawLine(ray.origin, point, Color.red);
            aim.transform.position = point;
        }
        else
        {
            var dir = new Vector3(targetPosition.x, 0, targetPosition.y).normalized;
            CurrentDirection = dir;
            var lookPoint = transform.position + dir;
            CurrentMousePosition = lookPoint;
            movement.LookAt(lookPoint);

        }
        aim.SetActive(usingMouse);
    }
}
