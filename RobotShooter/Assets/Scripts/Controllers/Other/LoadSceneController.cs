using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneController : MonoBehaviour
{

    [SerializeField] private GameObject loadCamera;
    [SerializeField] private Image progressBar;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }

    // Update is called once per frame
    IEnumerator LoadAsyncOperation()
    {
        AsyncOperation gameLevel = SceneManager.LoadSceneAsync((int)SceneIndexes.GAME, LoadSceneMode.Additive);

        while (gameLevel.progress < 1)
        {
            progressBar.fillAmount = gameLevel.progress;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(SetUpAll());
    }

    IEnumerator SetUpAll()
    {
        yield return new WaitForSeconds(0.2f);
        loadCamera.SetActive(false);
        gameObject.SetActive(false);
    }
}
