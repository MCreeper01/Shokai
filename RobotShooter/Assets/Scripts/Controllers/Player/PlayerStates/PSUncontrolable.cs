using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSUncontrolable : PlayerState
{

    public PSUncontrolable(PlayerController pc)
    {
        //pc.rb2D.isKinematic = false;
        //pc.ator.SetInteger("State", 1);
        //pc.counter = 0;
    }

    public override void CheckTransition(PlayerController pc)
    {
    }

    public override void FixedUpdate(PlayerController pc)
    {
        //if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(false);
    }

    public override void Update(PlayerController pc)
    {
        if (Input.GetKeyDown(pc.playerModel.interactKey) && pc.atShop) pc.Shop(false);
    }
}
