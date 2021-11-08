using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFX_DecaGizmo : MonoBehaviour
{
    Transform t;
    private void OnDrawGizmos()
    {
        if (t == null) t = transform;

        Gizmos.color = new Color(49 / 255.0f, 136 / 255.0f, 1, 0.15f);
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = new Color(49 / 255.0f, 136 / 255.0f, 1, 0.95f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(0.95f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawRay(t.position, t.up);
    }
}
