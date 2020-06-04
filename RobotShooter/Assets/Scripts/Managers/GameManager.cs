using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [HideInInspector] public UIController uiController;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public ShopController shopController;
    [HideInInspector] public RoundController roundController;
    [HideInInspector] public ObjectPoolerManager objectPoolerManager;
    //[HideInInspector] public CheckpointController checkpointController;
    [HideInInspector] public AudioManager audioManager;
    List<GameObject> activeDefenses = new List<GameObject>();

    private bool isGameRunning;

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }        

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
        //Invoke("StartGame", .1f);
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Level Loaded");
        //Debug.Log(scene.name);
        //Debug.Log(mode);    
        isGameRunning = false;
    }

    void Start()
    {
        
    }

    // Start is called before the first frame update
    private void StartGame()
    {
        if (player != null) player.StartGame();
        if (uiController != null && player != null) uiController.StartGame();
        if (audioManager != null) audioManager.StartGame();
        if (shopController != null) shopController.StartGame();
        if (roundController != null) roundController.StartGame();
        if (objectPoolerManager != null) objectPoolerManager.StartGame();
    }

    public void AddController(AController c)
    {

        if (c is PlayerController)
            player = (PlayerController)c;
        else if (c is AudioManager)
            audioManager = (AudioManager)c;
        else if (c is ShopController)
            shopController = (ShopController)c;
        else if (c is UIController)
            uiController = (UIController)c;
        else if (c is RoundController)
            roundController = (RoundController)c;
        else if (c is ObjectPoolerManager)
            objectPoolerManager = (ObjectPoolerManager)c;

        Invoke("StartGame", .1f);

        //else if (c is CheckpointController)
        //checkpointController = (CheckpointController)c;
    }

    // Update is called once per frame
    void Update()
    {
        Debuging();
    }    

    public void ChangeScene(string scene)
    {        
        SceneManager.LoadScene(scene);
        if (Time.timeScale == 0) Time.timeScale = 1;
        //isGameRunning = false;
    }

    public void DestroyDefenses()
    {
        foreach (GameObject defense in activeDefenses)
        {
            Destroy(defense);
        }
        activeDefenses.Clear();
    }

    public void AddActiveDefense(GameObject RestartGameElement)
    {
        activeDefenses.Add(RestartGameElement);
    }

    public void Restart()
    {
        //Aquí Pol inicies el restart
        //uiController.GameOver(false);
        if (Time.timeScale == 0) Time.timeScale = 1;
        StartGame();
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    void Debuging()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (GroundEnemy enemy in GameObject.FindObjectsOfType<GroundEnemy>())
            {
                //Destroy(enemy.gameObject);
                enemy.gameObject.SetActive(false);
                roundController.DecreaseEnemyCount();
            }
            foreach (FlyingEnemy enemy in GameObject.FindObjectsOfType<FlyingEnemy>())
            {
                //Destroy(enemy.gameObject);
                enemy.gameObject.SetActive(false);
                roundController.DecreaseEnemyCount();
            }
            foreach (TankEnemy enemy in GameObject.FindObjectsOfType<TankEnemy>())
            {
                //Destroy(enemy.gameObject);
                enemy.gameObject.SetActive(false);
                roundController.DecreaseEnemyCount();
            }
        }
#endif
    }

}
