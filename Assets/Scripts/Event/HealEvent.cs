using UnityEngine;

public class HealEvent : MonoBehaviour
{
    private GameManager _gameManager;
    private UIImgText _uiImgTxt;
    public int healPoint;
    public void Start()
    {
        _uiImgTxt = GetComponent<UIImgText>();
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
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
        _gameManager.EventPrinting = false;
    }
}