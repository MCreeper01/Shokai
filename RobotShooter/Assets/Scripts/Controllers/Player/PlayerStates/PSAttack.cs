using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSAttack : PlayerState
{
    public float attackCooldown;

    public PSAttack(PlayerController pc)
    {
        attackCooldown = pc.playerModel.shootARCooldown;
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
