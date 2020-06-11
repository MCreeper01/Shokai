using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartActive : MonoBehaviour
{
    public bool startActive;
    void Start()
    {
        gameObject.SetActive(startActive);
    }
}
