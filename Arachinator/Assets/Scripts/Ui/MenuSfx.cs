using UnityEngine;

public class MenuSfx : MonoBehaviour
{
    [SerializeField] AudioClip hover;
    [SerializeField] AudioClip select;

    public void Hover()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(hover);
    }
    public void Select()
    {
        CameraAudioSource.Instance.AudioSource.PlayOneShot(hover);
    }
}
