using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtMouse : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField]Transform gunPoint;
    [SerializeField] GameObject aim;
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
        life = GetComponent<Life>();
    }

    void Update()
    {
        if (life is { } l && l.IsDead) return;
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
            aim.SetActive(true);
        }
        else
            aim.SetActive(false);
    }
}
