using UnityEngine;

public class Status
{
    private int maxHp;
    private int currentHp;
    private int power;
    private int defense;
    public int MaxHp
    {
        get { return maxHp; }
        set
        {
            maxHp = Mathf.Max(value, 0);
            // Ensure CurrentHp doesn't exceed MaxHp
            currentHp = Mathf.Min(currentHp, maxHp);
            UIManager.Instance?.UpdateKnightStatusInfo(this);
        }
    }

    public int CurrentHp
    {
        get { return currentHp; }
        set
        {
            // Ensure CurrentHp is between 0 and MaxHp
            currentHp = Mathf.Clamp(value, 0, maxHp);
            UIManager.Instance?.UpdateKnightStatusInfo(this);
        }
    }

    public int Power
    {
        get { return power; }
        set
        {
            power = Mathf.Max(value, 0);
            UIManager.Instance?.UpdateKnightStatusInfo(this);
        }
    }

    public int Defense
    {
        get { return defense; }
        set { defense = Mathf.Max(value, 0); }
    }

    public Status()
    {
        
    }
    
    public Status(int maxHp, int power)
    {
        MaxHp = maxHp;
        currentHp = maxHp;
        Power = power;
    }
}
