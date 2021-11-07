using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float lifespan = 3f;
    [SerializeField]AudioClip impact;
    [SerializeField] TrailRenderer trailRenderer;
    Rigidbody bulletRigidbody;

    private void Awake() => bulletRigidbody = GetComponent<Rigidbody>();

    void OnEnable()
    {
        trailRenderer.Clear();
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
        Camera.main.GetComponent<AudioSource>().PlayOneShot(impact);
    }
}
