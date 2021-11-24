using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IDamageble, IEnemy
{
    [SerializeField]AudioClip deathAudio;
    [SerializeField]AudioClip deathAudio2;
    [SerializeField]AudioClip hitSound;
    [SerializeField]AudioClip projectileSound;
    [SerializeField]GameObject[] bloodEffects;
    [SerializeField]GameObject dieEffect;
    [SerializeField]Transform shootPoint;

    [SerializeField]EnemyConfiguration config;

    float currentNumberOfShoots = 0;
    NavMeshAgent navMeshAgent;
    Life target;
    Rigidbody rb;
    Life life;
    Cooldown cooldown;
    BoxCollider targetCollider;
    BoxCollider myCollider;
    Animator animator;

    State currentState = State.Stop;
    static readonly int IsShooting = Animator.StringToHash("IsShooting");

    void Awake()
    {
        life = GetComponent<Life>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();
        target = FindObjectOfType<Player>().GetComponent<Life>();
        animator = GetComponent<Animator>();
        life.onDeath += LifeOnDeath;
    }
    void OnDestroy() => life.onDeath -= LifeOnDeath;

    public void SetConfiguration(EnemyConfiguration config)
    {
        this.config = config;
        life.SetMaxLife(config.maxLife);
        life.Reset();
        navMeshAgent.speed = config.speed;
        cooldown = new Cooldown(config.shootCooldownTime);
        SetState(config.initialState);
    }

    void Start()
    {
        targetCollider = target.GetComponent<BoxCollider>();
        SetConfiguration(config);
    }

    void Update()
    {
        if (SeeTarget() && currentState != State.Seeking && currentState != State.Shooting)
        {
            StopAllCoroutines();
            SetState(State.Seeking);
        }

        if (!navMeshAgent.isStopped)
            Debug.DrawLine(navMeshAgent.destination, navMeshAgent.destination + Vector3.up * 5, Color.white);
    }

    bool SeeTarget() =>
        Utils.SeeTargetInFront(config.view, config.distanceToView, transform, target)
        || Vector3.Distance(transform.position, target.transform.position) <= config.distanceAroundToSee;

    void SetState(State newState)
    {
        //print($"{currentState} -> {newState}");
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
                currentState = State.Shooting;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void StopNav()
    {
        if (!life.IsDead)
            navMeshAgent.isStopped = true;
    }

    void StartNav()
    {
        if (!life.IsDead)
            navMeshAgent.isStopped = false;
    }
    void HandleStop()
    {
        if (currentState == State.Stop) return;
        StopAllCoroutines();
        StopNav();
        currentState = State.Stop;
    }

    void HandleDesingage()
    {
        if (currentState == State.Desingage) return;
        StopAllCoroutines();
        StartNav();

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
        StartNav();
        StartCoroutine(SeekCoroutine());
        currentState = State.Seeking;
    }

    void HandleSearching()
    {
        if (currentState == State.Searching) return;
        StopAllCoroutines();
        StartNav();
        StartCoroutine(SearchCoroutine());
        currentState = State.Searching;
    }


    Vector3 TargetDirection() => (transform.position - target.transform.position).normalized;

    void LifeOnDeath(Life life)
    {
        StopNav();
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

    bool ShouldShoot() =>
        config.shouldShoot
        && cooldown
        && Vector3.Distance(transform.position, target.transform.position) <= config.minShootDistance
        && Utils.SeeTargetInFront(config.view, config.distanceToView, transform, target);

    IEnumerator SeekCoroutine()
    {
        while (!life.IsDead)
        {
            yield return new WaitForSeconds(.25f);

            if (!target.IsDead)
            {
                if (ShouldShoot())
                  yield return StartShooting(Random.Range(1, config.maxShoots));
                else
                    GoCloser();
            }
            else
               SetState(State.Desingage);
        }

    }

    IEnumerator StartShooting(int numberOfShoots)
    {
        foreach (var spiderLegConstraint in GetComponentsInChildren<SpiderLegConstraint>())
            spiderLegConstraint.enabled = false;

        StopNav();
        cooldown.Reset();
        SetState(State.Shooting);
        currentNumberOfShoots = 0;
        animator.SetBool(IsShooting, true);

        while (currentNumberOfShoots < numberOfShoots && !target.IsDead)
        {
            // transform.LookAt(target.transform);
            StopNav();
            yield return null;
        }

        animator.SetBool(IsShooting, false);
        foreach (var spiderLegConstraint in GetComponentsInChildren<SpiderLegConstraint>())
            spiderLegConstraint.enabled = true;
        SetState(State.Seeking);
    }

    public void ShootProjectileAnimationEvent()
    {
        currentNumberOfShoots++;
        CameraAudioSource.Instance.AudioSource.PlayOneShot(projectileSound);
        ObjectPooling.Get(Pools.Cuspe, shootPoint.position, transform.rotation);
    }

    void GoCloser()
   {
        if (navMeshAgent.isStopped)
            StartNav();

        var targetDirection = TargetDirection();
        var targetPosition = target.transform.position +
                             targetDirection * (targetCollider.size.x / 2 - .5f);

        if (!life.IsDead && !target.IsDead)
            navMeshAgent.SetDestination(targetPosition);
   }

    IEnumerator SearchCoroutine()
    {
        Vector3 nearPoint() => transform.position + Random.rotation * transform.forward * config.searchStep;

        StartNav();
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
        StartNav();
    }

    public void TakeDamage(float amount) => life.Subtract(amount);

    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (!navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = true;
            Invoke(nameof(Walk), .5f);
        }
        TakeDamage(amount);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(hitSound);
        rb.velocity = Vector3.zero;
        var direction = (transform.position - target.transform.position).normalized;
        rb.AddForce(direction * force, ForceMode.VelocityChange);
        var i = Random.Range(0, bloodEffects.Length );
        var blood = Instantiate(bloodEffects[i], new Vector3(@from.x, 0, @from.z), transform.rotation);
        blood.transform.Rotate(Vector3.up,Random.rotation.eulerAngles.y);
        blood.transform.localScale *= 1.5f;
        Destroy(blood, 8);

        if (currentState != State.Seeking && currentState != State.Shooting)
            SetState(State.Seeking);
    }

}

public interface IEnemy
{
    void SetConfiguration(EnemyConfiguration configuration);
}

