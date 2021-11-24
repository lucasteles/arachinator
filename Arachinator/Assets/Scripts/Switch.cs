using UnityEditor;
using UnityEngine;

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
        if (!canBeUsed) return;
        if (Input.GetKeyDown(KeyCode.E) && !pushed)
            PushSwitch();
    }
    
    public void PushSwitch()
    {
        pushed = true;
        door.GetComponent<Door>().OpenDoor();
        currentMaterial.SetFloat("_FresnelLevel", 0);
        currentMaterial.SetFloat("_Auto", 0);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(pushSound);
    }
}