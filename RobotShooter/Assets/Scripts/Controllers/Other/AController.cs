using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class AController : MonoBehaviour
{
    [HideInInspector] protected GameManager gc;

    private void OnEnable()
    {
        GetGameController();
    }

    void OnLevelWasLoaded(int level)
    {
        GetGameController();
    }

    private void GetGameController()
    {
        try
        {
            gc = GameManager.instance;
            gc.AddController(this);
        }
        catch (System.Exception)
        {
            Invoke("GetGameController", .1f);
        }
    }
}
