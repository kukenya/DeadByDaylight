using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalletChild : MonoBehaviour
{
    public Pallet pallet;

    public void Interact()
    {
        switch (pallet.state)
        {
            case Pallet.State.Stand:
                pallet.Play("FallOnGround");
                SurviverObstacles.instance.StartPullDownPallet(transform);
                pallet.state = Pallet.State.Ground;
                break;
            case Pallet.State.Ground:
                SurviverObstacles.instance.StartJumpPallet(transform);
                break;
        }
    }
}
