using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private GameManager _gameManager;


    [Header("Buttons")]
    public GameObject combatPanelExitButton;
    public GameObject gameOverButton;


    [Header("CombatPanel")]
    public GameObject combatMonsterSprite;
    public GameObject combatPanel;
    public TextMeshProUGUI combatText;

    private int maxLines = 5;
    private int lineCount = 0;
    public GameObject scrollbarVertical;


    private void Start()
    {
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
    }

    public void B_Map()
    {
        
    }

    public void CombatActive(Sprite monsterSprite)
    {
        combatMonsterSprite.GetComponent<Image>().sprite = monsterSprite;
        combatPanel.SetActive(true);
        
    }

    public void OutputCombatText(string name1, string name2, int name1power, int name2currentHP)
    {
        if (name2currentHP < 0)
        {
            name2currentHP = 0;
        }

        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = name1 + "(이)가" + name1power + " 의 데미지를 입혔습니다. " + name2 + "의 남은 HP = " + name2currentHP;
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;


        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }

    }

    public void TestCombat()
    {

        OutputCombatText("player", "monster", 5, 5);
    }

    public void CombatPlayerWinText(string monsterName)
    {
        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = "용사가 " + monsterName + "(을)를 무찔렀습니다!";
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;

        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }

    }

    public void CombatMonsterWinText()
    {
        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = "용사의 눈앞이 깜깜해집니다..";
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;

        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }
    }

    private void ScrollCombatText()
    {
        RectTransform contentRectTransform = combatText.transform.parent.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y + 50f);

        scrollbarVertical.GetComponent<Scrollbar>().value = 0f;
    }

    public void ClearCombatText()
    {
        combatText.text = "";
        lineCount = 0;
    }

}