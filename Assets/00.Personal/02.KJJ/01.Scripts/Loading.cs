using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image loading1;
    public Image loading2;
    public Image loading3;
    public Image loading4;
    public Image loading5;

    public bool time;
    public float currentTime;
    // Start is called before the first frame update
    void Start()
    {
        time = true;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (time == true && currentTime > 2f)
        {
            if (loading1.fillAmount <= 1f)
            {
                loading1.fillAmount = Mathf.MoveTowards(loading1.fillAmount, 1f, Time.deltaTime);
                if (loading1.fillAmount >= 1f && loading2.fillAmount <= 1f)
                {
                    loading2.fillAmount = Mathf.MoveTowards(loading2.fillAmount, 1f, Time.deltaTime);
                    if (loading2.fillAmount >= 1f && loading3.fillAmount <= 1f)
                    {
                        loading3.fillAmount = Mathf.MoveTowards(loading3.fillAmount, 1f, Time.deltaTime);
                        if (loading3.fillAmount >= 1f && loading4.fillAmount <= 1f)
                        {
                            loading4.fillAmount = Mathf.MoveTowards(loading4.fillAmount, 1f, Time.deltaTime);
                            if (loading4.fillAmount >= 1f && loading5.fillAmount <= 1f)
                            {
                                loading5.fillAmount = Mathf.MoveTowards(loading5.fillAmount, 1f, Time.deltaTime);
                                if (loading5.fillAmount >= 1f)
                                {
                                    currentTime = 0;
                                    time = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (time == false && currentTime > 2f)
        {
            if (loading5.fillAmount >= 1f)
            {
                loading1.fillAmount = 0;
                loading2.fillAmount = 0;
                loading3.fillAmount = 0;
                loading4.fillAmount = 0;
                loading5.fillAmount = 0;
                currentTime = 0;
                time = true;
            }
        }

    }
}
