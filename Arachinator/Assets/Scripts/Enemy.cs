using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IDamageble
{
    [SerializeField]AudioClip deathAudio;
    [SerializeField]AudioClip deathAudio2;
    [SerializeField]AudioClip hitSound;
    [SerializeField]GameObject[] bloodEffects;
    [SerializeField]GameObject dieEffect;
    NavMeshAgent navMeshAgent;
    Life target;
    Rigidbody rb;
    Life life;

    void Awake()
    {
        life = GetComponent<Life>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        target = FindObjectOfType<Player>().GetComponent<Life>();
    }

    void Start()
    {
        StartCoroutine(SetDestination());
        life.onDeath += LifeOnDeath;
    }

    public void OnDestroy() => life.onDeath -= LifeOnDeath;

    void LifeOnDeath()
    {
        navMeshAgent.isStopped = true;
        if (Random.Range(0,2) == 1)
            CameraAudioSource.Instance.AudioSource.PlayOneShot(deathAudio);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(deathAudio2);
        var pos = transform.position;
        var blood = Instantiate(dieEffect, new Vector3(pos.x, 0,pos.z), transform.rotation);
        blood.transform.localScale *= Random.Range(1.5f, 2.5f);
        blood.transform.Rotate(Vector3.up,Random.Range(0f, 90f));
        Destroy(blood, 12);
        Destroy(gameObject);
    }

    IEnumerator SetDestination()
    {

        while (!life.IsDead)
        {
            yield return new WaitForSeconds(.25f);

            try
            {
                if (!target.IsDead)
                {
                    if (navMeshAgent.isStopped)
                        navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(target.transform.position);
                }
                else
                {
                    navMeshAgent.isStopped = true;
                }

            }
            catch { }
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
        CameraAudioSource.Instance.AudioSource.PlayOneShot(hitSound);
        rb.velocity = Vector3.zero;
        var direction = (transform.position - target.transform.position).normalized;
        rb.AddForce(direction * force * rb.mass);
        var i = Random.Range(0, bloodEffects.Length );
        var blood = Instantiate(bloodEffects[i], new Vector3(@from.x, 0, @from.z), transform.rotation);
        blood.transform.Rotate(Vector3.up,-90f);
        blood.transform.localScale *= 1.5f;
        blood.transform.SetParent(transform);
        Destroy(blood, 8);
    }
}

