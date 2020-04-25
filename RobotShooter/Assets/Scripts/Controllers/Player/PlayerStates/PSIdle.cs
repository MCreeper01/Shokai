using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSIdle : PlayerState {

    public PSIdle(PlayerController pc)
    {
        //pc.rb2D.isKinematic = false;
        //pc.ator.SetInteger("State", 1);
        //pc.counter = 0;
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (pc.moving) pc.ChangeState(new PSMovement(pc));
        if (Input.GetMouseButton(pc.playerModel.mouseShootButton)) pc.ChangeState(new PSShoot(pc));
    }

    public override void FixedUpdate(PlayerController pc)
    {
        pc.Aim();
        pc.Move();
        if (Input.GetKeyDown(pc.playerModel.reloadKeyCode) && pc.CanReload()) pc.ReloadAR();
    }

    public override void Update(PlayerController pc)
    {

    }
}
