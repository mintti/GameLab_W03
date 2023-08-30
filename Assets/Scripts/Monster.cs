using UnityEngine;

public class Monster
{
    public string Name { get; set; }
    public Status Status { get; set; }
    
    public Sprite Sprite;

    public Monster(){}
    public Monster(string name, Status status, Sprite sprite)
    {
        Name = name;
        Status = status;
        Sprite = sprite;
    }
}