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
        if (Input.GetMouseButtonDown(pc.playerModel.mouseShootButton)) pc.ChangeState(new PSAttack(pc));
    }

    public override void FixedUpdate(PlayerController pc)
    {
        pc.Move();
        pc.Aim();
    }

    public override void Update(PlayerController pc)
    {

    }
}
