using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    public Vector3 CurrentPosition { get; set; }
    public GameObject playerUI;
    private GameObject playerSkillUI;

    public FieldPiece CurrentFieldPiece { get; set; }
    
    private int _cost;
    public int Cost
    {
        get => _cost;
        set
        {
            _cost = value;
        }
    }



    public bool IsTurnEnd;

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
                _uiManager.FocusSkill(playerSkillUI, _selectedIdx);
                _gameManager.ChangeBehavior(_selectedIdx);
            }
        }
    }

    public void Start()
    {
        _selectedIdx = 0;
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>(); 
        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();
        playerSkillUI = playerUI.transform.GetChild(0).gameObject;
    }

    public void StartTurn()
    {
        IsTurnEnd = false;
        playerUI.SetActive(true);
    }

    public void EndTurn()
    {
        IsTurnEnd = true;
        playerUI.SetActive(false);
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
            if (SelectedIdx > 1) return; // 임시 스킬은 3개만 
            SelectedIdx ++;
        }  
    }
    #endregion
}