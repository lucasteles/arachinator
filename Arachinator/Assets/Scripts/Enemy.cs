using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageble
{
    [SerializeField]AudioClip deathAudio;
    NavMeshAgent navMeshAgent;
    GameObject target;
    Rigidbody rb;
    Life life;

    void Awake()
    {
        life = GetComponent<Life>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        target = FindObjectOfType<Player>().gameObject;
        StartCoroutine(SetDestination());
        life.onDeath += LifeOnDeath;
    }

    public void OnDestroy() => life.onDeath -= LifeOnDeath;

    void LifeOnDeath()
    {
        navMeshAgent.isStopped = true;
        CameraAudioSource.Instance.AudioSource.PlayOneShot(deathAudio);
        Destroy(gameObject);
    }

    void Update()
    {

    }


    IEnumerator SetDestination()
    {
        while (!life.IsDead)
        {
            yield return new WaitForSeconds(.25f);
            navMeshAgent.SetDestination(target.transform.position);
        }
    }

    void Walk()
    {
        rb.velocity = Vector3.zero;
        navMeshAgent.isStopped = false;
    }

    public void TakeDamage(float amount) => life.Subtract(amount);
    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (!navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = true;
            Invoke(nameof(Walk), .1f);
        }
        TakeDamage(amount);
        rb.velocity = Vector3.zero;
        var direction = (transform.position - @from).normalized;
        rb.AddForce(direction * force * rb.mass);
    }

}
