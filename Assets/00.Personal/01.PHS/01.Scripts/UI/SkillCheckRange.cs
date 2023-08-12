using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCheckRange : MonoBehaviour
{
    [Range(0, 360)]
    public float normalCheckStartPos;
    [Range(0, 360)]
    public float normalCheckEndPos;
    [Range(0, 360)]
    public float hardCheckStartPos;
    [Range(0, 360)]
    public float hardCheckEndPos;

}
