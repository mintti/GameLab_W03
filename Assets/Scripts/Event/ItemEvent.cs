using UnityEngine;

public class ItemEvent : MonoBehaviour
{
    private UIImgText _uiImgTxt;
    public void Start()
    {
        _uiImgTxt = GetComponent<UIImgText>();
    }
    
    public void Execute(ItemInfo info)
    {
        gameObject.SetActive(true);
        _uiImgTxt.Init(info.Sprite, End, info.Text);
    }


    void End()
    {
        gameObject.SetActive(false);
    }
}