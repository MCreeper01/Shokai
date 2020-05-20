using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSLaser : PlayerState
{
    float time;

    public PSLaser(PlayerController pc)
    {
        pc.lineRenderer.gameObject.SetActive(true);
        time = pc.playerModel.laserTime;
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (time <= 0)
        {
            pc.lineRenderer.gameObject.SetActive(false);
            pc.ChangeState(pc.previousState);
        }
        
    }

    public override void FixedUpdate(PlayerController pc)
    {

    }

    public override void Update(PlayerController pc)
    {
        if (!pc.lineRenderer.gameObject.activeSelf) pc.lineRenderer.gameObject.SetActive(true);
        Vector3 endRaycastPos = Vector3.forward * pc.playerModel.laserDistance;
        RaycastHit hit;
        if (Physics.Raycast(new Ray(pc.lineRenderer.transform.position, pc.lineRenderer.transform.forward), out hit, pc.playerModel.laserDistance, pc.shootLayerMask.value))
        {
            endRaycastPos = Vector3.forward * hit.distance;
            GroundEnemy gEnemy = hit.collider.GetComponentInParent<GroundEnemy>();
            if (gEnemy != null) gEnemy.TakeDamage(pc.playerModel.laserDamage);
            else
            {
                FlyingEnemy fEnemy = hit.collider.GetComponentInParent<FlyingEnemy>();
                if (fEnemy != null) fEnemy.TakeDamage(pc.playerModel.laserDamage);
                else
                {
                    Enemy3 tEnemy = hit.collider.GetComponent<Enemy3>();
                    if (tEnemy != null) tEnemy.TakeDamage(pc.playerModel.laserDamage);
                }
            }
        }
        pc.lineRenderer.SetPosition(1, endRaycastPos);
        time -= Time.deltaTime;
        pc.Move();
        pc.Aim();
        pc.CheckHabilities();
    }
}
