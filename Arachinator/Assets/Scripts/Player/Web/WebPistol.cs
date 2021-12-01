using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WebPistol : MonoBehaviour
{
    [SerializeField]Transform shotPoint;
    [SerializeField]float maxDistance;
    [SerializeField]float coodownTime;
    [SerializeField]Rigidbody rigidybody;
    [SerializeField]AudioSource audioSource;
    [SerializeField]AudioClip shotClip;
    [SerializeField]AudioClip hitClip;
    [SerializeField]float simpleBackdash;
    [SerializeField]float upForce;
    [SerializeField]float upBackDashForce;
    [SerializeField]GameObject webPrefab;
    [SerializeField]Transform butTransform;
    [SerializeField]AnimationCurve butScaleCurve;
    [SerializeField]LayerMask layerMask;
    Cooldown cooldown;
    Vector3? hitPosition = null;
    public Vector3 ShotPoint => shotPoint.position;
    public Vector3? Target => hitPosition;


    bool shotWeb;
    Movement movement;
    Player player;

    void Awake()
    {
        movement = GetComponentInParent<Movement>();
        player = GetComponentInParent<Player>();
    }

    void Start()
    {
        cooldown = new Cooldown(coodownTime);
        butScale = butTransform.transform.localScale;
        butPos = butTransform.localPosition;
    }

    bool PressedButton()
    {
        if (Enviroment.IsMobile)
        {
            var should = shotWeb;
            shotWeb = false;
            return should;
        }

        return Input.GetButtonDown("Fire2");
    }

    void Update()
    {
        if (PressedButton() && cooldown)
        {
            ThrowWeb();
            cooldown.Reset();
        }
    }

    void DisableInvincible() => player.DisableInvicible();

    public void ShotWeb()
    {
        shotWeb = true;
    }

    void ThrowWeb()
    {
        audioSource.PlayOneShot(shotClip);
        ButShotFeedback();
        if (Physics.Raycast(shotPoint.position, -shotPoint.forward, out var hit, maxDistance, layerMask))
        {
            player.EnableInvicible();
            Invoke(nameof(DisableInvincible), .3f);
            hitPosition = hit.point;
            StartCoroutine(Hit(hit));
            Invoke(nameof(Hide), .5f);
        }
        else
        {
            hitPosition = -transform.forward * maxDistance + transform.position;
            Invoke(nameof(Hide), .3f);
        }

    }

    IEnumerator Hit(RaycastHit hit)
    {
        yield return new WaitForSeconds(.3f);
        // var isPressing = Input.GetButton("Fire2");
        // movement.Lock(isPressing ? 1f : .3f);
        movement.Lock(1f);
        rigidybody.velocity = Vector3.zero;

        // var newForce = isPressing
        //     ? simpleBackdash * -transform.forward
        //     : upForce * transform.up + upBackDashForce * -transform.forward;

        var newForce = upForce * transform.up + upBackDashForce * -transform.forward;
        rigidybody.AddForce(newForce, ForceMode.Acceleration);
        var web = Instantiate(webPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        web.transform.SetParent(hit.collider.gameObject.transform);
        Destroy(web, 1f);
        audioSource.PlayOneShot(hitClip);
    }

    void Hide() => hitPosition = null;

    public bool TargetDefined() => hitPosition.HasValue;


    Coroutine butScaleCoroutine;
    Coroutine butTranslateCoroutine;
    Vector3 butScale;
    Vector3 butPos;

    void ButShotFeedback()
    {
        if (butScaleCoroutine!=null) StopCoroutine(butScaleCoroutine);
        if (butTranslateCoroutine!=null) StopCoroutine(butTranslateCoroutine);
        movement.Lock(.3f);
        butTransform.localPosition = butPos;
        butTransform.localScale = butScale;

        IEnumerator routineScale()
        {
            var targetScale = new Vector3(butTransform.localScale.x * .6f, butTransform.localScale.y, butTransform.localScale.z);
            yield return null;
            for (var i = 0f; i <= 1; i+=.045f)
            {
                butTransform.localScale = Vector3.Lerp(butScale, targetScale, butScaleCurve.Evaluate(i));
                yield return null;
            }
            butTransform.localScale = butScale;
        }

        IEnumerator routineTranslate()
        {
            var targetPos = butTransform.localPosition - (.008f * butTransform.forward);
            for (var i = 0f; i <= 1; i+=.1f)
            {
                butTransform.localPosition = Vector3.Lerp(butPos, targetPos, butScaleCurve.Evaluate(i));
                yield return null;
            }
            yield return null;
            butTransform.localPosition = butPos;
        }

        butTranslateCoroutine = StartCoroutine(routineTranslate());
        butScaleCoroutine = StartCoroutine(routineScale());
    }



}
