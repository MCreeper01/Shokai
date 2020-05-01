using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSUncontrolable : PlayerState
{

    public PSUncontrolable(PlayerController pc)
    {

    }

    public override void CheckTransition(PlayerController pc)
    {
    }

    public override void FixedUpdate(PlayerController pc)
    {

    }

    public override void Update(PlayerController pc)
    {
        if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(false);
        pc.CoolOverheat();
    }
}
