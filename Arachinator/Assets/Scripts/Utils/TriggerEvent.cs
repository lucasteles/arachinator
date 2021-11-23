using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]UnityEvent triggerEvent;
    [SerializeField]bool selfDestruct = true;
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;
        
        triggerEvent?.Invoke();
        if (selfDestruct) Destroy(gameObject);
    }
}
