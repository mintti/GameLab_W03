using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleEvent : MonoBehaviour
{
    [Header("사운드 안내 문구 ")] 
    public RectTransform text1;
    public RectTransform text2;


    public void Start()
    {
        StartCoroutine(MoveSound());
    }
    
    IEnumerator MoveSound()
    {
        bool isFirst = true;
        var moveStart = -2128;

        while (true)
        {
            text1.position -= new Vector3(1, 0, 0) ;
            text2.position -= new Vector3(1, 0, 0) ;

            if (isFirst)
            {
                if (text1.position.x < moveStart)
                {
                    text1.position = new Vector3(1976, text1.position.y, text1.position.z);
                    isFirst = false;
                }
            }
            else
            {
                if (text2.position.x < moveStart)
                {
                    text2.position = new Vector3(1976, text2.position.y, text2.position.z);
                    isFirst = true;
                }
            }

            yield return new WaitForSeconds(0.002f);
        }
    }
}
