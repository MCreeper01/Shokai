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
    public GameObject pauseMenu;
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
    public Text stickyGrenadeCost;
    public Text empCost;
    public Text mineCost;
    public Text terrainTurretCost;
    public Text airTurretCost;
    public Text timerText;

    [Header("Pause")]
    public bool paused;

    [Header("Game Over")]
    public Text scoreText;
    public Text newRecordText;

    private float shopTimer;

    void Awake()
    {
        //gameOver.SetActive(false);

        shopInterface.SetActive(false);
        interactiveText.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
        gameOver.SetActive(false);
    }

    // Use this for initialization
    public void StartGame()
    {
        ChangeAROverheat(gc.player.playerModel.MAX_CHARGER_AMMO_AR);

        jetpackCost.text = gc.shopController.ReturnCost("Jetpack").ToString();
        grenadeCost.text = gc.shopController.ReturnCost("Grenade").ToString();
        laserCost.text = gc.shopController.ReturnCost("Laser").ToString();
        healthCost.text = gc.shopController.ReturnCost("Health").ToString();
        stickyGrenadeCost.text = gc.shopController.ReturnCost("StickyGrenade").ToString();
        empCost.text = gc.shopController.ReturnCost("EMP").ToString();
        mineCost.text = gc.shopController.ReturnCost("Mine").ToString();
        terrainTurretCost.text = gc.shopController.ReturnCost("TerrainTurret").ToString();
        airTurretCost.text = gc.shopController.ReturnCost("AirTurret").ToString();

        shopTimer = gc.roundController.preparationTime;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) Pause();
        /*if (gc.roundController.currentState == RoundController.State.PREPARATION)
        {
            shopTimer -= Time.deltaTime;
            if (gc.player.atShop) timerText.text = shopTimer.ToString();
            if (shopTimer <= 0)
            {
                shopTimer = gc.roundController.preparationTime;
                gc.player.Shop(false);
            }                      
        }*/
    }

    public void Pause()
    {
        paused = !paused;
        Cursor.visible = paused;
        pauseMenu.SetActive(paused);
        if (paused) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = paused ? 0 : 1;
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
        Cursor.visible = show;
        if (show && !gameOver.activeSelf)
        {
            gameOver.SetActive(true);
            //gc.audioManager.Play("GameOver");
            HUD.SetActive(false);
            scoreText.text = gc.player.cash.ToString();           
            Cursor.lockState = CursorLockMode.None;
            // if cash > highscore  newRecordText.SetActive(true);
            // else newRecordText.SetActive(false);
        }
        else if (!show && gameOver.activeSelf)
        {
            gameOver.SetActive(false);
            HUD.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
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
