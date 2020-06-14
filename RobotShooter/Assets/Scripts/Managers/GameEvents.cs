using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;
    EventInstance finishRoundSound;

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

    //All enemies cleared
    public event Action onRoundFinish;
    public void RoundFinish()
    {
        if (!AudioManager.instance.isPlaying(finishRoundSound))
        {
            finishRoundSound = AudioManager.instance.PlayEvent("FinishRound", transform.position);
        }        
        //Debug.Log("RoundFinish");
        if (onRoundFinish != null) onRoundFinish();
    }

    //Start of the TRANSITION state, before the map animation
    public event Action onTransitionStart;
    public void TransitionStart()
    {        
        //Debug.Log("PreparationFinish");
        if (onTransitionStart != null) onTransitionStart();
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
        AudioManager.instance.PlayEvent("StartRound", transform.position);
        //Debug.Log("PreparationFinish");
        if (onPreparationFinish != null) onPreparationFinish();
    }
}
