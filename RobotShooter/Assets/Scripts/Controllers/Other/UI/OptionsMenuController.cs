using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OptionsMenuController : MonoBehaviour
{
    [Header("Default Settings")]
    public bool defaultShowFPS;
    [Range(0.0f, 1.0f)]
    public float defaultBrightness;
    [Range(0.0f, 1.0f)]
    public float defaultSensitivity;
    [Range(0.0f, 1.0f)]
    public float defaultGeneralVolume;
    [Range(0.0f, 1.0f)]
    public float defaultFXVolume;
    [Range(0.0f, 1.0f)]
    public float defaultMusicVolume;

    [Header("Values")]
    public float minSensitivity;
    public float maxSensitivity;

    [Header("Game Objects")]
    public Toggle showFPS;
    public Slider brightness;
    public Slider sensitivity;
    public Slider generalVolume;
    public Slider fxVolume;
    public Slider musicVolume;
    public Image brightnessPanel;

    void Start()
    {
        if (PlayerPrefs.GetInt("firstTime") != 1)
        {
            RestoreDefaultSettings();
            PlayerPrefs.SetInt("firstTime", 1);
        }
        gameObject.SetActive(false);
        maxSensitivity -= minSensitivity;
        InitializePlayerPrefs();

        ShowFPS(Convert.ToBoolean(PlayerPrefs.GetInt("showFPS")));
        ChangeBrightness(PlayerPrefs.GetFloat("brightness"));
        ChangeSensitivity(PlayerPrefs.GetFloat("sensitivity"));
        ChangeGeneralVolume(PlayerPrefs.GetFloat("generalVolume"));
        ChangeFXVolume(PlayerPrefs.GetFloat("fxVolume"));
        ChangeMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
    }

    void InitializePlayerPrefs()
    {
        showFPS.isOn = Convert.ToBoolean(PlayerPrefs.GetInt("showFPS"));
        brightness.value = PlayerPrefs.GetFloat("brightness");
        sensitivity.value = PlayerPrefs.GetFloat("sensitivity");
        generalVolume.value = PlayerPrefs.GetFloat("generalVolume");
        fxVolume.value = PlayerPrefs.GetFloat("fxVolume");
        musicVolume.value = PlayerPrefs.GetFloat("musicVolume");
    }

    public void ShowFPS(bool showFPS)
    {
        PlayerPrefs.SetInt("showFPS", Convert.ToInt32(showFPS));
        if (GameManager.instance.uiController != null) GameManager.instance.uiController.fpsText.gameObject.SetActive(showFPS);
    }

    public void ChangeBrightness(float value)
    {
        PlayerPrefs.SetFloat("brightness", value);
        brightnessPanel.color = new Color(0, 0, 0, (1 - value) / 255 * 100); //Opacity value between 0-1
    }

    public void ChangeSensitivity(float value)
    {
        PlayerPrefs.SetFloat("sensitivity", value);
        if (GameManager.instance.player != null)
        {
            GameManager.instance.player.playerModel.yawRotationalSpeed = minSensitivity + (value * maxSensitivity);
            GameManager.instance.player.playerModel.pitchRotationalSpeed = (minSensitivity + (value * maxSensitivity)) / 2;
        }
    }

    public void ChangeGeneralVolume(float value)
    {
        PlayerPrefs.SetFloat("generalVolume", value);
        AudioManager.instance.masterVolume = value;
        AudioManager.instance.musics[0].source.volume = value * AudioManager.instance.musicVolume;
        AudioManager.instance.crowdSource.volume = value * AudioManager.instance.fXVolume * AudioManager.instance.unitySounds[0].volume;
    }

    public void ChangeFXVolume(float value)
    {
        PlayerPrefs.SetFloat("fxVolume", value);
        AudioManager.instance.fXVolume = value;
        AudioManager.instance.crowdSource.volume = value * AudioManager.instance.masterVolume * AudioManager.instance.unitySounds[0].volume;
    }

    public void ChangeMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
        AudioManager.instance.musicVolume = value;
        AudioManager.instance.musics[0].source.volume = value * AudioManager.instance.masterVolume;
    }

    public void RestoreDefaultSettings()
    {
        ShowFPS(defaultShowFPS);
        ChangeBrightness(defaultBrightness);
        ChangeSensitivity(defaultSensitivity);
        ChangeGeneralVolume(defaultGeneralVolume);
        ChangeFXVolume(defaultFXVolume);
        ChangeMusicVolume(defaultMusicVolume);

        InitializePlayerPrefs();
    }
}
