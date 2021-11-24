using Assets.Scripts.Cameras.Effects;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] AudioClip openSound;
    [SerializeField] CameraShakeData cameraShakeData;
    public void OpenDoor()
    {
        GetComponent<Animator>().SetBool("Open", true);
        CameraAudioSource.Instance.AudioSource.PlayOneShot(openSound);
        CameraShaker.Instance.Shake(cameraShakeData);
    }
}