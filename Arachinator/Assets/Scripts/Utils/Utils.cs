using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
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

}