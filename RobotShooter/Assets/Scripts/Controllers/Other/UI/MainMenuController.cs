using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject howToPlayMenu;
    public GameObject scoreboard;
    public Image fadePanel;

    bool onCinematic;

    private void Start()
    {
        onCinematic = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (onCinematic)
            {
                StartCoroutine(StartGame());
            }
            else
            {
                mainMenu.SetActive(true);
                optionsMenu.SetActive(false);
                howToPlayMenu.SetActive(false);
                scoreboard.SetActive(false);
            }
        }    
    }

    public void PlayCameraAnimation()
    {
        mainMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Camera.main.GetComponent<Animation>().Play("CameraAnimation");
        onCinematic = true;
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(9);
        StartCoroutine(FadeIn());
        yield return new WaitForSeconds(2);
        GameManager.instance.ChangeScene("LoadingScene");
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
}
