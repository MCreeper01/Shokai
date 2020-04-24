using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSMovement : PlayerState {

    public PSMovement(PlayerController pc)
    {
        //pc.rb2D.isKinematic = false;
        //pc.ator.SetInteger("State", 1);
        //pc.counter = 0;      
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (Input.GetMouseButton(pc.playerModel.mouseShootButton)) pc.ChangeState(new PSShoot(pc));
        if (!pc.moving) pc.ChangeState(new PSIdle(pc));
    }

    public override void FixedUpdate(PlayerController pc)
    {
        pc.Move();
        pc.Aim();
        if (Input.GetKeyDown(pc.playerModel.reloadKeyCode) && pc.CanReload()) pc.ReloadAR();
        if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(true);
    }

    public override void Update(PlayerController pc)
    {

    }
}
