using Assets.Scripts.Cameras.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shotPoint;
    [SerializeField] Transform shellEjectionPoint;
    [SerializeField] GameObject muzzle;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] Transform gunBody;
    [SerializeField] float step;
    [SerializeField] float recoil;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shotAudioClip;
    [SerializeField] float cooldownTime;
    [SerializeField] CameraShakeData shakeData;

    Vector3 gunbodyPos;
    Cooldown cooldown;
    void Start()
    {
        cooldown = new Cooldown(cooldownTime);
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
        CameraShaker.Instance.Shake(shakeData);

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

    void EjectShell() => ObjectPooling.Get(Pools.Shell, shellEjectionPoint.position, shellEjectionPoint.rotation);

    void Shot()
    {
        var flash = ObjectPooling.Get(Pools.MuzzleFlash, muzzlePoint.position, transform.rotation);
        flash.transform.SetParent(gunBody);
        ObjectPooling.GiveBack(Pools.MuzzleFlash, flash, .1f);
        audioSource.PlayOneShot(shotAudioClip);
        ObjectPooling.Get(Pools.Bullet, shotPoint.transform.position, transform.rotation);
    }
}
