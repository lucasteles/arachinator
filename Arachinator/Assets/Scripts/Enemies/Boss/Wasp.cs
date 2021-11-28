using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Cameras.Effects;
using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(WaspEffects))]
public class Wasp : MonoBehaviour, IEnemy, IDamageble
{
    public enum WaspState
    {
        Sleep,
        Awake,
        Seeking,
        RunningAway,
        RunningAwayAndShoot,
        SpawnEnemies,
        Shoot,
    }

    public Dictionary<WaspState, (int from, int to)> Actions =
        new Dictionary<WaspState, (int, int)>
    {
        [WaspState.Seeking] = (0, 50),
        [WaspState.RunningAway] = (50, 100),
        [WaspState.RunningAwayAndShoot] = (0,0),
        [WaspState.Shoot] = (0,0),
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

    [Header("Shooting")]
    [SerializeField] Cooldown airShootCooldown;
    [SerializeField] AudioClip projectileSound;
    [SerializeField] Transform shootPoint;

    [Header("Wave")]
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
        animationManager = GetComponentInChildren<WaspAnimationManager>();
        life.onLifeChange += onLifeChange;
        life.onSubtract += onLifeSubtract;
        wave.OnWaveEnded += WaveOnOnWaveEnded;
        collider = GetComponent<CapsuleCollider>();
        legConstraintCache = GetComponentsInChildren<SpiderLegConstraint>();
    }

    void WaveOnOnWaveEnded()
    {
        if (currentState == WaspState.SpawnEnemies)
            SetState(WaspState.Awake);

        if (!wave.NextWave())
            damageAcumulator = float.NegativeInfinity;
    }

    void onLifeSubtract(float damage)
    {
        damageAcumulator += damage;
        if (damageAcumulator >= (.25 * life.MaxLife))
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
        ObjectPooling.Get(Pools.FireBall, shootPoint.position, transform.rotation);
    }

    public void AwakeBoss()
    {
        if (firstEncounter)
        {
            StartCoroutine(Awakening());
            firstEncounter = false;
        }
        else
            SetState(WaspState.Awake);
    }

    IEnumerator Awakening()
    {
        audioSource.PlayOneShot(awake);
        animationManager.OpenWings();
        var iddle = StartCoroutine(animationManager.Iddle());
        yield return waspEffects.OpenEyes();
        var dir = (player.transform.position - transform.position).normalized;
        var rot = transform.rotation;
        EnableLeg();
        for (var i = 0f; i <= 1; i+=.04f)
        {
            transform.rotation = Quaternion.Lerp(rot, Quaternion.LookRotation(dir),i);
            yield return null;
        }
        DisableLeg();
        yield return iddle;
        yield return Roar();
        SetState(WaspState.Awake);
    }

    void SetState(WaspState newState)
    {
        RestoreDefaults();
        print($"{currentState} -> {newState}");
        switch (newState)
        {
            case WaspState.Sleep:
                break;
            case WaspState.Awake:
                Iddle();
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
        }

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
            yield return WaitLooking(1f);
            yield return Roar();
            yield return TakeOff();
            var flyingAround = StartCoroutine(GoToFarPoint(true));

            yield return wave.Spawn(spawnPoints, player.transform);

            invincible = false;
            yield return new WaitUntil(() => currentState != WaspState.SpawnEnemies);
            StopCoroutine(flyingAround);
        }

        StartCoroutine(WaveIt());
    }

    private void RestoreDefaults()
    {
        StopAllCoroutines();
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

    public void Shake()
    {
        var x = Random.Range(-1, 2) * airShakeSize;
        var y = Random.Range(-1, 2) * airShakeSize;
        var z = Random.Range(-1, 2) * airShakeSize;
        body.transform.localPosition = new Vector3(x, y, z);
    }

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


    IEnumerator GoToFarPoint(bool forever = false)
    {
        var curveStrength = 2f;
        var numberOfSteps = Random.Range(1, maxAirMoveCount);
        for (var j = 0; j <= numberOfSteps; j++)
        {
            if (forever) j = 0;
            var point = GetFarPoint();
            var pos = transform.position;

            shouldShake = false;
            for (var i = 0f; i <= 1; i+=airMovementSpeed)
            {
                transform.position = Vector3.Lerp(pos, point, moveCurve.Evaluate(i));
                LookAtPlayer();
                yield return null;

            }
            if (j < numberOfSteps - 1)
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
            .Take(4)
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

    IEnumerator RunAwayCoroutine()
    {
        yield return TakeOff();
        yield return WaitLooking(.5f);
        yield return GoToFarPoint();
        yield return WaitLooking(1f);
        yield return Land();
    }
    void RunAway()
    {
        IEnumerator routine()
        {
            yield return RunAwayCoroutine();
            yield return WaitLooking(1f);
            SetState(WaspState.Awake);
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

    public void Iddle()
    {
        currentState = WaspState.Awake;

        IEnumerator Looking()
        {
            if (Random.Range(-1, 2) == 0)
                 yield return animationManager.Iddle();
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

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var c = Gizmos.color;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roarPushBackRadius);
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
            while (!playerLife.IsDead && Time.time <= timer)
            {
                LookAtPlayer();
                transform.position = (transform.position + transform.forward * seekSpeed * Time.deltaTime);
                yield return null;
            }
            DisableLeg();
            SetState(WaspState.Awake);
        }

        StartCoroutine(Follow());
    }

    public void SetConfiguration(EnemyConfiguration configuration) { }

    public bool ShouldDeflect => invincible || currentState == WaspState.Sleep;

    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (currentState == WaspState.Sleep || invincible)
        {
            return;
        }

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
        zunido.Stop();
        damageAcumulator = 0;
        transform.position = initialPos;
        transform.rotation = initialRot;
        wave.Reset();
        life.Reset();
        invincible = false;
        inFly = shouldShake = damageBlinking = false;

        SetState(WaspState.Sleep);
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
            spiderLegConstraint.Reset();
            spiderLegConstraint.enabled = false;
        }
    }
    void EnableLeg()
    {
        foreach (var spiderLegConstraint in legConstraintCache)
            spiderLegConstraint.enabled = true;
    }

}
