using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 CurrentPosition { get; set; }
    
    private int _cost;
    public int Cost
    {
        get => _cost;
        set
        {
            _cost = value;
        }
    }

    public bool IsTurnEnd = false;

    private int _selectedIdx;

    public int SelectedIdx
    {
        get => _selectedIdx;
        set
        {
            _selectedIdx = value;
            //UIManager.FocusItem(_selectedIdx);
        }
    }

    public void StartTurn()
    {
        IsTurnEnd = false;
    }

    public void EndTurn()
    {
        IsTurnEnd = true;
    }

    public void Update()
    {
        CheckScroll();
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
            if (SelectedIdx >= 4) return; // 임시 스킬은 4개만 
            SelectedIdx ++;
        }  
    }
    #endregion
}