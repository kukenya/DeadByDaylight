using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigAxe : MonoBehaviour
{
    public GameObject survivor;
    public Image blood;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Surviver")
        {
            print(other.gameObject.name);

            survivor.GetComponent<SurviverHealth>().NormalHit();

            //공통
            // 화면에 피 튀기기
            // blood.alp
            // 도끼에 맞는 소리 나기
            SoundManager.instance.PlayHitSounds(4);

            AnnaMove.instance.OffAxe();
        }



        
        // 헛치면 미스 애니메이션 실행해야 되는데 모르겠다...
    }
}
