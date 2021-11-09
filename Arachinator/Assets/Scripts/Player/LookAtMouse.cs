using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField]Transform gunPoint;
    Movement movement;
    Life life;

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
        movement.LookAt(point);
        Debug.DrawLine(ray.origin, point, Color.red);
    }
}
