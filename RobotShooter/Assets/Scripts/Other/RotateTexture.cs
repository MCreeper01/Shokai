using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTexture : MonoBehaviour
{

    public float speed;
    Vector2 offset = Vector2.zero;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        offset.y = (offset.y + speed * Time.deltaTime) % 1;
        GetComponent<MeshRenderer>().material.mainTextureOffset = offset;
    }
}
