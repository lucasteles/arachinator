using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Cameras.Effects;
using UnityEngine;
using UnityEngine.AI;

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

    [SerializeField] float timeToSeek;
    [SerializeField] AudioClip awake;
    [SerializeField] AudioClip roar;
    [SerializeField] CameraShakeData roarShake;

    [Header("Fly")]
    [SerializeField] AnimationCurve takeOffCurve;
    [SerializeField] float flyOffset = 4;
    [SerializeField] float takeOffSpeed = .02f;
    [SerializeField] Transform[] flyPoints;

    WaspEffects waspEffects;
    AudioSource audioSource;
    WaspState currentState = WaspState.Sleep;
    Player player;
    Life playerLife;
    NavMeshAgent navMeshAgent;
    Vector3 initialPos;

    public WaspState CurrentState => currentState;
    void Awake()
    {
        waspEffects = GetComponent<WaspEffects>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
        playerLife = player.GetComponent<Life>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        initialPos = transform.position;
    }

    void Update()
    {
        
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

       // yield return Roar();

        yield return TakeOff();
        //SetState(WaspState.Awake);
    }

    void StopNav()
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isStopped == false)
            navMeshAgent.isStopped = true;
    }

    void SetState(WaspState newState)
    {
        print($"{currentState} -> {newState}");
        StopAllCoroutines();
        StopNav();
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

    IEnumerator TakeOff()
    {
        var pos = transform.position;
        var targetPos = new Vector3(pos.x, pos.y + flyOffset, pos.z);
        for (var i = 0f; i < 1; i+=takeOffSpeed)
        {
            transform.position = Vector3.Lerp(pos, targetPos, takeOffCurve.Evaluate(i));
            yield return null;
        }

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
                yield return new WaitForSeconds(.1f);
                navMeshAgent.SetDestination(player.transform.position);
            }
            print(1);
            SetState(WaspState.Awake);
        }

        if (navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = false;
            StartCoroutine(Follow());
        }
    }


    public void SetConfiguration(EnemyConfiguration configuration) { }

    public bool ShouldDeflect { get; } = false;
    public void TakeHit(float amount, Vector3 @from, float force) { }

    public void TakeDamage(float amount) { }

    public void Reset() { }
}
