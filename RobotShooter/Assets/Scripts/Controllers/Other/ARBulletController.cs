using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARBulletController : MonoBehaviour
{
    public float speed;
    public float damage;
    public float duration;

    public LayerMask mask;

    private Vector3 previousPosition;

    void Start()
    {
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) Destroy(gameObject);

        previousPosition = transform.position;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        RaycastHit[] hits = Physics.RaycastAll(previousPosition, (transform.position - previousPosition).normalized, (transform.position - previousPosition).magnitude, GameManager.instance.player.shootLayerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            LayerMask layer = hits[i].collider.gameObject.layer;
            //Debug.Log(hits[i].collider.name);
            if (layer == LayerMask.NameToLayer("Enemy"))
            {
                GroundEnemy gEnemy = hits[i].collider.GetComponent<GroundEnemy>();
                if (gEnemy != null) gEnemy.TakeDamage(damage);
                else
                {
                    FlyingEnemy fEnemy = hits[i].collider.GetComponent<FlyingEnemy>();
                    if (fEnemy != null) fEnemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            if (layer == LayerMask.NameToLayer("Geometry"))
            {
                Destroy(gameObject);
            }
        }       
    }

    void FixedUpdate()
    {
        /*Debug.DrawRay(previousPosition, (transform.position - previousPosition).normalized, Color.red, 10);

        RaycastHit[] hits = Physics.RaycastAll(previousPosition, (transform.position - previousPosition).normalized, (transform.position - previousPosition).magnitude, GameManager.instance.player.shootLayerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            LayerMask layer = hits[i].collider.gameObject.layer;
            //Debug.Log(hits[i].collider.name);
            Debug.Log(hits[i].point);
            if (layer == LayerMask.NameToLayer("Enemy"))
            {
                GroundEnemy gEnemy = hits[i].collider.GetComponent<GroundEnemy>();
                if (gEnemy != null) gEnemy.TakeDamage(damage);
                else
                {
                    FlyingEnemy fEnemy = hits[i].collider.GetComponent<FlyingEnemy>();
                    if (fEnemy != null) fEnemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            if (layer == LayerMask.NameToLayer("Geometry"))
            {
                Destroy(gameObject);
            }
        }

        previousPosition = transform.position;*/
    }

    //void OnTriggerEnter(Collider col)
    //{
    //    if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
    //    {
    //        Debug.Log("hit");
    //        GroundEnemy gEnemy = col.GetComponent<GroundEnemy>();
    //        if (gEnemy != null) gEnemy.TakeDamage(damage);
    //        else
    //        {
    //            FlyingEnemy fEnemy = col.GetComponent<FlyingEnemy>();
    //            if (fEnemy != null) fEnemy.TakeDamage(damage);
    //        }
    //        Destroy(gameObject);
    //    }
    //}
}
