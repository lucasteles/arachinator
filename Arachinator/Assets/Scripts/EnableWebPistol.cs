using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Ui.WebTutorial;
using UnityEngine;

public class EnableWebPistol : MonoBehaviour
{
    [SerializeField] AudioClip audio;
    WebTutorial tutorialUI;

    private void Awake() =>
        tutorialUI = FindObjectOfType<WebTutorial>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        tutorialUI.Show();
        other.GetComponentInChildren<WebPistol>().enabled = true;
        CameraAudioSource.Instance.AudioSource.PlayOneShot(audio);
        Destroy(gameObject);
    }
}