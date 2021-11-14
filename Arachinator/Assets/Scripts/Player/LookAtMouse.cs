using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField]Transform gunPoint;
    [SerializeField] GameObject aim;
    Movement movement;
    Life life;

    public Vector3 CurrentMousePosition { get; private set; }

    void Awake()
    {
        mainCamera = Camera.main;
        movement = GetComponent<Movement>();
        life = GetComponent<Life>();
    }

    void Update()
    {
        if (life is { } l && l.IsDead) return;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var plan = new Plane(Vector3.up, Vector3.up * gunPoint.position.y);

        if (!plan.Raycast(ray, out var dist)) return;
        var point = ray.GetPoint(dist);
        CurrentMousePosition = point;
        movement.LookAt(point);
        Debug.DrawLine(ray.origin, point, Color.red);
        aim.transform.position = point;
    }

}
