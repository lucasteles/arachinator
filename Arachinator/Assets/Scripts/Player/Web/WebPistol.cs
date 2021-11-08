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
    [SerializeField]Movement movement;
    [SerializeField]float simpleBackdash;
    [SerializeField]float upForce;
    [SerializeField]float upBackDashForce;
    [SerializeField]GameObject muzzlePrefab;
    [SerializeField]GameObject webPrefab;
    Cooldown cooldown;
    Vector3? hitPosition = null;
    public Vector3 ShotPoint => shotPoint.position;
    public Vector3? Target => hitPosition;


    void Start()
    {
        cooldown = new Cooldown(coodownTime);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2") && cooldown)
        {
            ThrowWeb();
            cooldown.Reset();
        }
    }

    void ThrowWeb()
    {
        audioSource.PlayOneShot(shotClip);
        var muzzle= Instantiate(muzzlePrefab, shotPoint.position, shotPoint.rotation);
        muzzle.transform.Rotate(Vector3.up,180f);
        Destroy(muzzle, .3f);
        if (Physics.Raycast(shotPoint.position, -shotPoint.forward, out var hit, maxDistance))
        {
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
        var isPressing = Input.GetButton("Fire2");
        movement.Lock(isPressing ? 1f : .3f);
        rigidybody.velocity = Vector3.zero;
        var newForce = isPressing
            ? upForce * transform.up + upBackDashForce * -transform.forward
            : simpleBackdash * -transform.forward;
        rigidybody.AddForce(newForce, ForceMode.Acceleration);
        var web = Instantiate(webPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        web.transform.SetParent(hit.collider.gameObject.transform);
        Destroy(web, 1f);
        audioSource.PlayOneShot(hitClip);
    }
    void Hide()
    {
        hitPosition = null;
    }

    public bool TargetDefined() => hitPosition.HasValue;
}
