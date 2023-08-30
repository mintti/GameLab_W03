using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIImgText : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    private bool click = false;
    
    public void Init(Sprite sprite, Action endCb = null, params string[] texts)
    {
        image.sprite = sprite;
        StartCoroutine(TypingText(texts, endCb));
    }

    IEnumerator TypingText(string[] texts, Action endCb)
    {
        int index = 0;
        
        do
        {
            click = false;
            int charIndex = 0;
            text.text = string.Empty;
            
            do // 타이핑 효과
            {
                yield return new WaitForSeconds(0.1f);
                text.text += texts[index][charIndex++];
                
                if (click) // 타이핑 효과를 기다리지 않음
                {
                    click = false;
                    text.text = texts[index];
                }
            } while (charIndex < texts[index].Length);

            yield return new WaitUntil(() => click);
        } while (texts.Length < ++index);
        
        // 종료
        endCb?.Invoke();
    }

    public void B_Click() => click = true;
}
