using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    //End of CLEAR state, before the map animation
    public event Action onRoundFinish;
    public void RoundFinish()
    {
        //Debug.Log("RoundFinish");
        if (onRoundFinish != null) onRoundFinish();
    }

    //Start of the PREPARATION state, after the map animation
    public event Action onRoundStart;
    public void RoundStart()
    {
        //Debug.Log("RoundStart");
        if (onRoundStart != null) onRoundStart();
    }

    //End of the PREPARATION state
    public event Action onPreparationFinish;
    public void PreparationFinish()
    {
        //Debug.Log("PreparationFinish");
        if (onPreparationFinish != null) onPreparationFinish();
    }
}
