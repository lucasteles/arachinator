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

    Movement movement;
    Rigidbody rb;
    Life life;
    SkinnedMeshRenderer[] renderer;
    Dictionary<SkinnedMeshRenderer, Material> originalMaterials;

    Coroutine currentDamageCoroutine = null;
    static readonly int FresnelLevel = Shader.PropertyToID("_FresnelLevel");

    public bool IsInvincible { get; private set; }

    void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();
        life = GetComponent<Life>();
        renderer = GetComponentsInChildren<SkinnedMeshRenderer>().ToArray();
    }

    void Start()
    {
        ui.SetMaxHealth(life.MaxLife);
        originalMaterials =
            renderer.Select(r => (r, r.sharedMaterial))
                .ToDictionary(x => x.r, x => x.sharedMaterial);
    }

    void Update ()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Move(input.normalized * speed);

        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(DamageFlash());

    }
    public void TakeDamage(float amount) => ui.RemoveHealth(amount);

    public void DisableInvicible() => IsInvincible = false;
    public void EnableInvicible() => IsInvincible = true;

    public void TakeHit(float amount, Vector3 from, float force)
    {
        if (IsInvincible) return;

        EnableInvicible();
        rb.velocity = Vector3.zero;
        movement.Lock(damageTime);
        Invoke(nameof(DisableInvicible), damageTime);
        var direction = (transform.position - from).normalized;
        rb.AddForce(new Vector3(direction.x,0,direction.z) * force, ForceMode.VelocityChange);

        if (currentDamageCoroutine is {}) StopCoroutine(currentDamageCoroutine);
        currentDamageCoroutine = StartCoroutine(DamageFlash());
        TakeDamage(amount);
    }

    void SetMaterials(Material material)
    {
        for (var i = 0; i < renderer.Length; i++)
        {
            var color = renderer[i].sharedMaterial.color;
            renderer[i].sharedMaterial = material;
            renderer[i].sharedMaterial.color = color;
        }
    }

    void RestoreMaterials()
    {
        for (var i = 0; i < renderer.Length; i++)
            renderer[i].sharedMaterial = originalMaterials[renderer[i]];
    }
    IEnumerator DamageFlash()
    {
        const float initialFresnel = 2.5f;
        const float lastFresnel = 0.5f;
        var time = 0f;
        SetMaterials(damageMaterial);
        damageMaterial.SetFloat(FresnelLevel, 1f);

        while (time <= damageFlashTime)
        {
            var tstep = Mathf.InverseLerp(0f, damageFlashTime, time);
            var level = Mathf.Lerp(initialFresnel, lastFresnel, tstep);
            damageMaterial.SetFloat(FresnelLevel, level);

            time += Time.deltaTime;
            yield return null;
        }
        RestoreMaterials();
        currentDamageCoroutine = null;
    }


}
