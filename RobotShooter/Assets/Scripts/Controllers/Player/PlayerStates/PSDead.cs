using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSDead : PlayerState
{
    public PSDead(PlayerController pc)
    {
        pc.StartDeath();
    }

    public override void CheckTransition(PlayerController pc)
    {

    }

    public override void FixedUpdate(PlayerController pc)
    {

    }

    public override void Update(PlayerController pc)
    {

    }
}
