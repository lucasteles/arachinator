using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] Transform respownPoint;
    [SerializeField] AudioClip onSight;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip bossMusic;
    [SerializeField] AudioClip victoryMusic;
    [SerializeField] AudioClip defeatBossClip;
    [SerializeField] Wasp wasp;
    [SerializeField] CanvasGroup bossHeathBar;
    [SerializeField] GameObject[] toDisable;
    [SerializeField] CanvasGroup flashImg;
    [SerializeField] CanvasGroup fadeImg;
    [SerializeField] CanvasGroup[] endItems;

    [SerializeField] Image spiderzina;
    [SerializeField] GameObject targetSpiderzin;
    [SerializeField] AnimationCurve animationSpiderzin;

    [SerializeField] TMP_Text deathCountText;
    [SerializeField] TMP_Text clearTimeText;

    Life bossLife;
    AudioClip originalMusic;
    Life playerLife;
    Movement playerMovement;
    bool inBatle;
    bool isDefeated;

    void Awake()
    {
        var player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        playerMovement = player.Movement;
        originalMusic = musicSource.clip;
        playerLife.onDeath += PlayerDeath;
        bossLife = wasp.GetComponent<Life>();
        bossLife.onDeath += BossLifeOnonDeath;

        foreach (var g in endItems)
            g.alpha = 0;
    }


    void BossLifeOnonDeath(Life obj)
    {
        musicSource.Stop();
        CameraAudioSource.Instance.AudioSource.PlayOneShot(defeatBossClip);
        StartCoroutine(Deafeated());
    }

    IEnumerator Deafeated()
    {
        for (var i = 0f; i <= 1; i += .1f)
        {
            flashImg.alpha = i;
            yield return null;
        }

        for (var i = 1f; i >= 0; i -= .1f)
        {
            flashImg.alpha = i;
            yield return null;
        }
        flashImg.alpha = 0;

        playerMovement.Lock(50);
        var gun  = playerLife.GetComponentInChildren<Gun>();
        gun.enabled = false;
        gun.StopShot();

        playerLife.GetComponentInChildren<WebPistol>().enabled = false;
        var cam = FindObjectOfType<CameraFollow>();
        cam.SetTarget(wasp.transform);
        cam.IsLocket = true;
        yield return new WaitUntil(() => wasp.CurrentState == Wasp.WaspState.Dead);

        var counter = FindObjectOfType<Counter>();
        clearTimeText.text = "Clear Time: " + counter.TotalTime.ToString(@"hh\:mm\:ss");
        deathCountText.text = "Death Count: " + counter.deathCount;

        musicSource.clip = victoryMusic;
        musicSource.Play();

        for (var i = 0f; i < 1; i += .01f)
        {
            fadeImg.alpha = i;
            yield return null;
        }

        StartCoroutine(SpiderIcon());
        foreach (var item in endItems)
        {
            yield return new WaitForSeconds(.5f);
            for (var i = 0f; i <= 1; i += .02f)
            {
                item.alpha = i;
                yield return null;
            }
        }

        isDefeated = true;
    }

    IEnumerator SpiderIcon()
    {
        var spiderIconPos = spiderzina.transform.position;
        var spiderPosTarget = targetSpiderzin.transform.position;
        for (var i = 0f; i <= 1; i += .01f)
        {
            spiderzina.transform.position = Vector3.Lerp(spiderIconPos, spiderPosTarget, animationSpiderzin.Evaluate(i));
            yield return null;
        }
    }

    void Update()
    {
        if (!isDefeated) return;    
        if (Input.anyKey)
            SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy() => playerLife.onDeath -= PlayerDeath;

    void PlayerDeath(Life obj) => StartCoroutine(Respawn());

    IEnumerator Respawn()
    {
        //yield return new WaitUntil(() => playerLife.IsFull() && !playerMovement.IsLocked());
        yield return new WaitForSeconds(3f);
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
        //FindObjectOfType<Player>().RespawnPosition = respownPoint.position;
        ClearEnemies();
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

    public void ClearEnemies()
    {
        var enemies =
            FindObjectsOfType<Enemy>().Cast<MonoBehaviour>()
                .Concat(FindObjectsOfType<Beetle>());
        foreach (var enemy in enemies)
            Destroy(enemy.gameObject);
    }

}