using System;
using UnityEngine;

public class ItemEvent : MonoBehaviour
{
    private GameManager _gameManager;
    private UIImgText _uiImgTxt;

    public void Start()
    {
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>(); 
    }

    public void Execute(ItemInfo info)
    {
        gameObject.SetActive(true);
        _uiImgTxt ??= GetComponent<UIImgText>();
        _uiImgTxt.Init(info.Sprite, End, info.Text);
    }


    void End()
    {
        gameObject.SetActive(false);
        _gameManager.EventPrinting = false;
    }
}