using System.Linq;
using TMPro;
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
    [SerializeField] TMP_Dropdown resDropdown;
    Gun gun;

    Resolution[] resolutions;

    void SetVolume(AudioMixer mixer, float volume, bool sfx) => 
        mixer.SetFloat("MasterVolume", volume);

    float GetVolume(AudioMixer mixer)
    {
        mixer.GetFloat("MasterVolume", out var volume);
        return volume;
    }
    
    public void StartShoot()
    {
        gun.StartShoot();
    }
    
    public void StopShoot()
    {
        gun.StopShot();
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

        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        var currentResolutionIndex =
            resolutions.ToList()
                .FindIndex(x => x.height == Screen.currentResolution.height
                                             && x.width == Screen.currentResolution.width);
        var options = resolutions.Select(x => $"{x.width}x{x.height}").ToList();
        resDropdown.AddOptions(options);
        resDropdown.value = currentResolutionIndex;
        resDropdown.RefreshShownValue();
        resDropdown.onValueChanged.AddListener(SetResolution);
    }

    void SetResolution(int i)
    {
        var res = resolutions[i];
        print($"Setting resolution to {res.width}x{res.height}");
        Screen.SetResolution(res.width,res.height, Screen.fullScreen);
    }

}