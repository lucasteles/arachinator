using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Cameras.Effects;
using UnityEngine;

[RequireComponent(typeof(WaspEffects))]
public class Wasp : MonoBehaviour, IEnemy, IDamageble
{
    public enum WaspState
    {
        Sleep,
        Awake,
    }

    [SerializeField] AudioClip awake;
    [SerializeField] AudioClip roar;
    [SerializeField] CameraShakeData roarShake;
    WaspEffects waspEffects;
    AudioSource audioSource;
    WaspState currentState = WaspState.Sleep;
    Player player;

    public WaspState CurrentState => currentState;
    void Awake()
    {
        waspEffects = GetComponent<WaspEffects>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void AwakeBoss()
    {
        print("ola");
        StartCoroutine(Awakening());
    }

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
        audioSource.PlayOneShot(roar);
        roarShake.timeToShake = roar.length;
        CameraShaker.Instance.Shake(roarShake);

        currentState = WaspState.Awake;
    }

    public void SetConfiguration(EnemyConfiguration configuration) { }

    public bool ShouldDeflect { get; } = false;
    public void TakeHit(float amount, Vector3 @from, float force) { }

    public void TakeDamage(float amount) { }

    public void Reset() { }
}
