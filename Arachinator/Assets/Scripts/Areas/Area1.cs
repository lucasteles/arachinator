using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable]
public struct WaveItem
{
    public GameObject Enemy;
    public EnemyConfiguration Configuration;
    public int Quantity;
}

[Serializable]
public struct WaveGroup
{
    public WaveItem[] Waves;
}

[Serializable]
public class WaveController
{
    public int currentWave = 0;
    public List<GameObject> enemies;
    public float spawnWait = 1;
    public WaveGroup[] Waves;

    public GameObject effect;
    public event Action OnWaveEnded;

    public bool NextWave()
    {
        if (currentWave == Waves.Length - 1)
            return false;

        currentWave++;
        return true;
    }

    public IEnumerator Spawn(GameObject[] spawmPoints)
    {
        var waveGroup = Waves[currentWave];
        for (var j = 0; j < waveGroup.Waves.Length; j++)
        {
            var wave = waveGroup.Waves[j];
            for (var i = 0; i < wave.Quantity; i++)
            {
                yield return new WaitForSeconds(spawnWait);
                var enemy = Object.Instantiate(wave.Enemy);
                enemy.GetComponent<Life>().onDeath += onDeath;
                enemy.GetComponent<IEnemy>().SetConfiguration(wave.Configuration);
                var point = spawmPoints[Random.Range(0, spawmPoints.Length)].transform.position;
                enemy.GetComponent<NavMeshAgent>().Warp(point);
                enemy.transform.position = point;
                Object.Destroy(Object.Instantiate(effect, point, Quaternion.identity), 3f);
                enemies.Add(enemy);
            }
        }
    }

    void onDeath(Life life)
    {
        life.onDeath -= onDeath;
        enemies.Remove(life.gameObject);

        if (enemies.Count == 0)
            OnWaveEnded?.Invoke();
    }

    public void Reset()
    {
        currentWave = 0;
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Life>().onDeath -= onDeath;
            Object.Destroy(enemy);
        }
        enemies.Clear();
    }
}

public class Area1 : MonoBehaviour
{
    [SerializeField] GameObject[] gates;
    [SerializeField] GameObject[] spawnPoints;
    [SerializeField] WaveController wave;

    Life player;
    bool playing;
    bool done = false;
    public void CloseGates()
    {
        foreach (var portal in gates)
            portal.SetActive(true);
    }

    public void OpenGates()
    {
        foreach (var portal in gates)
            portal.SetActive(false);
    }

    void Awake()
    {
        wave.OnWaveEnded += WaveOnOnWaveEnded;
        player = FindObjectOfType<Player>().GetComponent<Life>();
        player.onDeath += PlayerOnDeath;
    }


    void PlayerOnDeath(Life obj)
    {
        playing = false;
        OpenGates();
        StopAllCoroutines();
        wave.Reset();
    }

    void OnDestroy()
    {
        wave.OnWaveEnded -= WaveOnOnWaveEnded;
        player.onDeath -= PlayerOnDeath;
    }

    void WaveOnOnWaveEnded()
    {
        if (wave.NextWave())
            StartCoroutine(wave.Spawn(spawnPoints));
        else
        {
            OpenGates();
            done = true;
        }
    }

    public void PlayerEntered()
    {
        if (playing || done) return;
        CloseGates();
        playing = true;
        StartCoroutine(wave.Spawn(spawnPoints));
    }
}