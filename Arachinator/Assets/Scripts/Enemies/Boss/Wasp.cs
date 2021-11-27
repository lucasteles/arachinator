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
        [WaspState.Seeking] = (0, 90),
        [WaspState.RunningAway] = (90, 100),
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
    float damageAcumulator;
    bool inFly;
    bool shouldShake;
    bool damageBlinking;
    bool deflectBlinking;
    bool invincible;

    public WaspState CurrentState => currentState;
    void Awake()
    {
        waspEffects = GetComponent<WaspEffects>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        rb = GetComponent<Rigidbody>();
        life = GetComponent<Life>();
        life.onLifeChange += onLifeChange;
        life.onSubtract += onLifeSubtract;
        wave.OnWaveEnded += WaveOnOnWaveEnded;
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

    public void AwakeBoss() => StartCoroutine(Awakening());

    IEnumerator Awakening()
    {
        audioSource.PlayOneShot(awake);
        yield return waspEffects.OpenEyes();

        var dir = (player.transform.position - transform.position).normalized;
        var rot = transform.rotation;
        for (var i = 0f; i <= 1; i+=.05f)
        {
            transform.rotation = Quaternion.Lerp(rot, Quaternion.LookRotation(dir),i);
            yield return null;
        }

        yield return Roar();

        SetState(WaspState.Awake);
    }

    void SetState(WaspState newState)
    {
        RestoreDefaults();

        switch (newState)
        {
            case WaspState.Sleep:
                break;
            case WaspState.Awake:
                LookAtPlayer();
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

        IEnumerator WaveIt()
        {
            audioSource.PlayOneShot(hurt);
            if (inFly)
                yield return Land();
            yield return WaitLooking(1f);
            yield return Roar();
            var flyingAround = StartCoroutine(GoToFarPoint(true));

            yield return wave.Spawn(spawnPoints, player.transform);

            yield return new WaitUntil(() => currentState != WaspState.SpawnEnemies);
            StopCoroutine(flyingAround);
        }

        StartCoroutine(WaveIt());
    }

    private void RestoreDefaults()
    {
        StopAllCoroutines();
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
        airShootCooldown.Reset();
        var pos = transform.position;
        var targetPos = new Vector3(pos.x, pos.y + flyOffset, pos.z);
        audioSource.PlayOneShot(takeOffSound);
        audioSource.PlayOneShot(takeOffWhoosh);
        for (var i = 0f; i <= 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            transform.LookAt(player.transform);
            yield return null;
        }

        inFly = shouldShake = true;
        zunido.Play();
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
                var target = point + Utils.SimpleCurve(i) * curveStrength * Vector3.down;
                transform.position = Vector3.Lerp(pos, target, moveCurve.Evaluate(i));
                transform.LookAt(player.transform);
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
            transform.LookAt(player.transform);
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
        zunido.Stop();
        var pos = transform.position;
        var targetPos = new Vector3(pos.x, initialPos.y, pos.z);
        audioSource.PlayOneShot(takeOffSound);
        audioSource.PlayOneShot(takeOffWhoosh);
        for (var i = 0f; i <= 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            yield return null;
        }
        transform.position = targetPos;
        audioSource.PlayOneShot(landSound);
        inFly = false;
    }

    public void LookAtPlayer()
    {
        currentState = WaspState.Awake;

        IEnumerator Looking()
        {
            yield return new WaitForSeconds(.2f);
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


    IEnumerator Roar()
    {
        audioSource.PlayOneShot(roar);
        roarShake.timeToShake = roar.length;
        CameraShaker.Instance.Shake(roarShake);
        yield return new WaitForSeconds(roar.length);
    }

    public void StartFollow()
    {
        currentState = WaspState.Seeking;
        IEnumerator Follow()
        {
            var timer = Time.time + timeToSeek;
            while (!playerLife.IsDead && Time.time <= timer)
            {
                transform.LookAt(player.transform);
                transform.position = (transform.position + transform.forward * seekSpeed * Time.deltaTime);
                yield return null;
            }
            SetState(WaspState.Awake);
        }

        StartCoroutine(Follow());
    }

    public void SetConfiguration(EnemyConfiguration configuration) { }

    public bool ShouldDeflect => invincible;

    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (currentState == WaspState.Sleep || invincible)
        {
            CameraAudioSource.Instance.AudioSource.PlayOneShot(deflectSound);
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
        if (!invincible || currentState != WaspState.Sleep)
            return;

        if (!other.transform.CompareTag("Projectile")) return;

        var bulletRb = other.transform.GetComponent<Rigidbody>();
        var mag = bulletRb.velocity.magnitude;
        var currentDir = (transform.position - other.transform.position).normalized;
        var dir = Vector3.Reflect(currentDir, other.contacts[0].normal);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(deflectSound);
        other.transform.rotation = Quaternion.LookRotation(dir);
        bulletRb.velocity = new Vector3(dir.x, currentDir.y, dir.y) * mag + rb.velocity;
        StartCoroutine(BlinkReflect());
    }

    public void Reset()
    {
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

}
