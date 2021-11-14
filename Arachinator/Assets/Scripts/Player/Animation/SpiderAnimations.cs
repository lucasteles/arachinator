using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderAnimations : MonoBehaviour
{
    [SerializeField] Transform[] idleTranforms;
    [SerializeField] float idleBreathOffset;
    [SerializeField] float idleSpped;
    [SerializeField] AnimationCurve idleAnimationCurve;
    Movement movement;
    Gun gun;
    Dictionary<Transform, Vector3> originalPositions;

    float idleIndex;
    void Start()
    {
        originalPositions = idleTranforms.ToDictionary(x => x, x => x.transform.localPosition);
        movement = GetComponent<Movement>();
        gun = GetComponentInChildren<Gun>();
    }

    void ResetIdle()
    {
        idleIndex = 0;
        for (var i = 0; i < idleTranforms.Length; i++)
            idleTranforms[i].localPosition = originalPositions[idleTranforms[i]];
    }
    void Update()
    {
        if (gun.IsShooting || movement.IsLocked()) return;

        if (movement.Direction == Vector3.zero)
        {
            idleIndex += Time.deltaTime * idleSpped;
            if (idleIndex >= 1f) idleIndex = 0;
            var curve = idleAnimationCurve.Evaluate(idleIndex);
            for (var i = 0; i < idleTranforms.Length; i++)
            {
                var pos = originalPositions[idleTranforms[i]];
                idleTranforms[i].localPosition = Vector3.Lerp(pos, pos + Vector3.up * -idleBreathOffset, curve);
            }
        }
        else
        {
            ResetIdle();
        }

    }
}
