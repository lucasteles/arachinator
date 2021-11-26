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
    void Awake() => bulletRigidbody = GetComponent<Rigidbody>();

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

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Item")
            || (other.gameObject.TryGetComponent<IEnemy>(out var enemy) && enemy.ShouldDeflect))
            return;

        ObjectPooling.Get(Pools.HitParticle, transform.position, Quaternion.identity);
        Vanish();
        CameraAudioSource.Instance.AudioSource.PlayOneShot(impact);

        if (other.transform.GetComponent<IDamageble>() is { } damageble)
        {
            damageble.TakeHit(damage, other.contacts[0].point, force);
        }
    }

}
