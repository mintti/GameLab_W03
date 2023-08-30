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

    public void StartTurn()
    {
        IsTurnEnd = false;
    }

    public void EndTurn()
    {
        IsTurnEnd = true;
    }
}