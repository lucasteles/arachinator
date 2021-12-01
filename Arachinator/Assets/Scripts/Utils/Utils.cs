using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils
{
    static Camera mainCamera;

    static Utils()
    {
        mainCamera = Camera.main;
    }

    public static bool SeeTargetInFront(float view, float distanceToView, Transform transform,  Life target)
    {
        var looking = new Vector2(transform.forward.x, transform.forward.z);
        var direction =
            (new Vector2(target.transform.position.x, target.transform.position.z)
             - new Vector2(transform.position.x, transform.position.z)
            ).normalized;

        var dot = Vector2.Dot(looking, direction);
        if (dot > view &&
            Physics.Raycast(transform.position,
                (target.transform.position - transform.position).normalized,
                out var hit, float.MaxValue))
        {
            if (hit.transform.CompareTag("Player") && !target.IsDead)
            {
                if (hit.distance <= distanceToView)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    return true;
                }

                Debug.DrawLine(transform.position, hit.point, Color.cyan);
            }
        }

        return false;
    }

    public static bool IsInLayerMask(GameObject obj, LayerMask layerMask) =>
        (layerMask.value & (1 << obj.layer)) > 0;

    public static float SimpleCurve(float x) => 4 * (-Mathf.Pow(x, 2) + x);

    public static Vector3 RandomVector3(float from, float to)
    {
        var x = Random.Range(from, to);
        var y = Random.Range(from, to);
        var z = Random.Range(from, to);
        return new Vector3(x, y, z);
    }

    public static bool IsTouching()
    {
        if (Input.touchCount == 0)
            return false;

        for (var i = 0; i < Input.touchCount; i++)
            if (Input.GetTouch(i) is var t && !IsPointerOverUIObject(t.position) && t.pressure > 0)
                return true;

        return false;
    }

    public static bool IsPointerOverUIObject(Vector2 position) {
         var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
         {
             position = position,
         };
         var results = new List<RaycastResult>();
         EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
         foreach (var item in results)
             if (IsInLayerMask(item.gameObject, LayerMask.GetMask("MobileUI")))
                 return true;

         return false;
    }

    public static bool PressedJoyStick(bl_Joystick firestick) =>
        !Mathf.Approximately(firestick.Horizontal + firestick.Vertical, 0);
}