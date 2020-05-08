using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.instance.onRoundChange += OnRoundChange;
    }

    void OnRoundChange()
    {
        //Feu aquí el que hagueu de fer.
    }

}
