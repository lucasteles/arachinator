using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Ui.HealthPoints;
using UnityEngine;

public class Player : MonoBehaviour, IDamageble
{
    [SerializeField]float speed = 5;
    [SerializeField]PlayerHealthPointsUi ui;

    [SerializeField]float damageTime = .5f;
    [SerializeField]float damageFlashTime = 1f;
    [SerializeField]Material damageMaterial;
    [SerializeField]Material dissolveMaterial;

    [SerializeField]AudioClip hit;
    [SerializeField]AudioClip dieSound;

    Movement movement;
    Rigidbody rb;
    Life life;
    Renderer[] renderers;
    Dictionary<Renderer, Material> originalMaterials;
    Dictionary<Renderer, Material> damageMaterialsCache;
    Dictionary<Renderer, Material> dissolveMaterialsCache;
    AudioSource audioSource;

    Coroutine currentDamageCoroutine = null;
    static readonly int FresnelLevel = Shader.PropertyToID("_FresnelLevel");
    static readonly int DissolveLevel = Shader.PropertyToID("_Dissolve");

    public bool IsInvincible { get; private set; }

    void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();
        life = GetComponent<Life>();
        audioSource = GetComponent<AudioSource>();
        renderers = GetComponentsInChildren<Renderer>().ToArray();
        life.onDeath += OnDeath;
    }
    void OnDestroy() => life.onDeath -= OnDeath;

    void OnDeath() => StartCoroutine(DieAnimation());

    void Start()
    {
        ui.SetMaxHealth(life.MaxLife);
        damageMaterialsCache = InstantiateMaterials(damageMaterial);
        dissolveMaterialsCache = InstantiateMaterials(dissolveMaterial);

        originalMaterials =
            renderers.Select(r => (r, r.sharedMaterial))
                .ToDictionary(x => x.r, x => x.sharedMaterial);
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

    Dictionary<Renderer, Material> InstantiateMaterials(Material material)
    {
        var cache = new Dictionary<Renderer, Material>();
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var color = renderer.sharedMaterial.color;
            var tex = renderer.sharedMaterial.mainTexture;
            var newMaterial = new Material(material);
            cache.Add(renderer, newMaterial);
            renderer.sharedMaterial = newMaterial;
            renderer.sharedMaterial.color = color;
            renderer.sharedMaterial.mainTexture = tex;
        }
        return cache;
    }

    void RestoreMaterials()
    {
        for (var i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterial = originalMaterials[renderers[i]];
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
        yield return DissolveEffect(dissolveStep);

        rb.MovePosition(Vector3.zero);
        transform.rotation = originalRotation;
        yield return RestoreEffect(dissolveStep);

        gun.enabled = webPistol.enabled = true;
        rb.constraints = constraints;
        life.Reset();
        ui.SetMaxHealth(life.MaxLife);
        DisableInvicible();
        movement.Unlock(lockKey);
    }

    public void SetMaterialFromCache(Dictionary<Renderer, Material> cache)
    {
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var newMaterial = cache[renderer];
            renderer.sharedMaterial = newMaterial;
        }
    }

    public void UseFresnelShader() => SetMaterialFromCache(damageMaterialsCache);
    public void UseDissolveShader() => SetMaterialFromCache(dissolveMaterialsCache);
    public void SetDissolveOnAllMaterials(float dissolve)
    {
        foreach (var mat in dissolveMaterialsCache.Values)
            mat.SetFloat(DissolveLevel, dissolve);
    }
    public void SetFresnelOnAllMaterials(float dissolve)
    {
        foreach (var mat in damageMaterialsCache.Values)
            mat.SetFloat(FresnelLevel, dissolve);
    }

    public IEnumerator DissolveEffect(float step)
    {
        UseDissolveShader();
        for (var i = 0f; i <= 1; i+=step)
        {
            SetDissolveOnAllMaterials(i);
            yield return null;
        }
    }

    public IEnumerator RestoreEffect(float step)
    {
        for (var i = 1f; i >= 0; i-=step)
        {
            SetDissolveOnAllMaterials(i);
            yield return null;
        }

        RestoreMaterials();
    }

    IEnumerator DamageFlash()
    {
        const float initialFresnel = 2.5f;
        const float lastFresnel = 0.5f;
        var time = 0f;
        UseFresnelShader();
        SetFresnelOnAllMaterials(1f);

        while (time <= damageFlashTime)
        {
            var tstep = Mathf.InverseLerp(0f, damageFlashTime, time);
            var level = Mathf.Lerp(initialFresnel, lastFresnel, tstep);
            SetFresnelOnAllMaterials(level);

            time += Time.deltaTime;
            yield return null;
        }
        RestoreMaterials();
        currentDamageCoroutine = null;
    }


}
