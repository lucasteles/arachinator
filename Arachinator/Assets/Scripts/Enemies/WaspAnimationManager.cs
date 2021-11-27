using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaspAnimationManager : MonoBehaviour
{
    Animator animator;

    bool startTaunt;
    static readonly int TauntTrigger = Animator.StringToHash("Taunt");
    static readonly int CloseWingsBool = Animator.StringToHash("CloseWings");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public IEnumerator Taunt()
    {
        startTaunt = false;
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

    public void StartTauntEvent() => startTaunt = true;
}
