using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private GameManager _gameManager;
    private MapManager _mapManager;
    private UIManager _uiManager;

    [Header("오브젝트 및 스킬 정보")]
    public GameObject playerUI;
    private GameObject playerSkillUI;
    public FieldPiece CurrentFieldPiece { get; set; }
    public FieldType fieldType;

    private int _selectedIdx;
    public int SelectedIdx
    {
        get => _selectedIdx;
        set
        {
            if (_selectedIdx != value)
            {
                _selectedIdx = value;
                _selectedIdx = Mathf.Min(_selectedIdx, 3);
                _gameManager.ChangeBehavior(_selectedIdx);
                _uiManager.UpdateInfoText(_selectedIdx);
                _uiManager.FocusSkill(playerSkillUI, _selectedIdx);
            }
        }
    }

    [Header("스텟 및 코스트 정보")]
    public int defaultHp;
    public int defaultPower;
    public int defaultDefense;
    public int defaultDex;

    private int _cost;
    public int Cost
    {
        get => _cost;
        set
        {
            _cost = value;
            _uiManager.UpdateCostText(_cost);
        }
    }

    public bool IsTurnEnd;

    public Status Status { get; set; }



    public void Start()
    {
        SelectedIdx = 0;
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
        _mapManager = GameObject.Find(nameof(MapManager)).GetComponent<MapManager>();
        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();
        playerSkillUI = playerUI.transform.GetChild(0).gameObject;

        Status = new Status(defaultHp, defaultPower, defaultDefense, defaultDex, true);

        _uiManager.UpdateKnightStatusInfo();

    }

    public void StartTurn()
    {
        Cost = _gameManager.MaxCost;
        IsTurnEnd = false;
        playerUI.SetActive(true);
        _uiManager.UpdateInfoText(_selectedIdx);
    }

    public void EndTurn()
    {
        IsTurnEnd = true;
        playerUI.SetActive(false);
        if (fieldType == FieldType.Princess)
            _mapManager.currentField = FieldType.Knight;
        else
            _mapManager.currentField = FieldType.Princess;
    }

    public void Update()
    {
        if (!IsTurnEnd)
        {
            CheckScroll();
        }
    }

    #region Check Scroll
    void CheckScroll()
    {
        Vector2 wheelInput2 = Input.mouseScrollDelta;
        if (wheelInput2.y > 0) // 휠을 밀어 돌렸을 때의 처리 ↑
        {
            if (_selectedIdx <= 0) return;
            SelectedIdx--;
        }
        else if (wheelInput2.y < 0) // 휠을 당겨 올렸을 때의 처리 ↓
        {
            if (SelectedIdx > 0) return; // 임시 스킬은 3개만 
            SelectedIdx++;
        }
    }
    #endregion
}