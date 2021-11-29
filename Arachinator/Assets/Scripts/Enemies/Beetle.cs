using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Beetle : MonoBehaviour, IDamageble, IEnemy
{
    [SerializeField]AudioClip deathAudio;
    [SerializeField]AudioClip deathAudio2;
    [SerializeField]AudioClip deflectSound;
    [SerializeField]AudioClip hitSound;
    [SerializeField]AudioClip whoosh;
    [SerializeField]AudioClip creek;
    [SerializeField]AudioClip impact;
    [SerializeField]GameObject[] bloodEffects;
    [SerializeField]GameObject dieEffect;
    [SerializeField]GameObject trackEffect;
    [SerializeField]float trackeForce;
    [SerializeField]LayerMask layersToTrackleIgnore;
    [SerializeField]GameObject enemyCheckerPoint;
    [SerializeField]float enemyPushForce;

    [SerializeField]EnemyConfiguration config;

    NavMeshAgent navMeshAgent;
    Life target;
    Rigidbody rb;
    Life life;
    Cooldown cooldown;
    BoxCollider targetCollider;
    BoxCollider myCollider;
    Animator animator;
    TrailRenderer trailRenderer;
    CapsuleCollider hitCollider;
    EnemyEffects enemyEffects;

    bool inTracke = false;
    bool trackeHit = true;
    State currentState = State.Stop;
    static readonly int IsShooting = Animator.StringToHash("IsShooting");
    BeetleTrackeEvent tracke;

    public bool ShouldDeflect => inTracke;
    void Awake()
    {
        life = GetComponent<Life>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();
        hitCollider = GetComponent<CapsuleCollider>();
        tracke = GetComponentInChildren<BeetleTrackeEvent>();
        target = FindObjectOfType<Player>().GetComponent<Life>();
        animator = GetComponentInChildren<Animator>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        enemyEffects = GetComponentInChildren<EnemyEffects>();
        life.onDeath += LifeOnDeath;
        tracke.onTracke += Trackle;
    }

    void Trackle()
    {
        inTracke = true;
    }
    void OnDestroy()
    {
        life.onDeath -= LifeOnDeath;
        tracke.onTracke -= Trackle;
    }


    void Start()
    {
        targetCollider = target.GetComponent<BoxCollider>();
        SetConfiguration(config);
    }
    public void SetConfiguration(EnemyConfiguration configuration)
    {
        config = configuration;
        navMeshAgent.speed = config.speed;
        cooldown = new Cooldown(config.shootCooldownTime);
        SetState(config.initialState);
        life.SetMaxLife(config.maxLife);
        life.Reset();

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

        if (inTracke && rb.velocity.sqrMagnitude > .01f
            && Physics.SphereCast(
                enemyCheckerPoint.transform.position,
                myCollider.size.x / 2,
                transform.forward,
                out var hit, 2f,
                LayerMask.GetMask("Enemy"))
            && hit.transform.TryGetComponent<IDamageble>(out var damageble))
        {
            damageble.TakeHit(0, hit.point, enemyPushForce);
        }
    }

    bool SeeTarget() => Utils.SeeTargetInFront(config.view, config.distanceToView, transform, target)
                        ||
                        Vector3.Distance(transform.position, target.transform.position)
                        <= config.distanceAroundToSee;

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
        if (!life.IsDead && navMeshAgent.isActiveAndEnabled)
            navMeshAgent.isStopped = true;
    }

    void StartNav()
    {
        if (!life.IsDead && navMeshAgent.isActiveAndEnabled)
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
        var blood = Instantiate(dieEffect, pos, transform.rotation);
        blood.transform.localScale *= Random.Range(1.5f, 2.5f);
        blood.transform.Rotate(Vector3.up,Random.Range(0f, 90f));
        config.InstantiateDrop(transform.position, Quaternion.identity);
        Destroy(blood, 12);
        Destroy(gameObject);
    }

    bool ShouldShoot() =>
        config.shouldShoot
        && cooldown
        && Vector3.Distance(transform.position, target.transform.position) >= config.minShootDistance
        && Utils.SeeTargetInFront(config.view, config.distanceToView, transform, target);

    IEnumerator SeekCoroutine()
    {
        while (!life.IsDead)
        {
            yield return new WaitForSeconds(.25f);

            if (!target.IsDead)
            {
                if (ShouldShoot())
                   yield return DoTrackle();
                else
                    GoCloser();
            }
            else
               SetState(State.Desingage);
        }

    }

    IEnumerator DoTrackle()
    {
        yield return new WaitForSeconds(1f);
        foreach (var spiderLegConstraint in GetComponentsInChildren<SpiderLegConstraint>())
            spiderLegConstraint.enabled = false;

        var damage = GetComponent<EnemyDamageDealer>();
        trackeHit = false;
        var constraints = rb.constraints;
        var useGravity = rb.useGravity;
        damage.damage *= 2;
        damage.force *= 2;
        rb.velocity = Vector3.zero;
        myCollider.enabled = false;
        hitCollider.enabled = true;

        for (var i = 0f; i <= 1; i+= .2f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), i);
            yield return null;
        }

        animator.SetBool(IsShooting, true);
        StopNav();
        cooldown.Reset();
        SetState(State.Shooting);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(creek);
        while (!inTracke)
        {
            var direction = (target.transform.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), .2f);
            yield return null;
        }

        rb.isKinematic = false;
        animator.SetBool(IsShooting, false);
        var time = 0f;
        trailRenderer.emitting = true;
        var playedWoosh = false;
        var originalPos = transform.position;
        rb.constraints |= RigidbodyConstraints.FreezePositionY;
        rb.useGravity = false;
        var upPos = new Vector3(originalPos.x, originalPos.y + 2f, originalPos.z);
        for (var i = 0f; i < 1; i += .1f)
        {
            rb.MovePosition(Vector3.Lerp(originalPos, upPos, i));
            yield return null;
        }
        while (!target.IsDead && time <= 1f && !trackeHit)
        {
            rb.AddForce(transform.forward * trackeForce, ForceMode.VelocityChange);
            yield return null;
            time += Time.deltaTime;
            if (!playedWoosh && time > .1f)
            {
                playedWoosh = true;
                CameraAudioSource.Instance.AudioSource.PlayOneShot(whoosh);
            }
        }

        time = 0;
        while (time < 3f
               && !trackeHit
               && navMeshAgent.FindClosestEdge(out var border)
               &&  border.distance > .5f)
        {
            yield return null;
            time += Time.deltaTime;
        }

        foreach (var spiderLegConstraint in GetComponentsInChildren<SpiderLegConstraint>())
            spiderLegConstraint.enabled = true;

        trailRenderer.emitting = false;
        myCollider.enabled = true;
        hitCollider.enabled = false;
        damage.damage /= 2;
        damage.force /= 2;
        rb.velocity /= 3;
        rb.constraints = constraints;
        rb.isKinematic = true;
        rb.useGravity = useGravity;
        rb.MovePosition(new Vector3(transform.position.x, originalPos.y, transform.position.z));
        yield return new WaitForSeconds(.5f);
        inTracke = false;
        navMeshAgent.ResetPath();
        enemyEffects.RestoreMaterials();
        SetState(State.Seeking);
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
            if (navMeshAgent.isActiveAndEnabled &&
                (navMeshAgent.remainingDistance <= myCollider.size.z / 2
                 || !navMeshAgent.isOnNavMesh))
                navMeshAgent.SetDestination(nearPoint());
        }

    }

    void Walk()
    {
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        StartNav();
    }

    public void TakeDamage(float amount) => life.Subtract(amount);

    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (currentState == State.Shooting)
        {
            TakeDamage(amount);
            return;
        }

        if (!navMeshAgent.isStopped)
        {
            StopNav();
            Invoke(nameof(Walk), .5f);
        }

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        var direction = (transform.position - target.transform.position).normalized;
        rb.AddForce(direction * force * .8f, ForceMode.VelocityChange);

        if (amount > 0)
        {
            TakeDamage(amount);
            CameraAudioSource.Instance.AudioSource.PlayOneShot(hitSound);
            var i = Random.Range(0, bloodEffects.Length);
            var blood = Instantiate(bloodEffects[i], @from, transform.rotation);
            blood.transform.Rotate(Vector3.up, Random.rotation.eulerAngles.y);
            blood.transform.localScale *= 2f;
            Destroy(blood, 8);

            if (currentState != State.Seeking && currentState != State.Shooting)
                SetState(State.Seeking);
        }
    }

    Coroutine blink;
    IEnumerator Blink()
    {
        enemyEffects.UseDeflectShader();
        yield return new WaitForSeconds(.2f);
        enemyEffects.RestoreMaterials();
    }

    void OnCollisionEnter(Collision other)
    {
        if (!inTracke) return;

        if (other.transform.CompareTag("Projectile"))
        {
            var bulletRb = other.transform.GetComponent<Rigidbody>();
            var mag = bulletRb.velocity.magnitude;
            var currentDir = (transform.position - other.transform.position).normalized;
            var dir = Vector3.Reflect(currentDir, other.contacts[0].normal);
            CameraAudioSource.Instance.AudioSource.PlayOneShot(deflectSound);
            if (blink != null) StopCoroutine(blink);
            blink = StartCoroutine(Blink());
            other.transform.rotation = Quaternion.LookRotation(dir);
            bulletRb.velocity = new Vector3(dir.x, currentDir.y, dir.y) * mag + rb.velocity;
            return;
        }

        if (Utils.IsInLayerMask(other.gameObject, layersToTrackleIgnore)) return;
        if (!trackeHit)
        {
            CameraAudioSource.Instance.AudioSource.PlayOneShot(impact);
            Destroy(Instantiate(trackEffect, other.contacts[0].point, Quaternion.identity), .5f);
            trackeHit = true;
        }
        rb.velocity = Vector3.zero;
    }

}

