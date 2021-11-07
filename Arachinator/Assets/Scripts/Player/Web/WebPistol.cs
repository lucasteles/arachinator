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
    [SerializeField]float impulseForce;
    [SerializeField] GameObject muzzlePrefab;
    [SerializeField] GameObject webPrefab;
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
        movement.Lock(.2f);
        yield return new WaitForSeconds(.2f);
        rigidybody.velocity = Vector3.zero;
        rigidybody.AddForce(impulseForce * -transform.forward);
        var muzzle= Instantiate(muzzlePrefab, transform.position, transform.rotation);
        muzzle.transform.SetParent(shotPoint);
        muzzle.transform.Rotate(Vector3.up,180f);
        Destroy(muzzle, -.2f);
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
