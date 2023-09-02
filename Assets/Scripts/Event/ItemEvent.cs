using System;
using UnityEngine;

public class ItemEvent : MonoBehaviour
{
    private GameManager _gameManager;
    private UIImgText _uiImgTxt;


    public void Execute(ItemInfo info)
    {
        gameObject.SetActive(true);
        _uiImgTxt ??= GetComponent<UIImgText>();
        _gameManager ??= GameObject.Find(nameof(GameManager)).GetComponent<GameManager>(); 
        
        switch(info.Type){
            case EventType.HP:
                _gameManager.knight.Status.CurrentHp += info.EffectAmount;
            break;
            case EventType.Power:
                _gameManager.knight.Status.Power += info.EffectAmount;

            break;

        }
        _uiImgTxt.Init(info.Sprite, End, info.Text);
    }


    void End()
    {
        gameObject.SetActive(false);
        _gameManager.EventPrinting = false;
    }
}