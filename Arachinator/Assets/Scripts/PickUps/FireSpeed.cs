using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpeed : MonoBehaviour
{
    [SerializeField] float amount;
    [SerializeField] AudioClip sound;

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        var gun = other.GetComponentInChildren<Gun>();
        gun.IncreaseFireSpeed(amount);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(sound);
        Destroy(gameObject);
    }
}
