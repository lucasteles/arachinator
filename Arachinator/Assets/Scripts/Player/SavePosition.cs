using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SavePosition : MonoBehaviour
{
    [SerializeField] float timeToSave = 5f;

    Vector3 lastValidPosition;

    Player player;
    Life life;
    Cooldown cooldown;
    Rigidbody rb;

    void Awake()
    {
        player = GetComponent<Player>();
        life = GetComponent<Life>();
        rb = GetComponent<Rigidbody>();
        cooldown = new Cooldown(timeToSave);
        life.onDeath += LifeOnDeath;
    }
    void OnDestroy() => life.onDeath -= LifeOnDeath;
    void LifeOnDeath() => cooldown.Reset();


    void Update()
    {
        if (life.IsDead) cooldown.Reset();

        if (!life.IsDead && cooldown && transform.position.y > 0 && Mathf.Approximately(rb.velocity.y, 0f))
        {
            print("save...");
            cooldown.Reset();
            player.RespawnPosition = new Vector3(transform.position.x, 1.5f, transform.position.z);
        }
    }
}
