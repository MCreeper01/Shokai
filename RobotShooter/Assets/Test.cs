using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.instance.onRoundStart += OnRoundStart;
    }

    void OnRoundStart()
    {
        //Feu aquí el que hagueu de fer.
    }

}
