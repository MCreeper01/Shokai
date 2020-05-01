using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSShoot : PlayerState
{
    public PSShoot(PlayerController pc)
    {
        pc.actualARShootCooldown = 0;
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (Input.GetMouseButtonUp(pc.playerModel.mouseShootButton) || pc.actualOverheat >= pc.playerModel.maxOverheatAR)
        {
            pc.shooting = false;
            pc.ChangeState(new PSMovement(pc));
        } 
    }

    public override void FixedUpdate(PlayerController pc)
    {
            
    }

    public override void Update(PlayerController pc)
    {
        pc.Move();
        pc.Aim();
        pc.actualARShootCooldown -= Time.deltaTime;
        pc.CheckHabilities();
        if (pc.actualARShootCooldown <= 0)
        {
            pc.Shoot();
        } 
        
    }
}
