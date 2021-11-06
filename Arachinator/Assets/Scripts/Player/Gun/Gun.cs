using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shotPoint;
    [SerializeField] GameObject shell;
    [SerializeField] Transform shellEjectionPoint;
    [SerializeField] GameObject muzzle;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] float CooldownTime;
    [SerializeField] Transform gunBody;
    [SerializeField] float step;
    [SerializeField] float recoil;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shotAudioClip;

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
            Shot();
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

    void Shot()
    {
        var flash = Instantiate(muzzle, muzzlePoint.position, transform.rotation);
        flash.transform.SetParent(gunBody);
        Destroy(flash, .1f);
        audioSource.PlayOneShot(shotAudioClip);
        Instantiate(this.projectile, shotPoint.transform.position, transform.rotation);
    }
}
