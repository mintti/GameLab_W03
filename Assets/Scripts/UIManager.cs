using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private GameManager _gameManager;
    private Player _player1;
    private Player _player2;

    [Header("게임 정보")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI waveText;
    
    [Header("CostUI")]
    public TextMeshProUGUI leftCostLeft;

    [Header("플레이어 정보")] 
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI powerText;

    public TextMeshProUGUI skillInfoText;

    [Header("CombatPanelUI")]
    public GameObject combatPanel;
    public TextMeshProUGUI combatText;
    public TextMeshProUGUI monsterInf;

    private int maxLines = 5;
    private int lineCount = 0;
    public GameObject scrollbarVertical;

    public GameObject combatPanelExitButton;
    public GameObject gameOverButton;

    [Header("화면 제어")]
    public GameObject gameOverObj;
    public GameObject gameClearObj;
    public GameObject endingScreen;

    [Header("미니 상점")]
    public TextMeshProUGUI ShopProductInfoText;
    public TextMeshProUGUI CoinText;

    [Header("턴 종료 UI")]
    public float blinkInterval = 0.5f;
    private bool shouldBlink1 = false;
    private bool shouldBlink2 = false;
    public float nextBlinkTime1 = 0f;
    public float nextBlinkTime2 = 0f;
    public TextMeshProUGUI blinkText1;
    public TextMeshProUGUI blinkText2;


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
        _player1 = GameObject.Find(nameof(Knight)).GetComponent<Player>();
        _player2 = GameObject.Find(nameof(Princess)).GetComponent<Player>();
        infoText.text = string.Empty;
    }

    public void B_Map()
    {
        
    }

    private void Update()
    {
        Debug.Log(_player1.Cost);
        if (Time.time > nextBlinkTime1 && _player1.Cost == 0)
        {
            nextBlinkTime1 = Time.time + blinkInterval;
            shouldBlink1 = !shouldBlink1;
            blinkText1.enabled = shouldBlink1;
        }

        if (Time.time > nextBlinkTime2 && _player2.Cost == 0)
        {
            nextBlinkTime2 = Time.time + blinkInterval;
            shouldBlink2 = !shouldBlink2;
            blinkText2.enabled = shouldBlink2;
        }

    }

    public void BlinkText1Reset()
    {
        shouldBlink1 = false;
        blinkText1.enabled = true;
        nextBlinkTime1 = 0f;
    }

    public void BlinkText2Reset()
    {
        shouldBlink2 = false;
        blinkText2.enabled = true;
        nextBlinkTime2 = 0f;
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
        Debug.Log(_gameManager.whoseTurn);

        string text;
        if (_gameManager.whoseTurn.Equals(nameof(Princess).ToLower()))
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
        powerText.text = $"<color=#FFD400>파워</color>  {(status.Buff ? $"{status.Power +2}(버프)" : status.Power)}";
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

    public void UpdateTurnText(int turn)
    {
        turnText.text = $"{turn} 턴";
        waveText.text = $"{_gameManager.waveInterval - (turn % _gameManager.waveInterval)}턴 후 몬스터가 증식합니다.";
    }

    public GameObject infoTextObj;
    public TextMeshProUGUI infoText;
    private float timer;
    public void OutputInfo(string text)
    {
        StartCoroutine(Message(text));
    }

    IEnumerator Message(string text)
    {
        timer = .7f;
        
        do
        {
            infoTextObj.SetActive(true);
            infoText.text = text;
            yield return new WaitForSeconds(0.1f);
            timer -= .1f;
        } while (timer > 0);
        
        
        infoTextObj.SetActive(false);
    }

    public void UpdateCoinText(int coin)
    {
        CoinText.text = coin.ToString();
    }

    public void UpdateMiniShopInfoText(string text)
    {
        ShopProductInfoText.text = text;
    }
}