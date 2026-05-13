using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.UI;

public class VolSliders : MonoBehaviour
{

    FMOD.Studio.Bus Music;
    FMOD.Studio.Bus SFX;
    FMOD.Studio.Bus Voice;
    FMOD.Studio.Bus UI;
    FMOD.Studio.Bus Ambience;
    FMOD.Studio.Bus Master;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start() {
        Music = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        Voice = FMODUnity.RuntimeManager.GetBus("bus:/Master/Voice");
        UI = FMODUnity.RuntimeManager.GetBus("bus:/Master/UI");
        Ambience = FMODUnity.RuntimeManager.GetBus("bus:/Master/Amb");
        Master = FMODUnity.RuntimeManager.GetBus("Bus:/Master");

        UpdateMusicSlider(PlayerPrefs.GetFloat("MusicVolume", 0.5f));
        UpdateSFXSlider(PlayerPrefs.GetFloat("SFXVolume", 0.5f));
        UpdateMasterSlider(PlayerPrefs.GetFloat("MasterVolume", 1.0f));

        // Update the slider
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }

    public void UpdateMusicSlider(float volume)
    {
        Music.setVolume(volume);
        Ambience.setVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void UpdateSFXSlider(float volume)
    {
        SFX.setVolume(volume);
        Voice.setVolume(volume);
        UI.setVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void UpdateMasterSlider(float volume)
    {
        Master.setVolume(volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
}