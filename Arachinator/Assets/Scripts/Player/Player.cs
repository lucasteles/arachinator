using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;

public class Player : MonoBehaviour, IDamageble
{
    [Header("Params")]
    [SerializeField]float speed = 5;
    [SerializeField]float damageTime = .5f;
    [SerializeField]float damageFlashTime = 1f;
    [SerializeField]bool invincible;

    [Header("Audio")]
    [SerializeField]AudioClip hit;
    [SerializeField]AudioClip dieSound;

    [Header("Refs")]
    [SerializeField]PlayerHealthPointsUi ui;

    Movement movement;
    Rigidbody rb;
    Life life;
    AudioSource audioSource;
    PlayerEffects playerEffects;
    Coroutine currentDamageCoroutine;

    public bool IsInvincible {
        get => invincible;
        private set => invincible = value; }

    void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();
        life = GetComponent<Life>();
        playerEffects = GetComponent<PlayerEffects>();
        audioSource = GetComponent<AudioSource>();
        life.onDeath += OnDeath;
    }
    void OnDestroy() => life.onDeath -= OnDeath;

    void OnDeath() => StartCoroutine(DieAnimation());

    void Start()
    {
        ui.SetMaxHealth(life.MaxLife);
    }

    void Update ()
    {
        if (life.IsDead) return;
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Move(input.normalized * speed);
    }

    public void DisableInvicible() => IsInvincible = false;
    public void EnableInvicible() => IsInvincible = true;

    public void TakeDamage(float amount)
    {
        ui.RemoveHealth(amount);
        life.Subtract(amount);
    }
    public void TakeHit(float amount, Vector3 from, float force)
    {
        if (IsInvincible || life.IsDead) return;

        EnableInvicible();
        rb.velocity = Vector3.zero;
        movement.Lock(damageTime);
        Invoke(nameof(DisableInvicible), damageTime);
        var direction = (transform.position - from).normalized;
        rb.AddForce(new Vector3(direction.x,0,direction.z) * force, ForceMode.VelocityChange);

        if (currentDamageCoroutine is {}) StopCoroutine(currentDamageCoroutine);
        currentDamageCoroutine = StartCoroutine(DamageFlash());

        audioSource.PlayOneShot(hit);
        TakeDamage(amount);
    }

    IEnumerator DieAnimation()
    {
        EnableInvicible();
        rb.velocity = rb.angularVelocity = Vector3.zero;
        var constraints = rb.constraints;
        var originalRotation = transform.rotation;

        var gun = GetComponentInChildren<Gun>();
        var webPistol = GetComponentInChildren<WebPistol>();

        gun.enabled = webPistol.enabled = false;

        rb.constraints = RigidbodyConstraints.FreezePositionX
                         | RigidbodyConstraints.FreezePositionZ;
        var lockKey = movement.Lock();
        rb.AddForce(Vector3.up * 12,ForceMode.VelocityChange);

        audioSource.PlayOneShot(dieSound);
        var rotationTarget = transform.rotation * Quaternion.Euler(0,40, 180);
        for (var i = 0f; i <= 1; i+=0.02f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation,  rotationTarget,i);
            yield return null;
        }
        yield return new WaitForSeconds(.3f);

        var dissolveStep = 0.0065f;
        yield return playerEffects.DissolveEffect(dissolveStep);

        rb.MovePosition(Vector3.zero);
        transform.rotation = originalRotation;
        yield return playerEffects.DissolveRestoreEffect(dissolveStep);

        gun.enabled = webPistol.enabled = true;
        rb.constraints = constraints;
        life.Reset();
        ui.SetMaxHealth(life.MaxLife);
        DisableInvicible();
        movement.Unlock(lockKey);
    }

    IEnumerator DamageFlash()
    {
        const float initialFresnel = 2.5f;
        const float lastFresnel = 0.5f;
        var time = 0f;
        playerEffects.UseFresnelShader();
        playerEffects.SetFresnelOnAllMaterials(1f);

        while (time <= damageFlashTime)
        {
            var tstep = Mathf.InverseLerp(0f, damageFlashTime, time);
            var level = Mathf.Lerp(initialFresnel, lastFresnel, tstep);
            playerEffects.SetFresnelOnAllMaterials(level);

            time += Time.deltaTime;
            yield return null;
        }
        playerEffects.RestoreMaterials();
        currentDamageCoroutine = null;
    }


}
