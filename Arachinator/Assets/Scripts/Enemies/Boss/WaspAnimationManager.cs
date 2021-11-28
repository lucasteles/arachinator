using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspAnimationManager : MonoBehaviour
{
    Animator animator;

    bool startTaunt;
    bool inFly;
    bool takeOff;
    bool iddleEnded;

    static readonly int TauntTrigger = Animator.StringToHash("Taunt");
    static readonly int CloseWingsBool = Animator.StringToHash("CloseWings");
    static readonly int TakeOffTrigger = Animator.StringToHash("TakeOff");
    static readonly int LandTrigger = Animator.StringToHash("Land");
    static readonly int IddleTrigger = Animator.StringToHash("Iddle");

    void Awake()
    {
        animator = GetComponent<Animator>();
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

    public IEnumerator InGround()
    {
        yield return new WaitUntil(() => !inFly);
    }
    public void StartTauntEvent() => startTaunt = true;
    public void InFlyEvent() => inFly = true;
    public void IddleEndEvent() => iddleEnded = true;
    public void TakeOffEvent() => takeOff = true;

    public IEnumerator Iddle()
    {
        iddleEnded = false;
        animator.SetTrigger(IddleTrigger);
        return new WaitUntil(() => iddleEnded);

    }
}
