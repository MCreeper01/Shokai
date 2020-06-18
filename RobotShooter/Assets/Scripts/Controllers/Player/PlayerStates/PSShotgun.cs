using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSShotgun : PlayerState
{
    public PSShotgun(PlayerController pc)
    {
        pc.shooting = true;
        pc.muzzleFlashAR.Simulate(1);      
        pc.muzzleFlashShotgun.Play();
        pc.anim.SetBool("shooting", true);
    }

    public override void CheckTransition(PlayerController pc)
    {
        if (pc.shotgunShotted)
        {           
            pc.shooting = false;
            pc.anim.SetBool("shooting", false);
            pc.ChangeState(new PSMovement(pc));
        } 
    }

    public override void FixedUpdate(PlayerController pc)
    {
        
    }

    public override void Update(PlayerController pc)
    {
        if (!pc.muzzleFlashShotgun.isPlaying) pc.muzzleFlashShotgun.Play();
        pc.Move();
        pc.Aim();
        pc.Shoot();
        pc.CheckHabilities();
    }
}
