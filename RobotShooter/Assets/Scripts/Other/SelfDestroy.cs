using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public bool desactivate;
    public int time;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (desactivate) Invoke("Desactivate", time);
        else Destroy(gameObject, time);
    }

    void Desactivate() => gameObject.SetActive(false);

}
