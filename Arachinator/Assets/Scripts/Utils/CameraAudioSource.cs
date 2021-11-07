using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAudioSource : MonoBehaviour
{
    public static CameraAudioSource Instance { get; private set; }

    public AudioSource AudioSource { get; private set; }

    void Awake()
    {
        Instance = this;
        AudioSource = GetComponent<AudioSource>();
    }
}
