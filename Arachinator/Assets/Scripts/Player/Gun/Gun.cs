using System;
using Assets.Scripts.Cameras.Effects;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform shotPointLeft;
    [SerializeField] Transform shotPointRight;
    [SerializeField] Transform shellEjectionPoint;
    [SerializeField] Transform muzzleLeftPoint;
    [SerializeField] Transform muzzleRightPoint;
    [SerializeField] Transform gunBody;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shotAudioClip;
    [SerializeField] float cooldownTime;
    [SerializeField] CameraShakeData shakeData;
    [SerializeField] Animator gunAnimator;
    [SerializeField] TurretAnimationEvents turretAnimationEvents;


    Cooldown cooldown;
    public bool IsShooting { get; private set; }

    void Awake()
    {
        turretAnimationEvents.ShotLeftEvent += ShotLeftEvent;
        turretAnimationEvents.ShotRightEvent += ShotRightEvent;
    }

    void OnDestroy()
    {
        turretAnimationEvents.ShotLeftEvent -= ShotLeftEvent;
        turretAnimationEvents.ShotRightEvent -= ShotRightEvent;
    }

    void Start()
    {
        cooldown = new Cooldown(cooldownTime);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !gunAnimator.GetBool("Shooting"))
        {
            gunAnimator.SetFloat("RateOfFire", 1 / cooldownTime / 2);
            gunAnimator.SetBool("Shooting", true);
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            gunAnimator.SetBool("Shooting", false);
        }

    }

    void ShotLeftEvent() => Shot(shotPointLeft, muzzleLeftPoint);
    void ShotRightEvent() => Shot(shotPointRight, muzzleRightPoint);

    void Shot(Transform shotOrigin, Transform muzzleOrigin)
    {
        LaunchProjectile(shotOrigin, muzzleOrigin);
        EjectShell();
        ShotFeedback();
        cooldown.Reset();
    }

    void ShotFeedback() => CameraShaker.Instance.Shake(shakeData);

    void EjectShell() => ObjectPooling.Get(Pools.Shell, shellEjectionPoint.position, shellEjectionPoint.rotation);

    void LaunchProjectile(Transform shotOrigin, Transform muzzleOrigin)
    {
        var flash = ObjectPooling.Get(Pools.MuzzleFlash, muzzleOrigin.position, transform.rotation);
        flash.transform.SetParent(gunBody);
        ObjectPooling.GiveBack(Pools.MuzzleFlash, flash, .1f);
        audioSource.PlayOneShot(shotAudioClip);
        ObjectPooling.Get(Pools.Bullet, shotOrigin.position, transform.rotation);
    }
}
