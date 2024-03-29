using Assets.Scripts.Cameras.Effects;
using UnityEngine;
using Assets.Scripts.Ui.PlayerFireSpeedUI;
using UnityEditor;
using UnityEngine.InputSystem;

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
    [SerializeField] float fireSpeed;
    [SerializeField] CameraShakeData shakeData;
    [SerializeField] Animator gunAnimator;
    [SerializeField] bl_Joystick firestick;
    [SerializeField] TurretAnimationEvents turretAnimationEvents;
    public bool canShoot = true;

    Cooldown cooldown;
    public bool IsShooting { get; private set; }
    PlayerFireSpeedUI uiFireSpeed;

    bool pressingShootButton, releasedShootButton;
    public void Shoot(InputAction.CallbackContext context)
    {
        pressingShootButton = context.started || context.performed;
        releasedShootButton = context.canceled;
    }

    void Awake()
    {
        turretAnimationEvents.ShotLeftEvent += ShotLeftEvent;
        turretAnimationEvents.ShotRightEvent += ShotRightEvent;
        uiFireSpeed = FindObjectOfType<PlayerFireSpeedUI>();
    }

    void OnDestroy()
    {
        turretAnimationEvents.ShotLeftEvent -= ShotLeftEvent;
        turretAnimationEvents.ShotRightEvent -= ShotRightEvent;
    }

    void Start()
    {
        cooldown = new Cooldown(fireSpeed);
    }

    public void StartShoot()
    {
        gunAnimator.SetFloat("RateOfFire", fireSpeed);
        gunAnimator.SetBool("Shooting", true);
    }

    public void StopShot()
    {
        gunAnimator.SetBool("Shooting", false);
    }

    void Update()
    {
        if (Pressed() && !gunAnimator.GetBool("Shooting") && canShoot)
        {
            StartShoot();
        }
        else if (IsReleased())
        {
            StopShot();
        }

    }

    bool IsReleased()
    {
        if (Environment.IsMobile && firestick!=null)
            return !Utils.PressedJoyStick(firestick);
        return releasedShootButton;
    }


    bool Pressed()
    {
        if (Environment.IsMobile && firestick!=null)
            return Utils.PressedJoyStick(firestick);
        return pressingShootButton;
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

    public void IncreaseFireSpeed(float amount)
    {
        fireSpeed += amount;

        if (!uiFireSpeed) return;
        uiFireSpeed.SetCurrentSpeed(fireSpeed);
    }
}