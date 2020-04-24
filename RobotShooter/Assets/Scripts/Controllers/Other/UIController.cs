using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : AController
{
    [Header("Canvas")]
    public Image fadePanel;    
    public GameObject HUD;
    public GameObject shopInterface;
    public GameObject pause;
    public GameObject gameOver;

    [Header("HUD")]
    public Text bulletAR;
    public Text interactiveText;

    [Header("SHOP")]
    public Text description;

    [Header("Pause")]
    public bool paused;

    void Awake()
    {
        //gameOver.SetActive(false);

        shopInterface.SetActive(false);
        interactiveText.gameObject.SetActive(false);
    }

    // Use this for initialization
    public void StartGame()
    {
        ChangeARAmmo(gc.player.playerModel.MAX_CHARGER_AMMO_AR);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) Pause();
    }

    public void Pause()
    {
        /*paused = !paused;
        pause.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;*/
    }

    public void ChangeLife(float value)
    {
        //healthBar.fillAmount = value / 100;
    }

    public void Shop()
    {
        shopInterface.SetActive(!shopInterface.activeSelf);
    }

    public void ChangeARAmmo(float value)
    {
        bulletAR.text = value.ToString();
    }

    public void ChangeEnergy(float value)
    {
        //energyBar.fillAmount = value / 100;
    }   

    public void ShowInteractiveText(string text)
    {
        interactiveText.text = text;
        interactiveText.gameObject.SetActive(true);
    }

    public void HideInteractiveText()
    {
        interactiveText.gameObject.SetActive(false);
    }


    public void GameOver(bool show)
    {
        if (show && !gameOver.activeSelf)
        {
            gameOver.SetActive(true);
            gc.audioManager.Play("GameOver");
            HUD.SetActive(false);
        }
        else if (!show && gameOver.activeSelf)
        {
            gameOver.SetActive(false);
            HUD.SetActive(true);
        }
    }

    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(0.01f);
        if (fadePanel.color.a < 1)
        {
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, fadePanel.color.a + 0.01f);
            StartCoroutine(FadeIn());
        }
        else gc.ChangeScene("MainMenu");
    }
}
