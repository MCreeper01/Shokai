using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARBulletController : MonoBehaviour
{
    public float speed;
    public float damage;
    public float duration;

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
            Debug.Log("hit");
            GroundEnemy gEnemy = col.GetComponent<GroundEnemy>();
            if (gEnemy != null) gEnemy.TakeDamage(damage);
            else
            {
                FlyingEnemy fEnemy = col.GetComponent<FlyingEnemy>();
                if (fEnemy != null) fEnemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
