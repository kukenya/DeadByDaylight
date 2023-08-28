using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Pallet : MonoBehaviourPun
{
    public enum PalletState
    {
        Stand,
        Ground,
        Destroy
    }

    public PalletState state;
    public PalletState State { get { return state; } set {
            photonView.RPC(nameof(SetState), RpcTarget.All, value);
         }
    }

    public float destroyDelayTime = 1f;
    public GameObject[] pallet;
    public GameObject destroyedPallet;

    [PunRPC]
    void SetState(PalletState value)
    {
        if (value == PalletState.Ground)
        {
            Play("FallOnGround");
            StartCoroutine(Stun());
        }
        else if(value == PalletState.Destroy)
        {
            StartCoroutine(DestroyPallet());
        }
        state = value;
    }

    public float stunTime = 1f;
    public bool isStun = false;

    IEnumerator Stun()
    {
        isStun = true;
        yield return new WaitForSeconds(stunTime);
        isStun = false;
    }

    IEnumerator DestroyPallet()
    {
        Play("Destroy");
        yield return new WaitForSeconds(destroyDelayTime);
        destroyedPallet.SetActive(true);
        foreach(GameObject go in pallet)
        {
            Destroy(go);
        }
    }

    public LayerMask layerMask;

    public Animator anim;
    string currentState;

    public Transform animPos1;
    public Transform animPos2;

    public void Play(string state, float time = 0.1f)
    {
        if (state == currentState) return;

        anim.enabled = true;
        anim.CrossFadeInFixedTime(state, time, 0);
        currentState = state;
    }

    public Transform GetAnimPosition(Vector3 player)
    {
        float dist = Vector3.Distance(animPos1.position, player);
        float dist2 = Vector3.Distance(animPos2.position, player);

        return dist < dist2 ? animPos1 : animPos2;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStun == false) return;

        if (collision.gameObject.name.Contains("Anna"))
        {
            collision.gameObject.GetComponent<AnnaMove>().Stunned();
        }
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(state);
    //    }
    //    else
    //    {
    //        state = (PalletState)stream.ReceiveNext();
    //    }
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    interaction = other.GetComponent<SurvivorInteraction>();
    //    if (Physics.Raycast(other.transform.position, transform.position - other.transform.position, out RaycastHit hitInfo, 3, layerMask))
    //    {
    //        if(hitInfo.transform.gameObject.layer == 7)
    //        {
    //            SurviverUI.instance.FocusSpaceBarUI();
    //            float dist = Vector3.Distance(animPos1.position, other.transform.position);
    //            float dist2 = Vector3.Distance(animPos2.position, other.transform.position);

    //            interaction.ChangeInteract(SurvivorInteraction.InteractiveType.Pallet, this, dist < dist2 ? animPos1 : animPos2);
    //        }
    //        else
    //        {
    //            SurviverUI.instance.UnFocusSpaceBarUI();
    //            interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    SurviverUI.instance.UnFocusSpaceBarUI();
    //    interaction.ChangeInteract(SurvivorInteraction.InteractiveType.None);
    //}
}
