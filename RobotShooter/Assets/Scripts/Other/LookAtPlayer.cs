using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        if (GameManager.instance.player != null) transform.LookAt(GameManager.instance.player.transform);
    }
}
