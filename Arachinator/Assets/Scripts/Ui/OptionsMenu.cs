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

    void SetVolume(AudioMixer mixer, float volume, bool sfx)
    {
        mixer.SetFloat("MasterVolume", volume);
        if (sfx) gun.StartShoot();
    }

    float GetVolume(AudioMixer mixer)
    {
        mixer.GetFloat("MasterVolume", out var volume);
        return volume;
    }
    private void Awake()
    {
        gun = FindObjectOfType<Gun>();
        gun.canShoot = false;
    }

    void Start()
    {
        qualitySlider.value = QualitySettings.GetQualityLevel();
        musicSlider.value = GetVolume(musicMixer);
        sfxSlider.value = GetVolume(sfxMixer);

        musicSlider.onValueChanged.AddListener(volume => SetVolume(musicMixer, volume, false));
        sfxSlider.onValueChanged.AddListener(volume => SetVolume(sfxMixer, volume, true));
        qualitySlider.onValueChanged.AddListener(value => QualitySettings.SetQualityLevel((int)value));
    }
}