using UnityEngine;

public class HealEvent : MonoBehaviour
{
    private UIImgText _uiImgTxt;
    public int healPoint;
    public void Start()
    {
        _uiImgTxt = GetComponent<UIImgText>();
    }

    public void Execute(Knight knight, Sprite sprite)
    {
        gameObject.SetActive(true);
        knight.Status.CurrentHp += healPoint;
        _uiImgTxt.Init(sprite, End, "체력이 회복 되었습니다.");
    }

    void End()
    {
        gameObject.SetActive(false);
    }
}