using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverArroundUI : MonoBehaviour
{
    [Header("플레이어")]
    public float arroundSearchDist = 3f;
    public LayerMask layerMask;


    void Update()
    {
        CheckArroundInteraction();
    }

    void CheckArroundInteraction()
    {
        Collider[] arroundColls = new Collider[5];
        // 본인을 기준으로 엄페물을 찾기 위한 레이더를 돌린다.
        int hits = Physics.OverlapSphereNonAlloc(transform.position, arroundSearchDist, arroundColls, layerMask);
        int hitReduction = 0;

        for(int i = 0; i < hits; i++)
        {

        }
    }


    //System.Array.Sort(coverColls, ColliderArraySortComparer);

    int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
        }
    }
}
