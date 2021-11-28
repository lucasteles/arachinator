using System.Collections;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour, IDamageble
{
    [Header("Params")]
    [SerializeField]float speed = 5;
    [SerializeField]float damageTime = .5f;
    [SerializeField]float damageFlashTime = 1f;
    [SerializeField]bool invincible;

    [SerializeField]Transform startPosition;

    [Header("Audio")]
    [SerializeField]AudioClip hit;
    [SerializeField]AudioClip dieSound;

    [Header("Dash")]
    [SerializeField]float dashForce;
    [SerializeField]float dashDuration;
    [SerializeField]Cooldown dashCooldown;
    [SerializeField]AudioClip dashSound;
    [SerializeField]ParticleSystem dashParticle;

    public Vector3 RespawnPosition { get; set; }
    PlayerHealthPointsUi ui;

    Movement movement;
    Rigidbody rb;
    Life life;
    AudioSource audioSource;
    PlayerEffects playerEffects;
    Coroutine currentDamageCoroutine;
    bool inDash = false;
    bool hasWebPistol = false;

    public Movement Movement => movement;
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
        life.onLifeChange += OnLifeChange;
        ui = GameObject.Find("PlayerLifeSlide")?.GetComponent<PlayerHealthPointsUi>();
    }

    void OnLifeChange(float current, float max)
    {
        if (!ui) return;
        ui.SetMaxHealth(life.MaxLife);
        ui.SetHealth(life.CurrentLife);
    }

    void OnDestroy()
    {
        life.onDeath -= OnDeath;
    }

    void OnDeath(Life life) => StartCoroutine(DieAnimation());

    void Start()
    {
        RespawnPosition = startPosition.position;
    }

    void Update ()
    {
        if (life.IsDead || inDash) return;

        if (Input.GetKeyDown(KeyCode.Space) && dashCooldown && !movement.IsLocked())
        {
            StartCoroutine(Dash());
        }


        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Move(input.normalized * speed);
    }

    IEnumerator Dash()
    {
        inDash = true;
        dashCooldown.Reset();
        rb.velocity = Vector3.zero;
        var mlock = movement.Lock();
        var direction = movement.Direction == Vector3.zero ? transform.forward : movement.Direction;
        audioSource.PlayOneShot(dashSound);
        dashParticle.Play();
        rb.AddForce(direction * dashForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector3.zero;
        dashParticle.Stop();
        movement.Unlock(mlock);
        inDash = false;
    }

    public void DisableInvicible() => IsInvincible = false;
    public void EnableInvicible() => IsInvincible = true;

    public void TakeDamage(float amount)
    {
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


    public void PushBack(Vector3 from, float force)
    {
        rb.velocity = Vector3.zero;
        movement.Lock(damageTime);
        var direction = (transform.position - from).normalized;
        rb.AddForce(new Vector3(direction.x,0,direction.z) * force, ForceMode.Acceleration);
    }


    IEnumerator DieAnimation()
    {
        EnableInvicible();
        var aimLegCubes = GetComponentsInChildren<AimLegGrounder>();
        aimLegCubes.ToList().ForEach(x => x.EnableKinematic());
        rb.velocity = rb.angularVelocity = Vector3.zero;
        var constraints = rb.constraints;
        var originalRotation = transform.rotation;

        var gun = GetComponentInChildren<Gun>();
        var webPistol = GetComponentInChildren<WebPistol>();

        gun.StopShot();
        hasWebPistol = webPistol.enabled;
        gun.enabled = webPistol.enabled = false;

        rb.constraints = RigidbodyConstraints.FreezePositionX
                         | RigidbodyConstraints.FreezePositionZ;
        var lockKey = movement.Lock();
        rb.AddForce(Vector3.up * 12,ForceMode.VelocityChange);

        audioSource.PlayOneShot(dieSound);

        var dissolveStep = 0.01f;
        if (!IsInFall())
        {
            yield return DieJumpAnimation(aimLegCubes, dissolveStep);
            yield return playerEffects.DissolveEffect(dissolveStep);
        }
        else
            yield return playerEffects.DissolveEffect(1f);

        rb.velocity = Vector3.zero;
        rb.MovePosition(RespawnPosition);
        transform.rotation = originalRotation;
        aimLegCubes.ToList().ForEach(x => x.DisableKinematic());
        aimLegCubes.ToList().ForEach(x => x.RestorePosition());
        yield return playerEffects.DissolveRestoreEffect(dissolveStep);

        gun.enabled = true;
        webPistol.enabled = hasWebPistol;
        
        rb.constraints = constraints;
        life.Reset();
        DisableInvicible();
        movement.Unlock(lockKey);
    }

    bool IsInFall() => transform.position.y < -5;

    private IEnumerator DieJumpAnimation(AimLegGrounder[] aimLegCubes, float dissolveStep)
    {
        var rotationTarget = transform.rotation * Quaternion.Euler(0, 40, 180);
        for (var i = 0f; i <= 1; i += 0.02f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotationTarget, i);

            foreach (var aim in aimLegCubes)
                aim.transform.localPosition = Vector3.Lerp(aim.transform.localPosition, Vector3.down * 2f, i / 15);
            yield return null;
        }

        yield return new WaitForSeconds(.3f);

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

    public bool IsMaxHealth() => life.IsFull();
    public void AddLife(float amount) => life.Add(amount);
}
