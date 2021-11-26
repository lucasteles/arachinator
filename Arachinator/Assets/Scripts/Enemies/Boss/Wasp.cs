using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Cameras.Effects;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(WaspEffects))]
public class Wasp : MonoBehaviour, IEnemy, IDamageble
{
    public enum WaspState
    {
        Sleep,
        Awake,
        Seeking,
        RunningAway
    }

    [SerializeField] float timeToSeek = 5;
    [SerializeField] float seekSpeed = 3;

    [SerializeField] AudioClip awake;
    [SerializeField] AudioClip roar;
    [SerializeField] CameraShakeData roarShake;

    [Header("Fly")]
    [SerializeField] AnimationCurve takeOffCurve;
    [SerializeField] float flyOffset = 4;
    [SerializeField] float takeOffSpeed = .02f;
    [SerializeField] AudioClip takeOffSound;
    [SerializeField] AudioClip takeOffWhoosh;
    [SerializeField] AudioSource zunido;
    [SerializeField] GameObject body;
    [SerializeField] Transform[] flyPoints;
    [SerializeField] float airShakeSize;


    WaspEffects waspEffects;
    AudioSource audioSource;
    WaspState currentState = WaspState.Sleep;
    Player player;
    Life playerLife;
    Vector3 initialPos;
    bool inFly;
    bool shouldShake;

    public WaspState CurrentState => currentState;
    void Awake()
    {
        waspEffects = GetComponent<WaspEffects>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
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
        StopAllCoroutines();
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
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
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
        var pos = transform.position;
        var targetPos = new Vector3(pos.x, pos.y + flyOffset, pos.z);
        audioSource.PlayOneShot(takeOffSound);
        audioSource.PlayOneShot(takeOffWhoosh);
        for (var i = 0f; i < 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            yield return null;
        }

        inFly = shouldShake = true;
        zunido.Play();
        yield return new WaitForSeconds(2f);
    }

    void RunAway()
    {
        currentState = WaspState.RunningAway;
    }

    public void LookAtPlayer()
    {
        currentState = WaspState.Awake;

        IEnumerator Looking()
        {
            yield return new WaitForSeconds(.5f);
            var dir = (player.transform.position - transform.position).normalized;
            while (Vector3.Dot(transform.forward, dir) < .8f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), .06f);
                yield return null;
                dir = (player.transform.position - transform.position).normalized;
            }

            SetState(WaspState.Seeking);
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
                transform.position += transform.forward * seekSpeed * Time.deltaTime;
                yield return null;
            }

            yield return TakeOff();
            //SetState(WaspState.Awake);
        }

        StartCoroutine(Follow());
    }


    public void SetConfiguration(EnemyConfiguration configuration) { }

    public bool ShouldDeflect { get; } = false;
    public void TakeHit(float amount, Vector3 @from, float force) { }

    public void TakeDamage(float amount) { }

    public void Reset() { }
}
