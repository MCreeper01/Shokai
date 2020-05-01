using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSDefense : PlayerState
{
    public PSDefense(PlayerController pc)
    {
        pc.withDefense = true;
        pc.gun.SetActive(false);
        pc.PrepareDeffense();
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (Input.GetMouseButtonDown(pc.playerModel.mouseShootButton)) pc.PlaceDeffense();
        if (Input.GetKeyDown(pc.playerModel.changeWeaponKey))
        {
            pc.DestroyDefense();
            pc.gun.SetActive(true);
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
        pc.CheckHabilities();
        pc.UpdateAttachedDefense();
        if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(true);
    }
}
