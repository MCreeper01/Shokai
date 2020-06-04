using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMPController : MonoBehaviour
{
    public float scaleSpeed;
    PlayerModel playerModel;

    private void Start()
    {
        playerModel = GameManager.instance.player.playerModel;
    }

    void Update()
    {
        if (transform.localScale.x <= playerModel.empRadius * 2) //The scale equals the diameter
        {
            Vector3 s = transform.localScale;
            transform.localScale = new Vector3(s.x + scaleSpeed, s.y + scaleSpeed, s.z + scaleSpeed);
        } else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        GroundEnemy gEnemy = collider.GetComponentInParent<GroundEnemy>();
        if (gEnemy != null) gEnemy.ActivateStun(playerModel.empDuration);
        else
        {
            FlyingEnemy fEnemy = collider.GetComponentInParent<FlyingEnemy>();
            if (fEnemy != null) fEnemy.ActivateStun(playerModel.empDuration);
            else
            {
                TankEnemy tEnemy = collider.GetComponentInParent<TankEnemy>();
                if (tEnemy != null) tEnemy.ActivateStun(playerModel.empDuration);
            }
        }
    }
}
