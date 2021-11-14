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
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shotAudioClip;
    [SerializeField] float cooldownTime;
    [SerializeField] CameraShakeData shakeData;
    [SerializeField] Animator gunAnimator;

    Vector3 gunbodyPos;
    Cooldown cooldown;
    private Coroutine stopShooting;

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
        gunAnimator.SetFloat("RateOfFire", 1 / cooldownTime);
        gunAnimator.SetBool("Shooting", true);

        CameraShaker.Instance.Shake(shakeData);

        if (stopShooting != null) StopCoroutine(stopShooting);
        stopShooting = StartCoroutine(StopShooting());
    }

    private IEnumerator StopShooting()
    {
        yield return new WaitForSeconds(cooldownTime);
        gunAnimator.SetBool("Shooting", false);
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
