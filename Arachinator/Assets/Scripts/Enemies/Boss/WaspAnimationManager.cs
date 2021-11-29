using System;
using System.Collections;
using UnityEngine;

public class WaspAnimationManager : MonoBehaviour
{
    Animator animator;

    bool startTaunt;
    bool inFly;
    bool takeOff;
    bool iddleEnded;
    bool deathEnded;
    bool endAttack;

    static readonly int TauntTrigger = Animator.StringToHash("Taunt");
    static readonly int CloseWingsBool = Animator.StringToHash("CloseWings");
    static readonly int TakeOffTrigger = Animator.StringToHash("TakeOff");
    static readonly int LandTrigger = Animator.StringToHash("Land");
    static readonly int IddleTrigger = Animator.StringToHash("Iddle");
    static readonly int ShootBool = Animator.StringToHash("Shoot");
    static readonly int DeathTrigger = Animator.StringToHash("Death");
    static readonly int ResetTrigger = Animator.StringToHash("Reset");
    static readonly int AttackTrigger = Animator.StringToHash("Attack");

    [Header("Death")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource zunidoAudioSource;

    [SerializeField] AudioClip deathScream;
    [SerializeField] AudioClip deathDrop;
    [SerializeField] AudioClip deathFly;
    [SerializeField] AudioClip fastAtack;
    [SerializeField] AudioClip deathFly2;
    [SerializeField] AudioClip deathLastBreath;
    [SerializeField] Collider attackFowardCollider;

    [SerializeField] TrailRenderer trailRenderer;
    public event Action onShoot;
    public event Action onAttack;

    void Awake()
    {
        animator = GetComponent<Animator>();
       attackFowardCollider.enabled = false;
    }

    public IEnumerator Taunt()
    {
        startTaunt = false;
        iddleEnded = true;
        animator.SetTrigger(TauntTrigger);
        yield return new WaitUntil(() => startTaunt);
    }

    public void CloseWings()
    {
        animator.SetBool(CloseWingsBool, true);
    }

    public void OpenWings()
    {
        animator.SetBool(CloseWingsBool, true);
    }

    public IEnumerator TakeOff()
    {
        takeOff = false;
        animator.SetTrigger(TakeOffTrigger);
        yield return new WaitUntil(() => takeOff);
    }
    public IEnumerator Land()
    {
        animator.SetTrigger(LandTrigger);
        yield return new WaitUntil(() => !inFly);
    }

    public IEnumerator InFly()
    {
        yield return new WaitUntil(() => inFly);
    }

    public void StartShooting()
    {
       animator.SetBool(ShootBool, true);
    }

    public IEnumerator BeginAttack()
    {
       endAttack = false;
       animator.SetTrigger(AttackTrigger);
       attackFowardCollider.enabled = true;
       yield return new WaitUntil(() => endAttack);
    }

    public void StopShooting()
    {
       animator.SetBool(ShootBool, false);
    }

    public IEnumerator InGround()
    {
        yield return new WaitUntil(() => !inFly);
    }
    public void StartTauntEvent() => startTaunt = true;
    public void InGroundEvent() => inFly = false;
    public void InFlyEvent() => inFly = true;
    public void AttackEnd()
    {
        print("Attack end");
        trailRenderer.emitting = false;
        attackFowardCollider.enabled = false;
        endAttack = true;
    }

    public void IddleEndEvent() => iddleEnded = true;
    public void TakeOffEvent() => takeOff = true;
    public void ShootEvent() => onShoot?.Invoke();

    public IEnumerator Idle()
    {
        iddleEnded = false;
        animator.SetTrigger(IddleTrigger);
        return new WaitUntil(() => iddleEnded);

    }
    public IEnumerator BeginDeath()
    {
        deathEnded = false;
        animator.SetTrigger(DeathTrigger);
        yield return new WaitUntil(() => deathEnded);
    }
    public void DeathScream() => audioSource.PlayOneShot(deathScream);
    public void DeathStartFlyScream()
    {
        zunidoAudioSource.Play();
        audioSource.PlayOneShot(deathFly);
        audioSource.PlayOneShot(deathFly2);
    }
    public void DeathDrop()
    {
        zunidoAudioSource.Stop();
        audioSource.PlayOneShot(deathDrop);
    }

    public void BeginGroundAttackEvent()
    {
        trailRenderer.emitting = true;
        audioSource.PlayOneShot(fastAtack);
        onAttack?.Invoke();
    }

    public void DeathLastBreath() => audioSource.PlayOneShot(deathLastBreath);

    public void DeathEnd() => deathEnded = true;

    public void Reset()
    {
        startTaunt = true;
        inFly = false;
        takeOff = true;
        iddleEnded = true;
        endAttack = true;
        animator.SetTrigger(ResetTrigger);
    }
}
