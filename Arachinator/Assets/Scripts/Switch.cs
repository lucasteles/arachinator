using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Switch : MonoBehaviour
{
    [SerializeField] AudioClip pushSound;
    [SerializeField] GameObject door;
    Material currentMaterial;
    bool canBeUsed;
    bool pushed;
    void Awake()
    {
        var renderer = GetComponent<Renderer>();
        currentMaterial = new Material(renderer.sharedMaterial);
        renderer.sharedMaterial = currentMaterial;
    }
    
    public void Enable() => canBeUsed = true;

    public void Disable() => canBeUsed = false;

    void Update ()
    {
        if (ActivateButtom.Instance.Pressed)
            PushSwitch();
    }
    
    public void PushSwitch()
    {
        if (pushed) return;
        if (!canBeUsed) return;
        
        pushed = true;
        transform.parent.GetComponentInChildren<TriggerEvent>().Disable();
        transform.parent.GetComponentInChildren<SwitchText>().gameObject.SetActive(false);
        door.GetComponent<Door>().OpenDoor();
        currentMaterial.SetFloat("_FresnelLevel", 0);
        currentMaterial.SetFloat("_Auto", 0);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(pushSound);
        if (TryGetComponent<ShinyMaterial>(out var s)) s.Shiny();

        if (Environment.IsMobile)
            ActivateButtom.Instance.Hide();
    }
}

#if UNITY_EDITOR
 [CustomEditor(typeof(Switch))]
 class SwitchEditor : Editor
 {
     public override void OnInspectorGUI() {
         DrawDefaultInspector();
         if(GUILayout.Button("Push Switch"))
             ((MonoBehaviour)target).GetComponent<Switch>().PushSwitch();

     }
 }
#endif
