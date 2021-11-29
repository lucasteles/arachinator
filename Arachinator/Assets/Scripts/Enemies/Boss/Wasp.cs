using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Cameras.Effects;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(WaspEffects))]
public class Wasp : MonoBehaviour, IEnemy, IDamageble
{
    public enum WaspState
    {
        Sleep,
        Idle,
        Seeking,
        RunningAway,
        RunningAwayAndShoot,
        SpawnEnemies,
        Shoot,
        Dying,
        Dead,
    }

    public Dictionary<WaspState, (int from, int to)> Actions =
        new Dictionary<WaspState, (int, int)>
    {
        [WaspState.Seeking] = (0, 40),
        [WaspState.RunningAwayAndShoot] = (40,80),
        [WaspState.Shoot] = (80,90),
        [WaspState.RunningAway] = (90, 100),
    };

    [SerializeField] float timeToSeek = 5;
    [SerializeField] float seekSpeed = 3;

    [SerializeField] AudioClip awake;
    [SerializeField] AudioClip roar;
    [SerializeField] AudioClip hurt;
    [SerializeField] AudioClip deflectSound;
    [SerializeField] CameraShakeData roarShake;
    [SerializeField] GameObject hitEffect;
    [SerializeField] AudioClip hitSound;
    [SerializeField] PlayerHealthPointsUi heathBar;
    [SerializeField] EnemyEffects damageEffect;
    [SerializeField] EnemyEffects reflectEffect;
    [SerializeField] ParticleSystem dust;
    [SerializeField] float roarPushBack;
    [SerializeField] float roarPushBackRadius;

    [Header("Fly")]
    [SerializeField] AnimationCurve takeOffCurve;
    [SerializeField] float flyOffset = 4;
    [SerializeField] float takeOffSpeed = .02f;
    [SerializeField] AudioClip takeOffSound;
    [SerializeField] AudioClip takeOffWhoosh;
    [SerializeField] AudioClip landSound;
    [SerializeField] AudioSource zunido;
    [SerializeField] GameObject body;
    [SerializeField] Transform[] flyPoints;
    [SerializeField] float airShakeSize;
    [SerializeField] AnimationCurve moveCurve;
    [SerializeField] float airMovementSpeed;
    [SerializeField] int maxAirMoveCount = 4;
    [SerializeField] float timeBetweenDeathExplosions =.5f;

    [Header("Shooting")]
    [SerializeField] Cooldown airShootCooldown;
    [SerializeField] AudioClip projectileSound;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform shootPointGrounded;
    [SerializeField] float shootingArcDregrees = 45;
    [SerializeField] float shootingArcSpeed = .1f;
    [SerializeField] float shootingArcSpeedCenter = .1f;
    [SerializeField] int maxShootingTimes = 3;
    [SerializeField] float lastWaveShootCooldown = 10;
    [SerializeField] Transform centerPoint;
    [SerializeField] float centerPointRadiusToShootSlow;


    [Header("Attack")]
    [SerializeField] float distanceToAttackWhileSeeking = 3;
    [SerializeField] float attackForwardDistance = 3;
    [SerializeField] float attackForwardSpeed = .1f;
    [SerializeField] Cooldown attackCoodown;
    [SerializeField] EnemyDamageDealer basicDamageDealer;


    [Header("Wave")]
    [SerializeField] bool spawnWaves = true;
    [SerializeField] WaveController wave;
    [SerializeField] GameObject[] spawnPoints;

    WaspEffects waspEffects;
    AudioSource audioSource;
    WaspState currentState = WaspState.Sleep;
    Player player;
    Life playerLife;
    Vector3 initialPos;
    Quaternion initialRot;
    Vector3 velocity;
    Rigidbody rb;
    Life life;
    WaspAnimationManager animationManager;
    float damageAcumulator;
    bool inFly;
    bool shouldShake;
    bool damageBlinking;
    bool deflectBlinking;
    bool invincible;
    bool firstEncounter = true;
    CapsuleCollider collider;

    SpiderLegConstraint[] legConstraintCache;

    public WaspState CurrentState => currentState;
    void Awake()
    {
        waspEffects = GetComponent<WaspEffects>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        rb = GetComponent<Rigidbody>();
        life = GetComponent<Life>();
        collider = GetComponent<CapsuleCollider>();
        legConstraintCache = GetComponentsInChildren<SpiderLegConstraint>();
        animationManager = GetComponentInChildren<WaspAnimationManager>();

        life.onLifeChange += onLifeChange;
        life.onSubtract += onLifeSubtract;
        life.onDeath += OnDeath;
        wave.OnWaveEnded += WaveOnOnWaveEnded;
        animationManager.onShoot += OnShoot;
        animationManager.onAttack += AnimationManagerOnonAttack;

    }

