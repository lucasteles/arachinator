using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableWebPistol : MonoBehaviour
{
    [SerializeField] AudioClip audio;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponentInChildren<WebPistol>().enabled = true;
        CameraAudioSource.Instance.AudioSource.PlayOneShot(audio);
        Destroy(gameObject);
    }
}
