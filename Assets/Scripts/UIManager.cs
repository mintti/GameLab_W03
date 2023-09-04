using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.UI;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private GameManager _gameManager;
    private Player _player1;
    private Player _player2;

    [Header("Common")]
    public GameObject somethingBoxObj;
    public TextMeshProUGUI somethingText;
    public Action SomethingAction { get; set; }

    [Header("게임 정보")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI waveText;
    
    [Header("CostUI")]
    public TextMeshProUGUI leftCostLeft;

    [Header("플레이어 정보")]
    public TextMeshProUGUI knightLvText;
    public Text levelUpInfoText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI dexText;
    public Button[] statusUpButtons;
    public TextMeshProUGUI statusPoint;
    public RectTransform expBar;
    
    public TextMeshProUGUI skillInfoText;

    [Header("화면 제어")]
    public GameObject gameOverObj;
    public GameObject gameClearObj;
    public GameObject endingScreen;

    [Header("턴 종료 UI")]
    public float blinkInterval = 0.5f;
    private bool shouldBlink1 = false;
    private bool shouldBlink2 = false;
    public float nextBlinkTime1 = 0f;
    public float nextBlinkTime2 = 0f;
    public TextMeshProUGUI blinkText1;
    public TextMeshProUGUI blinkText2;

    [Header("BloodEffect")]
    public Image bloodEffect;

    [Header("타일 패널 UI")]
    public GameObject tileInfPanel;
    public TextMeshProUGUI tileName;
    public GameObject monInf;
    public GameObject eventInf;
    public GameObject itemInf;
    public Image MonImg;
    public Image key;
    public TextMeshProUGUI monHP;
    public TextMeshProUGUI monPow;
    public TextMeshProUGUI monDefense;
    public TextMeshProUGUI monDex;
    public TextMeshProUGUI monName;
    public TextMeshProUGUI monExp;
    public TextMeshProUGUI eventText;
    public TextMeshProUGUI itemText;


    [Header("탑/층 관련")]
    public GameObject[] currentFloorArrows;
    public GameObject BurningObj;


    private Dictionary<int, string> _princessSkillInfoDict = new()
    {
        {0, "특정 타일의 3x3을 영구적으로 밝힙니다. (행동력 1 소모)"},
        {1, "다음 턴, 용사의 파워를 2 증가시킵니다. (행동력 3 소모)"},
    };

    private Dictionary<int, string> _knightSkillInfoDict = new()
    {
        {0, "한 칸 이동합니다.(행동력 1 소모) 공주가 밝힌 곳은, 행동력이 소모되지 않습니다." },
        {1, "남은 행동력을 소모하여, 소모한 행동력 만큼 회복합니다. (행동력 x N 소모)" },
    };

    private void Awake()
    {
        Instance = this;
        skillInfoText.text = _knightSkillInfoDict[0];
    }

    private void Start()
    {
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
        _player1 = GameObject.Find("Knight").GetComponent<Player>();
        _player2 = GameObject.Find(nameof(Princess)).GetComponent<Player>();
        
        infoText.text = string.Empty;
    }

    public void B_Map()
    {
        
    }

    private void Update()
    {
        // Debug.Log(_player1.Cost);
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
        //skillInfoText.gameObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    #region Player LV/Status Info
    public void UpdateKnightStatusInfo()
    {
        Status status = _gameManager.knight.Status;

        if (status == null) return;
        
        // 용사, 레벨
        knightLvText.text = $"용사 LV.{status.Level}";
        levelUpInfoText.text = $"EXP {status.Exp}/{DataManager.Instance.ExpNeedForLevelUp[status.Level - 1]}";
        
        int calculWidth = 350 *  status.Exp/DataManager.Instance.ExpNeedForLevelUp[status.Level - 1];
        expBar.sizeDelta = new Vector2(calculWidth, expBar.sizeDelta.y);
        
        // 스텟
        hpText.text = $"<color=#D1180B>체력</color>  {status.CurrentHp}/{status.MaxHp}";
        powerText.text = $"<color=#FFD400>파워</color>  {status.Power}{(status.Buff ? $"(버프)" : "")}";
        defenseText.text = $"<color=#0000FF>방어</color>  {status.Defense}";
        dexText.text = $"<color=#80FF00>민첩</color>  {status.Dex}";


        statusPoint.text = $"Status Point {_gameManager.StatusPoint}";
        
        // 스텟 업 버튼
        bool canUp = _gameManager.StatusPoint > 0;
        foreach (var btn in statusUpButtons)
        {
            btn.interactable = canUp;
        }
    }

    public void StatusUp(string statusName)
    {
        Status status = _gameManager.knight.Status;
        switch (statusName)
        {
            case "hp" :
                status.MaxHp++;
                status.CurrentHp++;        
                break;
            case "power" :
                status.Power++;
                break;
            case "defense" :
                status.Defense++;
                break;
            case "dex" :
                status.Dex++;
                break;
        }

        _gameManager.StatusPoint--;
        UpdateKnightStatusInfo();
    }
    #endregion

    #region 탑 / 층

    public void UpdateCurrentDisplayFloor(int floor)
    {
        foreach (var arrow in currentFloorArrows)
        {
            arrow.SetActive(false);
        }
        
        currentFloorArrows[floor-1].SetActive(true);
    }
    

    #endregion
    
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
        turnText.text = $"현재 턴 : {turn}";

        if (_gameManager.CheckDotDam())
        {
            waveText.text = $"용사의 차례가 종료 후 { _gameManager.GetDotDam()} 데미지를 입습니다.";
        }
        else
        { 
            waveText.text = $"{-_gameManager.GetDotDam()}턴 후 {_gameManager.CurrentKnightFloor}층이 분화합니다.";
        }
    }

    public GameObject infoTextObj;
    public TextMeshProUGUI infoText;
    private float timer;
    private Coroutine currentMessageCoroutine;

    public void OutputInfo(string text)
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine); // Stop the previous coroutine
        }

        currentMessageCoroutine = StartCoroutine(Message(text));
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


    public void TileInfUI(MapType mapType, object obj = null)
    {
        tileInfPanel.SetActive(false);
        tileName.enabled = false;
        monInf.SetActive(false);
        eventInf.SetActive(false);
        itemInf.SetActive(false);
        key.enabled = false;

        if (mapType == MapType.Monster)
        {
            tileName.enabled = true;
            tileInfPanel.SetActive(true);
            monInf.SetActive(true);

            Monster monster = obj as Monster;
            MonImg.sprite = monster.Sprite;
            tileName.text = "몬스터";
            monHP.text = "<color=#D1180B>체력</color> : " + monster.Status.MaxHp;
            monPow.text = "<color=#FFD400>파워</color> : " + monster.Status.Power;
            monDefense.text = "<color=#0000FF>방어</color> : " + monster.Status.Defense;
            monDex.text = "<color=#80FF00>민첩</color> : " + monster.Status.Dex;
            monExp.text = "<color=#8A2BE2>EXP</color>  + " + monster.Status.Exp;
            monName.text = monster.Name;
        }

        else if (mapType == MapType.Item)
        {
            tileName.enabled = true;
            tileInfPanel.SetActive(true);
            itemInf.SetActive(true);

            tileName.text = "아이템";
            itemText.text = "좋은일이 일어날 것 같은 아이템 입니다.";
        }

        else if (mapType == MapType.Event)
        {
            tileName.enabled = true;
            tileInfPanel.SetActive(true);
            eventInf.SetActive(true);

            tileName.text = "이벤트";
            eventText.text = "무슨 일이 일어날 것 같습니다.";
        }

        else if (mapType == MapType.Boss)
        {
            tileName.enabled = true;
            tileInfPanel.SetActive(true);
            monInf.SetActive(true);

            Monster monster = obj as Monster;
            MonImg.sprite = monster.Sprite;
            tileName.text = "보스 몬스터";
            key.enabled = true;
            monHP.text = "<color=#D1180B>체력</color> : " + monster.Status.MaxHp;
            monPow.text = "<color=#FFD400>파워</color> : " + monster.Status.Power;
            monDefense.text = "<color=#0000FF>방어</color> : " + monster.Status.Defense;
            monDex.text = "<color=#80FF00>민첩</color> : " + monster.Status.Dex;
            monExp.text = "<color=#8A2BE2>EXP</color>  + " + monster.Status.Exp;
            monName.text = monster.Name;
        }

        else if (mapType == MapType.Dragon)
        {
            tileName.enabled = true;
            tileInfPanel.SetActive(true);
            monInf.SetActive(true);

            Monster monster = obj as Monster;
            MonImg.sprite = monster.Sprite;
            tileName.text = "드래곤";
            monHP.text = "<color=#D1180B>체력</color> : " + monster.Status.MaxHp;
            monPow.text = "<color=#FFD400>파워</color> : " + monster.Status.Power;
            monDefense.text = "<color=#0000FF>방어</color> : " + monster.Status.Defense;
            monDex.text = "<color=#80FF00>민첩</color> : " + monster.Status.Dex;
            monExp.text = "<color=#8A2BE2>EXP</color>  + " + monster.Status.Exp;
            monName.text = monster.Name;
        }

    }


    public void BloodEffect()
    {
        StartCoroutine(ShowBloodScreen());
    }

    IEnumerator ShowBloodScreen()
    {
        bloodEffect.gameObject.SetActive(true);
        bloodEffect.color = new Color(1, 0, 0, Random.Range(0.2f, 0.3f));
        yield return new WaitForSeconds(0.2f);
        bloodEffect.gameObject.SetActive(false);
        bloodEffect.color = Color.clear;
    }

    public void ActiveSomeThingBox(string text, Action endAction = null)
    {
        somethingBoxObj.SetActive(true);
        somethingText.text = text;
        SomethingAction = endAction;
    }
    
    public void B_YES()
    {
        SomethingAction?.Invoke();
        somethingBoxObj.SetActive(false);
    }
}