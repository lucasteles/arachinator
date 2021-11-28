using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]UnityEvent triggerEvent;
    [SerializeField]UnityEvent triggerEventExit;
    [SerializeField]bool selfDestruct = true;
    [SerializeField]bool runOnce = true;
    bool done;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Trigger();
    }

    public void Trigger()
    {
        if (runOnce && done) return;
        triggerEvent?.Invoke();
        done = true;

        if (selfDestruct) Destroy(gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerEventExit?.Invoke();
    }

}
#if UNITY_EDITOR
 [CustomEditor(typeof(TriggerEvent))]
 class TriggerEventEditor : Editor
 {
     public override void OnInspectorGUI() {
         DrawDefaultInspector();
         if(GUILayout.Button("Trigger"))
             ((MonoBehaviour)target).GetComponent<TriggerEvent>().Trigger();

     }
 }
#endif
