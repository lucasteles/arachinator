using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] private AudioClip pushSound;
    Material currentMaterial;
    void Awake()
    {
        var renderer = GetComponent<Renderer>();
        currentMaterial = new Material(renderer.sharedMaterial);
        renderer.sharedMaterial = currentMaterial;
    }
    
    public void PushSwitch()
    {
        currentMaterial.SetFloat("_FresnelLevel", 0);
        currentMaterial.SetFloat("_Auto", 0);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(pushSound);
    }
}