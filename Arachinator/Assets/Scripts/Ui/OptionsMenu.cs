using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] AudioMixer musicMixer;
    [SerializeField] AudioMixer sfxMixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider qualitySlider;
    Gun gun;

    private void SetVolume(AudioMixer mixer, float volume, bool sfx)
    {
        mixer.SetFloat("MasterVolume", volume);
        if (sfx) gun.StartShoot();
    }

    private void Awake()
    {
        gun = FindObjectOfType<Gun>();
        gun.canShoot = false;
        musicSlider.onValueChanged.AddListener(volume => SetVolume(musicMixer, volume, false));
        sfxSlider.onValueChanged.AddListener(volume => SetVolume(sfxMixer, volume, true));
        qualitySlider.onValueChanged.AddListener(value => QualitySettings.SetQualityLevel((int)value));
    }

    private void Start()
    {
        SetVolume(musicMixer, musicSlider.value, false);
        SetVolume(sfxMixer, sfxSlider.value, false);
    }
}