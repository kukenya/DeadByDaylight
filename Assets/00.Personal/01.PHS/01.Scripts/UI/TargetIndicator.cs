using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    public RectTransform OffScreenTargetIndicator;
    public float outOfSightOffset = 20f;
    public float imageOffSet = 10f;

    public float targetImageMoveTimeFront = 100f;
    public float targetImageMoveTimeOut = 0.1f;
    private float targetImageMoveTime;

    public GameObject target;
    private Camera mainCamera;
    private RectTransform canvasRect;

    private void Start()
    {
        targetImageMoveTime = targetImageMoveTimeFront;
    }

    public void InitialiseTargetIndicator(GameObject target, Camera mainCamera, Canvas canvas)
    {
        this.target = target;
        this.mainCamera = mainCamera;
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    public void UpdateTargetIndicator()
    {
        SetIndicatorPosition();
    }


    protected void SetIndicatorPosition()
    {
        //타겟 위치를 캔버스 좌표로 변환
        Vector3 indicatorPosition = mainCamera.WorldToScreenPoint(target.transform.position);

        //카메라 앞에서 보여질때
        if (indicatorPosition.z >= 0f & indicatorPosition.x <= canvasRect.rect.width * canvasRect.localScale.x
         & indicatorPosition.y <= canvasRect.rect.height * canvasRect.localScale.x & indicatorPosition.x >= 0f & indicatorPosition.y >= 0f)
        {
            //바꿔도 안바꿔도 노상관 
            indicatorPosition.z = 0f;

            //Target is in sight, change indicator parts around accordingly
            targetOutOfSight(false, indicatorPosition);
        }

        //내 앞에 있지만 시야 밖일때
        else if (indicatorPosition.z >= 0f)
        {
            targetImageMoveTime = targetImageMoveTimeOut;
            //Set indicatorposition and set targetIndicator to outOfSight form.
            indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);
            //indicator2Position = -(indicatorPosition - OutOfRangeindicator2(indicatorPosition));
            targetOutOfSight(true, indicatorPosition);
        }
        //내 앞도 아니고 시야 밖일때
        else
        {
            targetImageMoveTime = targetImageMoveTimeOut;
            //Invert indicatorPosition! Otherwise the indicator's positioning will invert if the target is on the backside of the camera!
            indicatorPosition *= -1f;

            //Set indicatorposition and set targetIndicator to outOfSight form.
            indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);
            //indicator2Position = -(indicatorPosition - OutOfRangeindicator2(indicatorPosition));
            targetOutOfSight(true, indicatorPosition);

        }

        //Set the position of the indicator
        OffScreenTargetIndicator.position = indicatorPosition;
    }

    private Vector3 OutOfRangeindicatorPositionB(Vector3 indicatorPosition)
    {
        //Set indicatorPosition.z to 0f; We don't need that and it'll actually cause issues if it's outside the camera range (which easily happens in my case)
        indicatorPosition.z = 0f;

        //Calculate Center of Canvas and subtract from the indicator position to have indicatorCoordinates from the Canvas Center instead the bottom left!
        Vector3 canvasCenter = new Vector3(canvasRect.rect.width / 2f, canvasRect.rect.height / 2f, 0f) * canvasRect.localScale.x;
        indicatorPosition -= canvasCenter;

        float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
        indicatorPosition.x = Mathf.Cos(Mathf.Deg2Rad * angle) * outOfSightOffset;
        indicatorPosition.y = Mathf.Sin(Mathf.Deg2Rad * angle) * outOfSightOffset;

        indicatorPosition += canvasCenter;

        return indicatorPosition;
    }

    private void targetOutOfSight(bool oos, Vector3 indicatorPosition)
    {
        //In Case the indicator is OutOfSight
        if (oos)
        {
            //Activate and Deactivate some stuff
            if (OffScreenTargetIndicator.gameObject.activeSelf == false) OffScreenTargetIndicator.gameObject.SetActive(true);

            //Set the rotation of the OutOfSight direction indicator
            OffScreenTargetIndicator.rotation = Quaternion.Euler(rotationOutOfSightTargetindicator(indicatorPosition));

            //outOfSightArrow.rectTransform.rotation  = Quaternion.LookRotation(indicatorPosition- new Vector3(canvasRect.rect.width/2f,canvasRect.rect.height/2f,0f)) ;
            /*outOfSightArrow.rectTransform.rotation = Quaternion.LookRotation(indicatorPosition);
            viewVector = indicatorPosition- new Vector3(canvasRect.rect.width/2f,canvasRect.rect.height/2f,0f);
            
            //Debug.Log("CanvasRectCenter: " + canvasRect.rect.center);
            outOfSightArrow.rectTransform.rotation *= Quaternion.Euler(0f,90f,0f);*/
        }

        //In case that the indicator is InSight, turn on the inSight stuff and turn off the OOS stuff.
        else
        {
            if (OffScreenTargetIndicator.gameObject.activeSelf == true) OffScreenTargetIndicator.gameObject.SetActive(false);
        }
    }


    private Vector3 rotationOutOfSightTargetindicator(Vector3 indicatorPosition)
    {
        //Calculate the canvasCenter
        Vector3 canvasCenter = new Vector3(canvasRect.rect.width / 2f, canvasRect.rect.height / 2f, 0f) * canvasRect.localScale.x;

        //Calculate the signedAngle between the position of the indicator and the Direction up.
        float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition - canvasCenter, Vector3.forward);

        //return the angle as a rotation Vector
        return new Vector3(0f, 0f, angle);
    }
}
