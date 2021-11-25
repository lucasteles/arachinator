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

    List<GameObject> objects = new List<GameObject>();

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
        foreach (var objectInfo in data)
        {
            var randomOffset = positions[Random.Range(0, positions.Length)] * Random.Range(0f, positionOffset);
            var objectPrefab = Instantiate(objectInfo.Prefab, transform.position + randomOffset, Quaternion.identity);
            if (objectInfo.Configuration) {
                objectPrefab.GetComponent<IEnemy>().SetConfiguration(objectInfo.Configuration);
            }
            objects.Add(objectPrefab);
        }
    }

    void PlayerLifeOnonDeath(Life obj) => Reset();

    void Reset()
    {
        for (var i = 0; i < objects.Count; i++)
            if (objects[i] != null)
                Destroy(objects[i]);
        objects.Clear();
        Respawn();
    }
}