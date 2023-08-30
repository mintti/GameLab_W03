using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager :  MonoBehaviour
{
    public List<Monster> Monsters { get; private set; }
    public List<FieldEventInfo> FieldEvents { get; private set; }
    
    public List<ItemInfo> Items { get; private set; }

    public Sprite healEventSprite;

    public void Awake()
    {
        
    }

    public void LoadData()
    {
        
    }
}