    void AnimationManagerOnonAttack()
    {
         IEnumerator action()
         {
             var pos = transform.position;
             var targetPos = pos + (player.transform.position - transform.position).normalized * attackForwardDistance;
             for (var i = 0f; i <= 1; i += attackForwardSpeed)
             {
                 transform.position = Vector3.Lerp(pos, targetPos, i);
                 yield return null;
             }
         }
        StartCoroutine(action());
    }

    void OnDeath(Life obj) => SetState(WaspState.Dying);

    void WaveOnOnWaveEnded()
    {
        if (currentState == WaspState.SpawnEnemies)
            SetState(WaspState.Idle);

        if (!wave.NextWave())
            damageAcumulator = float.NegativeInfinity;
    }

    void onLifeSubtract(float damage)
    {
        damageAcumulator += damage;
        if (spawnWaves && damageAcumulator >= ((1f/wave.Waves.Length) * life.MaxLife))
        {
            damageAcumulator = 0;
            SetState(WaspState.SpawnEnemies);
        }

    }

    void OnDestroy()
    {
        life.onLifeChange -= onLifeChange;
        life.onSubtract -= onLifeSubtract;
    }

    void onLifeChange(float arg1, float arg2)
    {
        if (!heathBar) return;
        heathBar.SetMaxHealth(life.MaxLife);
        heathBar.SetHealth(life.CurrentLife);

    }

    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
        damageAcumulator = 0;
        DisableLeg();
    }
    void Update()
    {
        if (currentState == WaspState.Sleep) return;
        if (shouldShake && inFly)
            Shake();

        if (inFly && airShootCooldown && Random.Range(-1,1) == 0)
        {
            airShootCooldown.Reset();
            AirShoot();
        }

    }

    void AirShoot()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(projectileSound);
        ObjectPooling.Get(Pools.FireBall, shootPoint.position, shootPoint.rotation);
    }

    public void AwakeBoss()
    {
        if (firstEncounter)
        {
            StartCoroutine(Awakening());
            firstEncounter = false;
        }
        else
            SetState(WaspState.Idle);
    }

    IEnumerator Awakening()
    {
        audioSource.PlayOneShot(awake);
        yield return new WaitForSeconds(.5f);
        animationManager.OpenWings();
        var idle = StartCoroutine(animationManager.Idle());
        yield return waspEffects.OpenEyes();
        yield return new WaitForSeconds(.2f);
        var dir = (player.transform.position - transform.position).normalized;
        var rot = transform.rotation;
        EnableLeg();
        for (var i = 0f; i <= 1; i+=.02f)
        {
            transform.rotation = Quaternion.Lerp(rot, Quaternion.LookRotation(dir),i);
            yield return null;
        }
        DisableLeg();
        yield return idle;
        yield return Roar();
        SetState(WaspState.Idle);
    }

    IEnumerator Attack()
    {
        basicDamageDealer.active = false;
        EnableLeg();
        yield return animationManager.BeginAttack();
        //collider.enabled = true;
        basicDamageDealer.active = true;
        DisableLeg();
    }

    void SetState(WaspState newState)
    {
        RestoreDefaults();
        //print($"{currentState} -> {newState}");
        switch (newState)
        {
            case WaspState.Sleep:
                break;
            case WaspState.Idle:
                Idle();
                break;
            case WaspState.Seeking:
                StartFollow();
                break;
            case WaspState.RunningAway:
                RunAway();
                break;
            case WaspState.SpawnEnemies:
                StartWave();
                break;
            case WaspState.Shoot:
                StartShooting();
                break;
            case WaspState.RunningAwayAndShoot:
                RunawyAndShoot();
                break;
            case WaspState.Dying:
                BeginDeath();
                break;
        }

    }
    void BeginDeath()
    {
        currentState = WaspState.Dying;

        IEnumerator action()
        {
            DisableLeg();
            collider.enabled = false;
            if (inFly) yield return Land();
            var bloodExplodins = StartCoroutine(waspEffects.BloodExplosions(hitEffect, timeBetweenDeathExplosions));
            yield return animationManager.BeginDeath();
            yield return waspEffects.CloseEyes();
            StopCoroutine(bloodExplodins);
            yield return waspEffects.Dissolve();
            currentState = WaspState.Dead;
        }
        StartCoroutine(action());
    }


    void RunawyAndShoot()
    {
        currentState = WaspState.RunningAwayAndShoot;
        IEnumerator action()
        {
            yield return TakeOff();
            yield return GoToFarPoint();
            yield return Land();
            yield return MultiShooting();
            SetState(WaspState.Idle);
        }

        StartCoroutine(action());
    }

    IEnumerator Shooting()
    {
        var degrees = shootingArcDregrees;
        var playerPos = player.transform.position;
        var targetPos = new Vector3(playerPos.x, transform.position.y, playerPos.z);
        var playerDirection = (targetPos - transform.position).normalized;
        var left = Quaternion.AngleAxis(-degrees, Vector3.up) * playerDirection;
        var right = Quaternion.AngleAxis(degrees, Vector3.up) * playerDirection;
        var currentRot = transform.rotation;

        // IEnumerator debug()
        // {
        //     while (true)
        //     {
        //         Debug.DrawLine(transform.position, transform.position + (playerDirection * 20), Color.green);
        //         Debug.DrawLine(transform.position, transform.position + (left * 20), Color.yellow);
        //         Debug.DrawLine(transform.position, transform.position + (right * 20), Color.yellow);
        //         yield return null;
        //     }
        // }
        //var d = StartCoroutine(debug());

        EnableLeg();
        var lookingLeft = Vector2.Dot(new Vector2(playerDirection.x, playerDirection.z),
            new Vector2(transform.right.x, transform.right.z)) > 0;
        var (from, to) = lookingLeft ? (left.normalized, right.normalized) : (right.normalized, left.normalized);
        for (var i = 0f; i <= 1; i += .2f)
        {
            transform.rotation = Quaternion.Lerp(currentRot, Quaternion.LookRotation(from), i);
            yield return null;
        }

        var arcSpeed =
            (Vector3.Distance(new Vector2(transform.position.x, transform.position.z),
                new Vector2(centerPoint.position.x, centerPoint.position.z)) <= centerPointRadiusToShootSlow)
                ? shootingArcSpeedCenter
                : shootingArcSpeed;

        animationManager.StartShooting();
        for (var i = 0f; i <= 1; i += arcSpeed)
        {
            transform.rotation = Quaternion.Lerp(Quaternion.LookRotation(from), Quaternion.LookRotation(to), i);
            yield return null;
        }

        animationManager.StopShooting();
        DisableLeg();
        //StopCoroutine(d);
    }

    IEnumerator MultiShooting()
    {
        var times = Random.Range(1, maxShootingTimes);
        for (var i = 0; i < times; i++)
        {
            yield return Shooting();
            yield return WaitLooking(.2f);
        }
    }

    void StartShooting()
    {
        IEnumerator shoot()
        {
            yield return Shooting();
            SetState(WaspState.Idle);
        }
        currentState = WaspState.Shoot;
        StartCoroutine(shoot());
    }

    void StartWave()
    {
        currentState = WaspState.SpawnEnemies;
        invincible = true;
        IEnumerator WaveIt()
        {
            audioSource.PlayOneShot(hurt);
            if (inFly)
                yield return Land();
            yield return WaitLooking(.5f);
            yield return Roar();
            yield return TakeOff();
            var flyingAround =
                wave.Last()
                ? StartCoroutine(FlyAroundAndGroundShootForever())
                : StartCoroutine(GoToFarPoint(int.MaxValue));

            yield return wave.Spawn(spawnPoints, player.transform);
            yield return new WaitUntil(() => currentState != WaspState.SpawnEnemies);
            StopCoroutine(flyingAround);
            invincible = false;
        }

        StartCoroutine(WaveIt());
    }

    IEnumerator FlyAroundAndGroundShootForever()
    {
        var shootCooldown = new Cooldown(lastWaveShootCooldown);
        invincible = true;
        while (true)
        {
            yield return GoToFarPoint();
            if (shootCooldown)
            {
                yield return Land();
                yield return MultiShooting();
                yield return TakeOff();
            }
            yield return WaitLooking(.5f);
        }
    }

    void RestoreDefaults()
    {
        StopAllCoroutines();
        animationManager.StopShooting();
        invincible = false;
        collider.enabled = true;
        if (damageBlinking)
        {
            damageEffect.RestoreMaterials();
            damageBlinking = false;
        }

        if (deflectBlinking)
        {
            reflectEffect.RestoreMaterials();
            deflectBlinking = false;
        }
    }


    void Shake() =>
        body.transform.localPosition = Utils.RandomVector3(-1, 2) * airShakeSize;

    IEnumerator TakeOff()
    {
        yield return animationManager.TakeOff();
        airShootCooldown.Reset();
        collider.enabled = false;
        var pos = transform.position;
        var targetPos = new Vector3(pos.x, initialPos.y + flyOffset, pos.z);
        audioSource.PlayOneShot(takeOffSound);
        audioSource.PlayOneShot(takeOffWhoosh);
        for (var i = 0f; i <= 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            LookAtPlayer();
            yield return null;
        }

        inFly = shouldShake = true;
        zunido.Play();
        yield return animationManager.InFly();
        collider.enabled = true;
    }


    IEnumerator GoToFarPoint(int qtd = 1)
    {
        for (var j = 0; j <= qtd; j++)
        {
            var point = GetFarPoint();
            var pos = transform.position;

            shouldShake = false;
            for (var i = 0f; i <= 1; i+=airMovementSpeed)
            {
                transform.position = Vector3.Lerp(pos, point, moveCurve.Evaluate(i));
                LookAtPlayer();
                yield return null;

            }
            if (j < qtd - 1)
                yield return WaitLooking(1);
            shouldShake = true;
        }
    }

    Vector3 GetFarPoint()
    {
        var playerPos = player.transform.position;
        var all = flyPoints
            .Select(x => (position: x.position, distance: Vector3.Distance(x.position, playerPos)))
            .OrderByDescending(x => x.distance)
            .Take(6)
            .ToArray();
        var point = all[Random.Range(0, all.Length)];
        return new Vector3(point.position.x, transform.position.y, point.position.z);
    }

    IEnumerator WaitLooking(float time)
    {
        var t = Time.time + time;
        var rot = transform.rotation;
        while (Time.time <= t)
        {
            LookAtPlayer();
            yield return null;
        }
        transform.rotation = rot;
    }

    void RunAway()
    {
        IEnumerator routine()
        {
            yield return TakeOff();
            yield return WaitLooking(.5f);
            var numberOfSteps = Random.Range(1, maxAirMoveCount);
            yield return GoToFarPoint(numberOfSteps);
            yield return WaitLooking(1f);
            yield return Land();

            yield return WaitLooking(1f);
            SetState(WaspState.Idle);
        }
        currentState = WaspState.RunningAway;
        StartCoroutine(routine());
    }

    IEnumerator Land()
    {
        shouldShake = false;
        StartCoroutine(animationManager.Land());
        var pos = transform.position;
        var targetPos = new Vector3(pos.x, initialPos.y, pos.z);
        audioSource.PlayOneShot(takeOffSound);
        audioSource.PlayOneShot(takeOffWhoosh);
        for (var i = 0f; i <= 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            yield return null;
        }
        inFly = false;
        transform.position = targetPos;
        audioSource.PlayOneShot(landSound);
        zunido.Stop();
    }

    void LookAtPlayer(bool fast = false)
    {
        var pos = player.transform.position;
        var targetPos = inFly
            ? player.transform.position
            : new Vector3(pos.x, transform.position.y, pos.z);

        if (fast)
            transform.LookAt(targetPos);
        else
        {
            var dir = (targetPos - transform.position).normalized;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), .1f);
        }
    }

    public void Idle()
    {
        currentState = WaspState.Idle;

        IEnumerator Looking()
        {
            if (inFly)
                yield return Land();

            if (Random.Range(-1, 1) == 0)
                 yield return IdleWait();
            else
                yield return WaitLooking(.2f);

            var dir = (player.transform.position - transform.position).normalized;
            while (Vector3.Dot(transform.forward, dir) < .8f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), .06f);
                yield return null;
                dir = (player.transform.position - transform.position).normalized;
            }

            SetState(GetRandomState());
        }

        StartCoroutine(Looking());
    }

    IEnumerator IdleWait()
    {
        var wait = StartCoroutine(WaitLooking(10));
        yield return animationManager.Idle();
        StopCoroutine(wait);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var c = Gizmos.color;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, roarPushBackRadius);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(new Vector3(centerPoint.position.x, transform.position.y, centerPoint.position.z), centerPointRadiusToShootSlow);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToAttackWhileSeeking);


        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * attackForwardDistance);

        Gizmos.color = c;
    }
