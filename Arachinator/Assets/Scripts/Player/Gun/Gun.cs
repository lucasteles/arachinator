using Assets.Scripts.Cameras.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.VFX;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform shotPoint;
    [SerializeField] Transform shellEjectionPoint;
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
    public bool IsShooting { get; private set; }

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
        IsShooting = true;
        var guntransform = gunBody.transform;
        guntransform.Translate(recoil * Vector3.back);
        CameraShaker.Instance.Shake(shakeData);

        IEnumerator routine()
        {
            for (var i = 0f; i < 1f; i+=step)
            {
                guntransform.localPosition = Vector3.Lerp(guntransform.localPosition, gunbodyPos, i);
                yield return null;
            }
            IsShooting = false;
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
