using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Transform vfxHit;
    [SerializeField] float speed = 50f;
    Rigidbody bulletRigidbody;

    [SerializeField]AudioClip impact;

    private void Awake() => bulletRigidbody = GetComponent<Rigidbody>();

    private void Start() {
        bulletRigidbody.velocity = transform.forward * speed;
    }

    void OnTriggerEnter(Collider other) {
        Instantiate(vfxHit, transform.position, Quaternion.identity);
        Destroy(gameObject);
        Camera.main.GetComponent<AudioSource>().PlayOneShot(impact);
    }
}
