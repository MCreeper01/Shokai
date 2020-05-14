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
    public Image healthbar;
    public Image shieldbar;
    public Image overheatAR;
    public Text interactiveText;
    public Text cashText;
    public Text roundCounter;

    [Header("Shop")]
    public Text jetpackCost;
    public Text grenadeCost;
    public Text laserCost;
    public Text healthCost;
    public Text mineCost;
    public Text terrainTurretCost;

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
        ChangeAROverheat(gc.player.playerModel.MAX_CHARGER_AMMO_AR);

        jetpackCost.text = gc.slotsController.ReturnCost("Jetpack").ToString();
        grenadeCost.text = gc.slotsController.ReturnCost("Grenade").ToString();
        laserCost.text = gc.slotsController.ReturnCost("Laser").ToString();
        healthCost.text = gc.slotsController.ReturnCost("Health").ToString();
        mineCost.text = gc.slotsController.ReturnCost("Mine").ToString();
        terrainTurretCost.text = gc.slotsController.ReturnCost("TerrainTurret").ToString();
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

    public void ChangeHealth(float value)
    {
        healthbar.fillAmount = value / 100;
        ChangeShieldPos();
    }

    public void ChangeShield(float value)
    {
        shieldbar.fillAmount = value / 100;
    }

    public void ChangeShieldPos()
    {
        shieldbar.transform.localPosition = new Vector3((healthbar.transform.localPosition.x - healthbar.rectTransform.rect.width / 2) + 
            healthbar.rectTransform.rect.width * healthbar.fillAmount + shieldbar.rectTransform.rect.width / 2, 
            shieldbar.transform.localPosition.y, shieldbar.transform.localPosition.z);
    }

    public void IncreaseRound()
    {
        roundCounter.text = gc.roundController.currentRound.ToString();
    }

    public void ChangeCash(int value)
    {
        cashText.text = "Cash: " + value;
    }

    public void Shop()
    {
        shopInterface.SetActive(!shopInterface.activeSelf);
    }

    public void ChangeAROverheat(float value)
    {
        overheatAR.fillAmount = value / 100;
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
