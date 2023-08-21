using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public GameObject body;
    public GameObject mask;

    public Material changematerial;

    public void ChangeMaterials()
    {
        // 머리와 마스크 마테리얼을 투명하게 설정한다.
        Material[] bodymat = body.GetComponent<SkinnedMeshRenderer>().materials;
        Material[] maskmat = mask.GetComponent<SkinnedMeshRenderer>().materials;

        bodymat[3] = changematerial;
        maskmat[0] = changematerial;
        maskmat[1] = changematerial;

        body.GetComponent<SkinnedMeshRenderer>().materials = bodymat;
        mask.GetComponent<SkinnedMeshRenderer>().materials = maskmat;
    }
}
