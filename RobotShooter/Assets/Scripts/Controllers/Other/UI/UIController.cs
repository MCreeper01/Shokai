using System;
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
    public GameObject optionsMenu;
    public GameObject gameOver;

    [Header("HUD")]
    public Image healthbar;
    public Image shieldbar;
    public List<Image> overheatAR;
    public Text interactiveText;
    public Text cashText;
    public Text roundCounter;
    public Text preparationTimeText;
    public Text fpsText;
    public KeyCode skipPreparationKey = KeyCode.O;
    public List<GameObject> pointers;

    [Header("Shop")]
    public Text cashShopText;
    public Text jetpackCost;
    public Text grenadeCost;
    public Text laserCost;
    public Text healthCost;
    public Text stickyGrenadeCost;
    public Text empCost;
    public Text mineCost;
    public Text terrainTurretCost;
    public Text airTurretCost;

    [Header("Pause")]
    public bool paused;

    [Header("Game Over")]
    public Text scoreText;
    public Text newRecordText;

    private float shopTimer;
    private float deltaTime;

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

        fpsText.gameObject.SetActive(Convert.ToBoolean(PlayerPrefs.GetInt("showFPS")));

        shopTimer = gc.roundController.preparationTime;
        GameEvents.instance.onRoundStart += OnRoundStart;
        GameEvents.instance.onPreparationFinish += OnPreparationFinish;
    }

    // Update is called once per frame
    void Update()
    {
        if (gc != null && gc.player != null && Input.GetKeyDown(gc.player.playerModel.pauseKey)) Pause();
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
        if (gc != null && preparationTimeText.gameObject.activeSelf)
        {
            preparationTimeText.text = "Preparation time remining: " + ((int)gc.roundController.preparationTime - (int)gc.roundController.elapsedTime).ToString() + "\nPress [" + skipPreparationKey + "] to skip";
            if (Input.GetKeyDown(skipPreparationKey)) gc.roundController.ChangeState(RoundController.State.SPAWN);
        } 
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
    }

    public void Pause()
    {
        paused = !paused;
        Cursor.visible = paused;
        pauseMenu.SetActive(paused);
        HUD.SetActive(!paused);
        optionsMenu.SetActive(false);
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
        shieldbar.transform.localPosition = new Vector3(shieldbar.transform.localPosition.x, healthbar.transform.localPosition.y + healthbar.rectTransform.rect.height * healthbar.transform.localScale.y
            * healthbar.fillAmount, shieldbar.transform.localPosition.z);
    }

    public void ChangePointer(int num)
    {
        foreach (GameObject pointer in pointers) pointer.SetActive(false);
        pointers[num - 1].SetActive(true);
    }

    public void IncreaseRound()
    {
        roundCounter.text = "Round " + gc.roundController.currentRound.ToString(); 
    }

    public void ChangeCash(int value)
    {
        cashText.text = "Cash: " + value;
        cashShopText.text = value.ToString();
    }

    public void Shop()
    {
        shopInterface.SetActive(!shopInterface.activeSelf);
    }

    public void ChangeAROverheat(float value)
    {
        foreach (Image overheat in overheatAR) overheat.fillAmount = value / 100;
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

    void OnRoundStart()
    {
        preparationTimeText.gameObject.SetActive(true);
    }

    void OnPreparationFinish()
    {
        preparationTimeText.gameObject.SetActive(false);
    }

    public void GameOver(bool show)
    {
        Cursor.visible = show;
        if (show && !gameOver.activeSelf)
        {
            gameOver.SetActive(true);
            //gc.audioManager.Play("GameOver");
            HUD.SetActive(false);
            scoreText.text = gc.player.score.ToString();           
            Cursor.lockState = CursorLockMode.None;
            if (gc.player.score > PlayerPrefs.GetInt("highScore"))
            {
                newRecordText.gameObject.SetActive(true);
                PlayerPrefs.SetInt("highScore", gc.player.score);
            } 
             else newRecordText.gameObject.SetActive(false);
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
