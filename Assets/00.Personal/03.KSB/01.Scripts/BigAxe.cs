using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigAxe : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Surviver")
        {
            print(other.gameObject.name);

            // 건강한 생존자를 때릴 때
            // 생존자 부상 상태
            // 생존자 UI 바꾸기

            // 부상 당한 생존자를 때릴 때
            // 생존자 기절 상태
            // 생존자 UI 바꾸기


            //공통
            // 화면에 피 튀기기
            // 도끼에 맞는 소리 나기
            SoundManager.instance.PlayHitSounds(4);

            AnnaMove.instance.OffAxe();
        }



        
        // 헛치면 미스 애니메이션 실행해야 되는데 모르겠다...
    }
}
