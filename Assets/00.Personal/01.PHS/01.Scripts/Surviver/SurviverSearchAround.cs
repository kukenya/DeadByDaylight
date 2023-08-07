using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviverSearchAround : MonoBehaviour
{
    [Header("�÷��̾�")]
    public float arroundSearchDist = 3f;
    public float interactAngle = 180f;
    public List<Transform> interactTargets;
    public LayerMask targetMask;
    public LayerMask obstacleMask;


    void Update()
    {
        CheckArroundInteraction();
    }

    void CheckArroundInteraction()
    {
        // ��ȣ�ۿ� ������ ��ü�� ������ �������� ���̴��� ������.
        Collider[] targetColls = Physics.OverlapSphere(transform.position, arroundSearchDist, targetMask);
        //int hitReduction = 0;

        for (int i = 0; i < targetColls.Length; i++)
        {
            Transform target = targetColls[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < interactAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position);

                // Ÿ������ ���� ����ĳ��Ʈ�� obstacleMask�� �ɸ��� ������ visibleTargets�� Add
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    interactTargets.Add(target);
                }
            }
        }

        interactTargets.Sort(TransformListSortComparer);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            Vector3 a = interactTargets[0].position;
        }
    }


    //System.Array.Sort(coverColls, ColliderArraySortComparer);

    int TransformListSortComparer(Transform A, Transform B)
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
