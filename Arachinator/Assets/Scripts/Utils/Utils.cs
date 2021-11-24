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
}