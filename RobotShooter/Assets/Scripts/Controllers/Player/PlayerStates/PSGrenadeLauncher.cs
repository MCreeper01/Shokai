using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSGrenadeLauncher : PlayerState
{
    public PSGrenadeLauncher(PlayerController pc)
    {

    }

    public override void CheckTransition(PlayerController pc)
    {
        if (pc.grenadeAmmo <= 0 || Input.GetKeyDown(pc.playerModel.changeWeaponKey) || Input.GetMouseButtonDown(pc.playerModel.alternativeChangeWeapon)) pc.ChangeWeapon();
    }

    public override void FixedUpdate(PlayerController pc)
    {

    }

    public override void Update(PlayerController pc)
    {
        pc.Move();
        pc.Aim();
        pc.CheckHabilities();
        if (Input.GetMouseButtonDown(pc.playerModel.mouseShootButton)) pc.Shoot();
        if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(true);
    }
}
