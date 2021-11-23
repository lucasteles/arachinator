using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuspe : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float lifespan = 3f;
    [SerializeField] AudioClip impact;
    [SerializeField] float damage = 1f;
    [SerializeField] float force = 10f;
    [SerializeField] GameObject explosion;
    Rigidbody bulletRigidbody;
    void Awake() => bulletRigidbody = GetComponent<Rigidbody>();

    void OnEnable()
    {
        bulletRigidbody.velocity = transform.forward * speed;
        Invoke(nameof(Vanish), lifespan);
    }

    void Vanish()
    {
        if (!gameObject) return;
        bulletRigidbody.velocity = Vector3.zero;
        ObjectPooling.GiveBack(Pools.Cuspe, gameObject);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Item")) return;
        Vanish();
        CameraAudioSource.Instance.AudioSource.PlayOneShot(impact);
        var effect = Instantiate(explosion, other.contacts[0].point, transform.rotation);
        effect.transform.localScale *= 3;
        Destroy(effect, 2);
        if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponent<IDamageble>() is { } damageble)
        {
            damageble.TakeHit(damage, other.contacts[0].point, force);
        }
    }
}
