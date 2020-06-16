using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSMovement : PlayerState {

    public PSMovement(PlayerController pc)
    {
        //pc.rb2D.isKinematic = false;
        //pc.ator.SetInteger("State", 1);
        //pc.counter = 0;     
        pc.anim.SetBool("shooting", false);

    }

    public override void CheckTransition(PlayerController pc)
    {
        if (Input.GetMouseButton(pc.playerModel.mouseShootButton) && pc.shooting) pc.CheckWeaponToShoot();
        if (Input.GetMouseButtonDown(pc.playerModel.mouseShootButton) && !pc.shooting && !GameManager.instance.uiController.paused) pc.CheckWeaponToShoot();
    }

    public override void FixedUpdate(PlayerController pc)
    {
       
        
    }

    public override void Update(PlayerController pc)
    {
        pc.Move();
        pc.Aim();
        if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(true);
        if (Input.GetKeyDown(pc.playerModel.changeWeaponKey) || Input.GetMouseButtonDown(pc.playerModel.alternativeChangeWeapon)) pc.ChangeWeapon();
        if (pc.actualShotgunShootCooldown > 0)
        {
            pc.actualShotgunShootCooldown -= Time.deltaTime;
            if (pc.actualShotgunShootCooldown <= 0) pc.actualShotgunShootCooldown = 0;
        }
        pc.CheckHabilities();
        pc.CoolOverheat();
    }
}
