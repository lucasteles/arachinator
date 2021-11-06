using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform shell;
    [SerializeField] Transform shellEjectionPoint;
    [SerializeField] float CooldownTime;
    [SerializeField] Transform gunBody;
    [SerializeField] float step;
    [SerializeField] float recoil;

    Vector3 gunbodyPos;
    Cooldown cooldown;

    void Start()
    {
        cooldown = new Cooldown(CooldownTime);
        gunbodyPos = gunBody.transform.localPosition;
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && cooldown)
        {
            EjectShell();
            ShotFeedback();
            cooldown.Reset();
        }
    }

    void ShotFeedback()
    {
        var guntransform = gunBody.transform;
        var pos = guntransform.localPosition;
        guntransform.Translate(new Vector3(pos.x, pos.y, pos.z - recoil));

        IEnumerator routine()
        {
            while (guntransform.localPosition != gunbodyPos)
            {
                guntransform.localPosition = Vector3.Lerp(guntransform.localPosition, gunbodyPos, step);
                yield return null;
            }
        }
        StartCoroutine(routine());
    }

    void EjectShell() => Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
}
