using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float lifespan = 3f;
    [SerializeField] AudioClip impact;
    [SerializeField] float damage = 1f;
    [SerializeField] float force = 10f;
    Rigidbody bulletRigidbody;
    private void Awake() => bulletRigidbody = GetComponent<Rigidbody>();

    void OnEnable()
    {
        bulletRigidbody.velocity = transform.forward * speed;
        Invoke(nameof(Vanish), lifespan);
    }

    void Vanish()
    {
        bulletRigidbody.velocity = Vector3.zero;
        ObjectPooling.GiveBack(Pools.Bullet, gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        ObjectPooling.Get(Pools.HitParticle, transform.position, Quaternion.identity);
        Vanish();
        CameraAudioSource.Instance.AudioSource.PlayOneShot(impact);

        if (other.GetComponent<IDamageble>() is { } damageble)
        {
            damageble.TakeHit(damage, other.ClosestPoint(transform.position), force);
        }
    }
}
