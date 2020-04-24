using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARBulletController : MonoBehaviour
{
    public float speed;
    public float damage;

    private float duration = 3;

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            //aplicar daño al enemigo
            Destroy(gameObject);
        }
    }
}
