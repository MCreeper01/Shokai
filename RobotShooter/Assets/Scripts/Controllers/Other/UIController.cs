using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : AController
{

    [Header("Canvas")]
    public Image fadePanel;
    public GameObject interact;
    public GameObject HUD;
    public GameObject pause;
    public GameObject gameOver;

    [Header("HUD")]
    public Image healthBar;
    public Image energyBar;
    //public GameObject interactiveText;

    [Header("Pause")]
    public bool paused;

    void Awake()
    {
        //gameOver.SetActive(false);

        //interactiveText.SetActive(false);
    }

    // Use this for initialization
    public void StartGame(bool restart)
    {
        interact.SetActive(false);
        paused = false;
        pause.SetActive(false); //No funciona al start game, però si a l'awake
        fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 0);

        ChangeLife(100);
        ChangeEnergy(100);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) Pause();
    }

    public void Pause()
    {
        gc.audioManager.Stop("Footsteps");
        paused = !paused;
        pause.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
    }

    public void ChangeLife(float value)
    {
        healthBar.fillAmount = value / 100;
    }

    public void ChangeEnergy(float value)
    {
        energyBar.fillAmount = value / 100;
    }

    public void EnableDoorText(bool haveKey)
    {
        interact.SetActive(true);
        Text text = interact.GetComponentInChildren<Text>();
        text.text = haveKey ? "Press [" + gc.player.playerModel.interactKey + "] to open door." : "You need a key to open this door.";
    }

    public void DisableDoorText()
    {
        interact.SetActive(false);
    }

    /*public void ShowInteractiveText(string text)
    {
        interactiveText.GetComponentInChildren<Text>().text = text;
        interactiveText.SetActive(true);
    }

    public void HideInteractiveText()
    {
        interactiveText.SetActive(false);
    }*/


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
