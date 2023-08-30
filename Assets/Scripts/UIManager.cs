using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private GameManager _gameManager;

    [Header("CostUI")]
    public TextMeshProUGUI leftCostLeft;

    [Header("플레이어 정보")] 
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI powerText;

    public TextMeshProUGUI skillInfoText;

    [Header("CombatPanelUI")]
    public GameObject combatPanel;
    public TextMeshProUGUI combatText;

    private int maxLines = 5;
    private int lineCount = 0;
    public GameObject scrollbarVertical;

    public GameObject combatPanelExitButton;
    public GameObject gameOverButton;

    [Header("화면 제어")]
    public GameObject gameOverObj;
    public GameObject gameClearObj;
    public GameObject endingScreen;

    private Dictionary<int, string> _princessSkillInfoDict = new()
    {
        {0, "주변을 밝힙니다. (행동력 -1 소모)"},
        {1, "밝힌 영역 중 지정한 위치에 일회성 회복 존을 생성합니다. (행동력 -2 소모)"},
        {2, "다음 전투에서 용사를 강화합니다. (행동력 -3 소모)"},
    };

    private Dictionary<int, string> _knightSkillInfoDict = new()
    {
        {0, "한 칸 이동하고 영역을 밝힙니다. (행동력 -1 소모)"},
        {1, "지정한 영역을 밝힙니다. (행동력 -2 소모)"},
        {2, "한 칸 뛰어넘어 이동합니다. (행동력 -3 소모)"},
    };

    private void Awake()
    {
        Instance = this;
        skillInfoText.text = _knightSkillInfoDict[0];
    }

    private void Start()
    {
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
    }

    public void B_Map()
    {
        
    }

    public void CombatActive()
    {
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

    public void FocusSkill(GameObject skillui, int index)
    {

        GameObject[] skillArray = new GameObject[skillui.transform.childCount];

        // Iterate through the child objects and store them in the array
        for (int i = 0; i < skillui.transform.childCount; i++)
        {
            skillArray[i] = skillui.transform.GetChild(i).gameObject;
        }
        // Iterate through all objects and disable outlines
        for (int i = 0; i < skillArray.Length; i++)
        {
            Outline outline = skillArray[i].GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
        }

        // Activate the outline for the object at the specified index
        if (index >= 0 && index < skillArray.Length)
        {
            Outline outline = skillArray[index].GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
            }
        }
    }

    public void UpdateCostText(int cost)
    {
        leftCostLeft.text = $"{cost}";
    }
    
    public void UpdateInfoText(int index)
    {
        string text;
        if (_gameManager.whoseTurn.Equals(nameof(Princess)))
        {
            text = _princessSkillInfoDict[index];
        }
        else
        {
            text = _knightSkillInfoDict[index];
        }

        skillInfoText.text = text;
        skillInfoText.gameObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void UpdateKnightStatusInfo(Status status)
    {
        hpText.text = $"<color=#D1180B>체력</color>  {status.CurrentHp}/{status.MaxHp}";
        powerText.text = $"<color=#FFD400>파워</color>  {status.Power}";
    }

    public void ActiveEndingScene()
    {
        endingScreen.SetActive(true);
        StartCoroutine(ShowEnding(gameClearObj));
    }

    public void ActiveGameOverObj()
    {
        endingScreen.SetActive(true);
        StartCoroutine(ShowEnding(gameOverObj));
    }

    IEnumerator ShowEnding(GameObject obj)
    {
        yield return new WaitForSeconds(2f);
        obj.SetActive(true);
    }
}