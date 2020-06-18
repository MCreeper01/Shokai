using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Shokai.Items;
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
    public InventoryController inventory;

    [Header("Shop")]
    public List<GameObject> itemsShop;
    public Text cashShopText;
    //public Text jetpackCost;
    //public Text grenadeCost;
    //public Text laserCost;
    //public Text healthCost;
    //public Text stickyGrenadeCost;
    //public Text empCost;
    //public Text mineCost;
    //public Text terrainTurretCost;
    //public Text airTurretCost;
    public GameObject jetpackShop;
    public GameObject grenadeShop;
    public GameObject stickyGrenadeShop;
    public GameObject laserShop;
    public GameObject healthShop;
    public GameObject empShop;
    public GameObject tTurretShop;
    public GameObject aTurretShop;
    public GameObject mineShop;

    public float alphaPurchaseLocked;
    public float alphaPurchaseUnlocked;

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

        /*jetpackCost.text = gc.shopController.ReturnCost("Jetpack").ToString();
        grenadeCost.text = gc.shopController.ReturnCost("Grenade").ToString();
        laserCost.text = gc.shopController.ReturnCost("Laser").ToString();
        healthCost.text = gc.shopController.ReturnCost("Health").ToString();
        stickyGrenadeCost.text = gc.shopController.ReturnCost("StickyGrenade").ToString();
        empCost.text = gc.shopController.ReturnCost("EMP").ToString();
        mineCost.text = gc.shopController.ReturnCost("Mine").ToString();
        terrainTurretCost.text = gc.shopController.ReturnCost("TerrainTurret").ToString();
        airTurretCost.text = gc.shopController.ReturnCost("AirTurret").ToString();*/

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

        if (gc != null && gc.player != null && gc.player.atShop)
        {
            if (gc.player.cash < inventory.jetpack.item.Cost)
            {
                GameObject shopIcon = jetpackShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = jetpackShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.grenade.item.Cost)
            {
                GameObject shopIcon = grenadeShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = grenadeShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.stickyGrenade.item.Cost)
            {
                GameObject shopIcon = stickyGrenadeShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = stickyGrenadeShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.emp.item.Cost)
            {
                GameObject shopIcon = empShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = empShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.laser.item.Cost)
            {
                GameObject shopIcon = laserShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = laserShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.health.item.Cost)
            {
                GameObject shopIcon = healthShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = healthShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.tTurret.item.Cost)
            {
                GameObject shopIcon = tTurretShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = tTurretShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.aTurret.item.Cost)
            {
                GameObject shopIcon = aTurretShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = aTurretShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }
            if (gc.player.cash < inventory.mine.item.Cost)
            {
                GameObject shopIcon = mineShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseLocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseLocked);
            }
            else
            {
                GameObject shopIcon = mineShop;
                shopIcon.GetComponent<Image>().color = new Color(shopIcon.GetComponent<Image>().color.r, shopIcon.GetComponent<Image>().color.g,
                    shopIcon.GetComponent<Image>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(0).GetComponent<Text>().color = new Color(shopIcon.transform.GetChild(0).GetComponent<Text>().color.r,
                    shopIcon.transform.GetChild(0).GetComponent<Text>().color.g, shopIcon.transform.GetChild(0).GetComponent<Text>().color.b, alphaPurchaseUnlocked);
                shopIcon.transform.GetChild(1).GetComponent<Image>().color = new Color(shopIcon.transform.GetChild(1).GetComponent<Image>().color.r,
                    shopIcon.transform.GetChild(1).GetComponent<Image>().color.g, shopIcon.transform.GetChild(1).GetComponent<Image>().color.b, alphaPurchaseUnlocked);
            }

        }
    }

    public void Pause()
    {
        paused = !paused;        
        pauseMenu.SetActive(paused);
        HUD.SetActive(!paused);
        Cursor.visible = paused;
        if (gc.player.shoping)
        {
            shopInterface.SetActive(!paused);
            if (!paused) Cursor.visible = true;
        }         
        optionsMenu.SetActive(false);
        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            AudioManager.instance.PauseAll();
        } 
        else
        {
            if (gc.player.shoping) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;
            AudioManager.instance.ResumeAll();
        }
        
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
            CalculateScore(gc.player.score);
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
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, fadePanel.color.a + 0.04f);
            StartCoroutine(FadeIn());
        }
    }

    void CalculateScore(int score)
    {
        if (score > PlayerPrefs.GetInt("BestScore"))
        {
            newRecordText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("BestScore", score);
        }
        else newRecordText.gameObject.SetActive(false);

        for (int i = 10; i >= 2; i--)
        {
            PlayerPrefs.SetInt("Score" + i, PlayerPrefs.GetInt("Score" + (i-1)));
        }
        PlayerPrefs.SetInt("Score1", score);
    }
}
