using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIncrese : MonoBehaviour
{
    [SerializeField] float amount;
    [SerializeField] AudioClip sound;

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        var life = other.GetComponentInChildren<Player>();
        if (life.IsMaxHealth()) return;
        life.AddLife(amount);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(sound);
        Destroy(gameObject);
    }
}
