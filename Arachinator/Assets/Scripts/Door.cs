using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private AudioClip openSound;
    public void OpenDoor()
    {
        GetComponent<Animator>().SetBool("Open", true);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(openSound);
    }
}