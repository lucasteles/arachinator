using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{

    [SerializeField] Transform respownPoint;
    [SerializeField] AudioClip onSight;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip bossMusic;
    [SerializeField] Wasp wasp;

    AudioClip originalMusic;
    Life playerLife;
    Movement playerMovement;
    bool inBatle;

    void Awake()
    {
        var player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        playerMovement = player.GetComponent<Movement>();
        originalMusic = musicSource.clip;
        playerLife.onDeath += PlayerDeath;
    }

    void OnDestroy() => playerLife.onDeath -= PlayerDeath;

    void PlayerDeath(Life obj) => StartCoroutine(Respawn());

    IEnumerator Respawn()
    {
        yield return new WaitUntil(() => playerLife.IsFull() && !playerMovement.IsLocked());
        musicSource.clip = originalMusic;
        musicSource.Play();
        wasp.Reset();
        inBatle = false;
    }

    public void SetRespawnPoint()
    {
        if (inBatle) return;
        inBatle = true;
        StartCoroutine(AwakeBoss());
    }

    IEnumerator AwakeBoss()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(onSight);
        FindObjectOfType<Player>().RespawnPosition = respownPoint.position;
        wasp.AwakeBoss();
        musicSource.Stop();
        yield return new WaitUntil(() => wasp.CurrentState != Wasp.WaspState.Sleep);
        StartBossMusic();
    }


    public void StartBossMusic()
    {
        musicSource.clip = bossMusic;
        musicSource.Play();
    }

}
