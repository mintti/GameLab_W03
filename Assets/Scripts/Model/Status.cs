using UnityEngine;

public class Status
{
    private bool player;
    private int maxHp;
    private int currentHp;
    private int power;
    private int defense;
    private int dex;
    private int exp;

    private bool _buff;

    public bool Buff
    {
        get => _buff;
        set
        {
            _buff = value;
            if(player) UIManager.Instance.UpdateKnightStatusInfo(this);
        }
    }
    public int MaxHp
    {
        get { return maxHp; }
        set
        {
            maxHp = Mathf.Max(value, 0);
            // Ensure CurrentHp doesn't exceed MaxHp
            currentHp = Mathf.Min(currentHp, maxHp);
            if(player) UIManager.Instance.UpdateKnightStatusInfo(this);
        }
    }

    public int CurrentHp
    {
        get { return currentHp; }
        set
        {
            // Ensure CurrentHp is between 0 and MaxHp
            currentHp = Mathf.Clamp(value, 0, maxHp);
            if (player)
            {
                UIManager.Instance.UpdateKnightStatusInfo(this);
                if (currentHp == 0)
                {
                    UIManager.Instance.ActiveGameOverObj();
                }
            }
        }
    }

    public int Power
    {
        get
        {
            return power + (Buff ? 2 : 0);
        }
        set
        {
            power = Mathf.Max(value, 0);
            if (player) UIManager.Instance.UpdateKnightStatusInfo(this);
        }
    }

    public int Defense
    {
        get { return defense; }
        set { defense = Mathf.Max(value, 0); }
    }

    public int Dex
    {
        get => dex;
        set
        {
            dex =  Mathf.Max(value, 0);
        }
    }

    public int Exp
    {
        get => exp;
        set
        {
            exp = Mathf.Max(value, 0);
        }
    }

    public Status()
    {
        
    }
    
    public Status(int maxHp, int power, bool isPlayer= false)
    {
        player = isPlayer;
        
        MaxHp = maxHp;
        currentHp = maxHp;
        Power = power;

    }
}
