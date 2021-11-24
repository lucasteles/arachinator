using System;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]UnityEvent triggerEvent;
    [SerializeField]UnityEvent triggerEventExit;
    [SerializeField]bool selfDestruct = true;
    [SerializeField]bool runOnce = true;
    bool done;
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") || (runOnce && done))
            return;
        
        triggerEvent?.Invoke();
        done = true;
        
        if (selfDestruct) Destroy(gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerEventExit?.Invoke();
    }
}