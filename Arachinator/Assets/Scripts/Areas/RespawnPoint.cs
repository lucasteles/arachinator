using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct SpawnData
{
    public GameObject Prefab;
    public EnemyConfiguration Configuration;
}

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] SpawnData[] data;
    [SerializeField] float positionOffset = 1;

    List<GameObject> enemies = new List<GameObject>();

    Player player;
    Life playerLife;
    Vector3[] positions;

    void Awake()
    {
        positions = new[] { transform.forward, transform.right, -transform.forward, -transform.right };
        player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        playerLife.onDeath += PlayerLifeOnonDeath;
    }

    void Start() => Respawn();

    void Respawn()
    {

        foreach (var enemyInfo in data)
        {
            var randomOffset = positions[Random.Range(0, positions.Length)] * Random.Range(0f, positionOffset);
            var enemy = Instantiate(enemyInfo.Prefab, transform.position + randomOffset, Quaternion.identity);
            enemy.GetComponent<IEnemy>().SetConfiguration(enemyInfo.Configuration);
            enemies.Add(enemy);
        }
    }

    void PlayerLifeOnonDeath(Life obj) => Reset();

    void Reset()
    {
        for (var i = 0; i < enemies.Count; i++)
            if (enemies[i] != null)
                Destroy(enemies[i]);
        enemies.Clear();
        Respawn();
    }

}
