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
    [SerializeField] CanvasGroup bossHeathBar;
    [SerializeField] GameObject[] toDisable;

    AudioClip originalMusic;
    Life playerLife;
    Movement playerMovement;
    bool inBatle;

    void Awake()
    {
        var player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        playerMovement = player.Movement;
        originalMusic = musicSource.clip;
        playerLife.onDeath += PlayerDeath;
    }

    void OnDestroy() => playerLife.onDeath -= PlayerDeath;

    void PlayerDeath(Life obj) => StartCoroutine(Respawn());

    IEnumerator Respawn()
    {
        //yield return new WaitUntil(() => playerLife.IsFull() && !playerMovement.IsLocked());
        yield return new WaitForSeconds(3);
        musicSource.clip = originalMusic;
        musicSource.Play();
        bossHeathBar.alpha = 0;
        wasp.Reset();
        foreach (var obj in toDisable)
            obj.SetActive(true);

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

        foreach (var obj in toDisable)
            obj.SetActive(false);

        yield return new WaitUntil(() => wasp.CurrentState != Wasp.WaspState.Sleep);
        StartBossMusic();
        for (var i = 0f; i < 1; i += .05f)
        {
            bossHeathBar.alpha = i;
            yield return null;
        }
    }

    public void StartBossMusic()
    {
        musicSource.clip = bossMusic;
        musicSource.Play();
    }

}
