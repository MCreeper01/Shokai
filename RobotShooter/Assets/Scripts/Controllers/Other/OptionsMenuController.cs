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

    [Header("Game Objects")]
    public Toggle showFPS;
    public Slider brightness;
    public Slider sensitivity;
    public Slider generalVolume;
    public Slider fxVolume;
    public Slider musicVolume;

    void Start()
    {
        gameObject.SetActive(false);
        InitializeOptions();
    }

    void InitializeOptions()
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
    }

    public void ChangeBrightness(float value)
    {
        PlayerPrefs.SetFloat("brightness", value);
    }

    public void ChangeSensitivity(float value)
    {
        PlayerPrefs.SetFloat("sensitivity", value);
    }

    public void ChangeGeneralVolume(float value)
    {
        PlayerPrefs.SetFloat("generalVolume", value);
    }

    public void ChangeFXVolume(float value)
    {
        PlayerPrefs.SetFloat("fxVolume", value);
    }

    public void ChangeMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    public void RestoreDefaultSettings()
    {
        ShowFPS(defaultShowFPS);
        ChangeBrightness(defaultBrightness);
        ChangeSensitivity(defaultSensitivity);
        ChangeGeneralVolume(defaultGeneralVolume);
        ChangeFXVolume(defaultFXVolume);
        ChangeMusicVolume(defaultMusicVolume);

        InitializeOptions();
    }

}
