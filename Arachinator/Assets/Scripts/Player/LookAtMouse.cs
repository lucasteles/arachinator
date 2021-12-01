using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtMouse : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField]Transform gunPoint;
    [SerializeField] GameObject aim;
    [SerializeField] bl_Joystick firestick;
    Movement movement;
    Life life;
    Vector3 mousePosition;
    bool usingMouse;

    public Vector3 CurrentMousePosition { get; private set; }

    public void OnLook(InputAction.CallbackContext context)
    {
        usingMouse = context.control.device == Mouse.current;
        if(usingMouse)
            mousePosition = Mouse.current.position.ReadValue();
        else
            mousePosition = context.ReadValue<Vector2>() * new Vector2(Screen.width, Screen.height);
    }

    void Awake()
    {
        mainCamera = Camera.main;
        movement = GetComponent<Movement>();
        if (Enviroment.IsMobile)
            aim.SetActive(false);
    }

    void Update()
    {
        if (life is { IsDead: true }) return;

        if (Enviroment.IsMobile && firestick!=null)
        {
            var h = Mathf.Round(firestick.Horizontal);
            var v = Mathf.Round(firestick.Vertical);
            var dir = new Vector3(h, 0, v).normalized;
            var lookPoint = transform.position + dir * 2;
            movement.LookAt(lookPoint);
            return;
        }

        var ray = mainCamera.ScreenPointToRay(mousePosition);

        var plan = new Plane(Vector3.up, Vector3.up * gunPoint.position.y);

        if (!plan.Raycast(ray, out var dist)) return;
        var point = ray.GetPoint(dist);
        CurrentMousePosition = point;
        movement.LookAt(point);
        Debug.DrawLine(ray.origin, point, Color.red);

        if(usingMouse)
        {
            aim.transform.position = point;
        }
        else
            aim.SetActive(false);
    }
}
