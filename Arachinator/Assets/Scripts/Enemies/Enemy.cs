using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IDamageble
{
    public enum State
    {
        Stop,
        Searching,
        Seeking,
        Shooting,
        Desingage
    }

    [SerializeField]AudioClip deathAudio;
    [SerializeField]AudioClip deathAudio2;
    [SerializeField]AudioClip hitSound;
    [SerializeField]GameObject[] bloodEffects;
    [SerializeField]GameObject dieEffect;
    [SerializeField]State initialState = State.Searching;
    [SerializeField]float view;
    [SerializeField]float distanceToView = 5;
    [SerializeField]float searchStep = 2;

    NavMeshAgent navMeshAgent;
    Life target;
    Rigidbody rb;
    Life life;
    BoxCollider targetCollider;
    BoxCollider myCollider;

    State currentState = State.Stop;
    void Awake()
    {
        life = GetComponent<Life>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();
        target = FindObjectOfType<Player>().GetComponent<Life>();
    }
    void OnDestroy() => life.onDeath -= LifeOnDeath;

    void Start()
    {
        targetCollider = target.GetComponent<BoxCollider>();
        life.onDeath += LifeOnDeath;
        SetState(initialState);
    }

    void Update()
    {
        if (SeeTarget() && currentState != State.Seeking)
        {
            StopAllCoroutines();
            SetState(State.Seeking);
        }

        if (!navMeshAgent.isStopped)
            Debug.DrawLine(navMeshAgent.destination, navMeshAgent.destination + Vector3.up * 5, Color.white);
    }

    void SetState(State newState)
    {
        print($"{currentState} -> {newState}");
        switch (newState)
        {
            case State.Searching:
                HandleSearching();
                break;
            case State.Seeking:
                HandleSeeking();
                break;
            case State.Desingage:
                HandleDesingage();
                break;
            case State.Stop:
                HandleStop();
                break;
            case State.Shooting:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void HandleStop()
    {
        if (currentState == State.Stop) return;
        StopAllCoroutines();
        navMeshAgent.isStopped = true;
        currentState = State.Stop;
    }

    void HandleDesingage()
    {
        if (currentState == State.Desingage) return;
        StopAllCoroutines();
        navMeshAgent.isStopped = false;

        IEnumerator Desingage()
        {
            var location = TargetDirection() * -5;
            navMeshAgent.SetDestination(location);
            yield return new WaitForSeconds(3f);
            SetState(State.Searching);
        }

        StartCoroutine(Desingage());
        currentState = State.Desingage;
    }

    void HandleSeeking()
    {
        if (currentState == State.Seeking) return;
        StopAllCoroutines();
        navMeshAgent.isStopped = false;
        StartCoroutine(SeekCoroutine());
        currentState = State.Seeking;
    }

    void HandleSearching()
    {
        if (currentState == State.Searching) return;
        StopAllCoroutines();
        navMeshAgent.isStopped = false;
        StartCoroutine(SearchCoroutine());
        currentState = State.Searching;
    }


    Vector3 TargetDirection() => (transform.position - target.transform.position).normalized;

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

    IEnumerator SeekCoroutine()
    {
        while (!life.IsDead)
        {
            yield return new WaitForSeconds(.25f);

            if (!target.IsDead)
            {
                if (navMeshAgent.isStopped)
                    navMeshAgent.isStopped = false;

                var targetDirection = TargetDirection();
                var targetPosition = target.transform.position +
                                     targetDirection * (targetCollider.size.x/2 - .5f);

                if (!life.IsDead && !target.IsDead)
                    navMeshAgent.SetDestination(targetPosition);
            }
            else
               SetState(State.Desingage);
        }

    }

    IEnumerator SearchCoroutine()
    {
        Vector3 nearPoint() => transform.position + Random.rotation * transform.forward * searchStep;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(nearPoint());
        while (!life.IsDead)
        {
            yield return new WaitForSeconds(.15f);
            if (navMeshAgent.remainingDistance <= myCollider.size.z / 2 || !navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(nearPoint());
            }
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

    bool SeeTarget()
    {
        var looking = new Vector2(transform.forward.x, transform.forward.z);
        var direction =
            (new Vector2(target.transform.position.x, target.transform.position.z)
            -new Vector2(transform.position.x, transform.position.z)
            ).normalized;

        var dot = Vector2.Dot(looking, direction);
        if (dot > view &&
            Physics.Raycast(transform.position,
            (target.transform.position - transform.position).normalized,
            out var hit, float.MaxValue))
        {
            if (hit.transform.CompareTag("Player") &&  !target.IsDead)
            {
                if (hit.distance <= distanceToView)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    return true;
                }
                Debug.DrawLine(transform.position, hit.point, Color.cyan);
            }
        }

        return false;
    }


}

