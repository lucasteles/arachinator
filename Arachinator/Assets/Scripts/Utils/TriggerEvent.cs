using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]UnityEvent triggerEvent;
    [SerializeField]bool selfDestruct = true;
    void OnTriggerEnter(Collider other)
    {
        triggerEvent?.Invoke();
        if (selfDestruct) Destroy(gameObject);
    }
}
