using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public float damage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
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
