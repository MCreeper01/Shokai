using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OptionsMenuController : MonoBehaviour
{
    const float SENSITIVITY_MIDDLE_VALUE = 90;

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
    public Image canvasPanel;

    void Start()
    {
        gameObject.SetActive(false);
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
    }

    public void ChangeBrightness(float value)
    {
        PlayerPrefs.SetFloat("brightness", value);
        canvasPanel.color = new Color(0, 0, 0, (1 - value) / 255 * 100); //Opacity value between 0-1
    }

    public void ChangeSensitivity(float value)
    {
        PlayerPrefs.SetFloat("sensitivity", value);
        if (GameManager.instance.player != null)
        {
            GameManager.instance.player.playerModel.yawRotationalSpeed = value * SENSITIVITY_MIDDLE_VALUE;
            GameManager.instance.player.playerModel.pitchRotationalSpeed = value * (SENSITIVITY_MIDDLE_VALUE / 2);
        }
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

        InitializePlayerPrefs();
    }

}
