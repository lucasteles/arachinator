using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Cameras.Effects;
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
    [SerializeField] CameraShakeData roarShake;
    [SerializeField] GameObject hitEffect;
    [SerializeField] AudioClip hitSound;

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



    WaspEffects waspEffects;
    AudioSource audioSource;
    WaspState currentState = WaspState.Sleep;
    Player player;
    Life playerLife;
    Vector3 initialPos;
    Vector3 velocity;
    Rigidbody rb;
    EnemyEffects enemyEffects;
    bool inFly;
    bool shouldShake;
    bool blinking;

    public WaspState CurrentState => currentState;
    void Awake()
    {
        waspEffects = GetComponent<WaspEffects>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        enemyEffects = GetComponent<EnemyEffects>();
        rb = GetComponent<Rigidbody>();
    }


    void Start()
    {
        initialPos = transform.position;
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
        for (var i = 0f; i < 1; i+=.05f)
        {
            transform.rotation = Quaternion.Lerp(rot, Quaternion.LookRotation(dir),i);
            yield return null;
        }

        yield return Roar();

        SetState(WaspState.Awake);
    }

    void SetState(WaspState newState)
    {
        print($"{currentState} -> {newState}");
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
        }

    }

    private void RestoreDefaults()
    {
        StopAllCoroutines();
        if (blinking)
        {
            enemyEffects.RestoreMaterials();
            blinking = false;
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
        for (var i = 0f; i < 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            transform.LookAt(player.transform);
            yield return null;
        }

        inFly = shouldShake = true;
        zunido.Play();
    }

    IEnumerator GoToFarPoint()
    {
        var curveStrength = 2f;
        var numberOfSteps = Random.Range(1, maxAirMoveCount);
        for (var j = 0; j < numberOfSteps; j++)
        {
            var point = GetFarPoint();
            var pos = transform.position;

            shouldShake = false;
            for (var i = 0f; i < 1; i+=airMovementSpeed)
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
        for (var i = 0f; i < 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            yield return null;
        }
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

    public bool ShouldDeflect { get; } = false;

    public void TakeHit(float amount, Vector3 @from, float force)
    {
        if (currentState == WaspState.Sleep) return;

        TakeDamage(amount);
        if (Random.Range(-1,1) == 0)
            CameraAudioSource.Instance.AudioSource.PlayOneShot(hitSound);

        var blood = Instantiate(hitEffect, @from, transform.rotation);
        blood.transform.Rotate(Vector3.up,Random.rotation.eulerAngles.y);
        blood.transform.localScale *= 4f;
        Destroy(blood, 4);
        StartCoroutine(Blink());
    }

    public void TakeDamage(float amount) { }

    public void Reset() { }

    IEnumerator Blink()
    {
        blinking = true;
        enemyEffects.UseDeflectShader();
        yield return new WaitForSeconds(.2f);
        enemyEffects.RestoreMaterials();
        blinking = false;
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
