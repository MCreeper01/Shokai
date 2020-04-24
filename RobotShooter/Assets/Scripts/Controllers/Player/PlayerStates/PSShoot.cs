using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSShoot : PlayerState
{
    public float attackCooldown;

    public PSShoot(PlayerController pc)
    {
        attackCooldown = pc.playerModel.shootARCooldown;
        pc.ShootAR();
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (attackCooldown <= 0) pc.ChangeState(new PSMovement(pc));
    }

    public override void FixedUpdate(PlayerController pc)
    {
        pc.Move();
        pc.Aim();        
    }

    public override void Update(PlayerController pc)
    {
        attackCooldown -= Time.deltaTime;
    }

}