#endif

    IEnumerator Roar()
    {
        yield return animationManager.Taunt();
        dust.Play();
        audioSource.PlayOneShot(roar);
        roarShake.timeToShake = roar.length;
        CameraShaker.Instance.Shake(roarShake);
        var wait = StartCoroutine(WaitLooking(roar.length));
        if (Physics.OverlapSphereNonAlloc(transform.position, roarPushBackRadius, new Collider[1], LayerMask.GetMask("Player")) > 0)
            for (var i = 0; i <= 3; i++)
            {
                player.PushBack(transform.position, roarPushBack);
                yield return new WaitForSeconds(.4f);
            }
        yield return wait;
        dust.Stop();
    }

    public void StartFollow()
    {
        currentState = WaspState.Seeking;
        IEnumerator Follow()
        {
            EnableLeg();
            var timer = Time.time + timeToSeek;
            var attacked = false;
            while (!playerLife.IsDead && Time.time <= timer)
            {
                LookAtPlayer();
                transform.position = (transform.position + transform.forward * seekSpeed * Time.deltaTime);
                if (attackCoodown && Vector3.Distance(transform.position, player.transform.position) <= distanceToAttackWhileSeeking)
                {
                    attacked = true;
                    attackCoodown.Reset();
                    yield return Attack();
                }
                yield return null;
            }

            if (!playerLife.IsDead && (!attacked || Random.Range(-1,1)==0))
                yield return Shooting();

            DisableLeg();
            SetState(WaspState.Idle);
        }

        StartCoroutine(Follow());
    }

    public void SetConfiguration(EnemyConfiguration configuration) { }

    public bool ShouldDeflect => invincible || currentState == WaspState.Sleep;

    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (currentState == WaspState.Sleep || invincible)
            return;

        TakeDamage(amount);
        if (Random.Range(-1,1) == 0)
            CameraAudioSource.Instance.AudioSource.PlayOneShot(hitSound);

        var blood = Instantiate(hitEffect, @from, transform.rotation);
        blood.transform.Rotate(Vector3.up,Random.rotation.eulerAngles.y);
        blood.transform.localScale *= 4f;
        Destroy(blood, 4);
        StartCoroutine(BlinkDamage());
    }

    public void TakeDamage(float amount) => life.Subtract(amount);

    void OnCollisionEnter(Collision other)
    {
        if (!ShouldDeflect) return;
        if (!other.transform.CompareTag("Projectile")) return;

        var bulletRb = other.transform.GetComponent<Rigidbody>();
        var mag = bulletRb.velocity.magnitude;
        var currentDir = (transform.position - other.transform.position).normalized;
        var dir = Vector3.Reflect(currentDir, other.contacts[0].normal);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(deflectSound);
        other.transform.rotation = Quaternion.LookRotation(dir);
        bulletRb.velocity = dir * mag * 2;
        StartCoroutine(BlinkReflect());
    }

    public void Reset()
    {
        RestoreDefaults();
        zunido.Stop();
        damageAcumulator = 0;
        transform.position = initialPos;
        transform.rotation = initialRot;
        wave.Reset();
        life.Reset();
        animationManager.Reset();
        inFly = shouldShake = damageBlinking = false;
        SetState(WaspState.Sleep);
        invincible = true;
    }

    IEnumerator BlinkDamage()
    {
        damageBlinking = true;
        damageEffect.UseDeflectShader();
        yield return new WaitForSeconds(.2f);
        damageEffect.RestoreMaterials();
        damageBlinking = false;
    }
    IEnumerator BlinkReflect()
    {
        deflectBlinking = true;
        reflectEffect.UseDeflectShader();
        yield return new WaitForSeconds(.2f);
        reflectEffect.RestoreMaterials();
        deflectBlinking = false;
    }

    WaspState GetRandomState()
    {
        var percent = Random.Range(0, 100);
        var action =
            Actions
                .Where(x => percent >= x.Value.from && percent <= x.Value.to)
                .OrderBy(x => x.Value)
                .First()
                .Key;

        return action;
    }

    void DisableLeg()
    {
        foreach (var spiderLegConstraint in legConstraintCache)
        {
            //spiderLegConstraint.Reset();
            spiderLegConstraint.enabled = false;
        }
    }
    void EnableLeg()
    {
        foreach (var spiderLegConstraint in legConstraintCache)
            spiderLegConstraint.enabled = true;
    }

    void OnShoot()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(projectileSound);
        ObjectPooling.Get(Pools.FireBall, shootPointGrounded.position, transform.rotation);
    }
}
