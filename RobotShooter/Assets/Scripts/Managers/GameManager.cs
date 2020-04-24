using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [HideInInspector] public UIController uiController;
    [HideInInspector] public PlayerController player;
    //[HideInInspector] public CheckpointController checkpointController;
    [HideInInspector] public AudioManager audioManager;
    [HideInInspector] public bool isGameRunning = false;


    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
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
        isGameRunning = true;
    }

    public void AddController(AController c)
    {
        if (c is PlayerController)
            player = (PlayerController)c;
        else if (c is AudioManager)
            audioManager = (AudioManager)c;
        //else if (c is UIController)
            //uiController = (UIController)c;
        //else if (c is CheckpointController)
            //checkpointController = (CheckpointController)c;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameRunning) Invoke("StartGame", .1f);     
    }    

    public void ChangeScene(string scene)
    {        
        SceneManager.LoadScene(scene);
        if (Time.timeScale == 0) Time.timeScale = 1;
        //isGameRunning = false;
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
}
