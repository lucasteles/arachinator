using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RespawnPickup : MonoBehaviour
{
    [SerializeField] GameObject[] data;
    [SerializeField] float positionOffset = 1;

    List<GameObject> pickups = new List<GameObject>();

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
        foreach (var pickupInfo in data)
        {
            var randomOffset = positions[Random.Range(0, positions.Length)] * Random.Range(0f, positionOffset);
            var pickup = Instantiate( pickupInfo, transform.position + randomOffset, Quaternion.identity);
            pickups.Add(pickup);
        }
    }

    void PlayerLifeOnonDeath(Life obj) => Reset();

    void Reset()
    {
        for (var i = 0; i < pickups.Count; i++)
            if (pickups[i] != null)
                Destroy(pickups[i]);
        pickups.Clear();
        Respawn();
    }

}
