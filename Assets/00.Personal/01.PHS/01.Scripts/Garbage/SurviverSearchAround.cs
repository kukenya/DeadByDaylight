using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurviverSearchAround : MonoBehaviour
{
    //[Header("�÷��̾�")]
    //public float arroundSearchDist = 3f;
    //public float interactAngle = 180f;
    //public List<Transform> interactTargets = new List<Transform>();
    //public LayerMask targetMask;
    //public LayerMask obstacleMask;

    //public SurviverUI surviverUI;


    //void Update()
    //{
    //    CheckArroundInteraction();
    //}

    //void CheckArroundInteraction()
    //{
    //    // ��ȣ�ۿ� ������ ��ü�� ������ �������� ���̴��� ������.
    //    Collider[] targetColls = Physics.OverlapSphere(transform.position, arroundSearchDist, targetMask);
    //    interactTargets = new List<Transform>();
    //    for (int i = 0; i < targetColls.Length; i++)
    //    {
    //        Transform target = targetColls[i].transform;
    //        Vector3 dirToTarget = (target.position - transform.position).normalized;

    //        if (Vector3.Angle(transform.forward, dirToTarget) < interactAngle / 2)
    //        {
    //            float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

    //            // Ÿ������ ���� ����ĳ��Ʈ�� obstacleMask�� �ɸ��� ������ interactTargets�� Add
    //            if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
    //            {
    //                interactTargets.Add(target);
    //            }
    //        }
    //    }
    //    interactTargets.Sort(TransformListSortComparer);
    //    //System.Array.Sort(interactTargets, TransformListSortComparer);

    //    surviverUI.UnFocusSpaceBarUI();

    //    if (interactTargets.Count != 0)
    //    {
    //        InteractiveObject obj = interactTargets[0].GetComponent<InteractiveObject>();

    //        switch (obj.interaction)
    //        {
    //            case InteractiveObject.Type.Window:
    //                surviverUI.FocusSpaceBarUI();
    //                break;
    //            case InteractiveObject.Type.Pallet:
    //                break;
    //            case InteractiveObject.Type.ExitLever:
    //                break;
    //        }

    //        if (Input.GetKeyDown(obj.keyCode))
    //        {
    //            obj.Interact();
    //        }
    //    } 
    //}


    ////System.Array.Sort(coverColls, ColliderArraySortComparer);

    //int TransformListSortComparer(Transform A, Transform B)
    //{
    //    if (A == null && B != null)
    //    {
    //        return 1;
    //    }
    //    else if (A != null && B == null)
    //    {
    //        return -1;
    //    }
    //    else if (A == null && B == null)
    //    {
    //        return 0;
    //    }
    //    else
    //    {
    //        return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
    //    }
    //}
}
