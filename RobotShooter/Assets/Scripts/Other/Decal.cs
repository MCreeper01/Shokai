using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decal : MonoBehaviour
{
    public List<GameObject> rayPoints;
    public LayerMask ground;
    public MeshRenderer meshRenderer;

    void OnEnable()
    {
        foreach (GameObject ray in rayPoints)
        {
            RaycastHit hit;
            if (!Physics.Raycast(ray.transform.position, -ray.transform.forward.normalized, out hit, 1f, ground))
            {
                Destroy(gameObject);
            }
            else Debug.Log(hit.collider.name);
        }
        meshRenderer.enabled = true;
    }
}
