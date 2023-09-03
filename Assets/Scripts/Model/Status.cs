using UnityEngine;

public class Status
{
    public int Level { get; set; } = 1;
    
    private bool player;
    private int maxHp;
    private int currentHp;
    private int power;
    private int defense;
    private int dex;
    private int exp = 0;

    private bool _buff;

    public bool Buff
    {
        get => _buff;
        set
        {
            _buff = value;
            if (player) UIManager.Instance.UpdateKnightStatusInfo();
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
            if (player) UIManager.Instance.UpdateKnightStatusInfo();
        }
    }

    public int CurrentHp
    {
        get { return currentHp; }
        set
        {
            if (player && value < currentHp)
            {
                UIManager.Instance.BloodEffect();
            }
            // Ensure CurrentHp is between 0 and MaxHp
            currentHp = Mathf.Clamp(value, 0, maxHp);
            if (player)
            {
                UIManager.Instance.UpdateKnightStatusInfo();
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
            if (player) UIManager.Instance.UpdateKnightStatusInfo();
        }
    }

    public int Defense
    {
        get { return defense; }
        set 
        { 
            defense = Mathf.Max(value, 0);
            if (player) UIManager.Instance.UpdateKnightStatusInfo();
        }
    }

    public int Dex
    {
        get => dex;
        set
        {
            dex = Mathf.Max(value, 0);
            if (player) UIManager.Instance.UpdateKnightStatusInfo();
        }
    }

    public int Exp
    {
        get
        {
            if (player) return exp;
            else return Level + 1;
        }
        set
        {
            exp = Mathf.Max(value, 0);
        }
    }

    public Status()
    {

    }

    public Status(int maxHp, int power, int defnese, int dex, bool isPlayer = false)
    {
        player = isPlayer;

        MaxHp = maxHp;

        currentHp = maxHp;
        Power = power;
        Defense = defense;
        Dex = dex;

    }
}
