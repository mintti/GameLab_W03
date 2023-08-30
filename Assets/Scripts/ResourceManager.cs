using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;
public class ResourceManager :  MonoBehaviour
{
    public List<Monster> Monsters { get; private set; }
    public List<FieldEventInfo> FieldEvents { get; private set; }
    
    public List<ItemInfo> Items { get; private set; }

    public Sprite healEventSprite;

    
    public void Awake()
    {
        InitFieldEvent();
        InitMonster();
        InitItemEvent();
        
        // LoadData();
    }

    // public void LoadData()
    // {
    //     string[] data;
    //     TextAsset field = Resources.Load("FieldEvent/events") as TextAsset;
    //     data = field.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
    //
    //     FieldEvents = new();
    //     for (int i = 0, cnt = data.Length / 2; i < cnt ; i++)
    //     {
    //         string text = data[2 + (i * 2)];
    //         Sprite sprite = Resources.Load($"{data[3 * (i + 1) + 1]}").GetComponent<Sprite>();
    //     
    //         var fieldInfo = new FieldEventInfo(sprite, text); 
    //         FieldEvents.Add(fieldInfo);
    //     }
    //
    //     
    // }

    public void InitFieldEvent()
    {
        FieldEvents = new();
        FieldEvents.Add(new(GetSrc("FieldEvent","test"),"축복의 샘입니다.\n\n체력이 회복됩니다."));
    }


    public void InitMonster()
    {
        Monsters = new();
        Monsters.Add(new("슬라임", new(10, 1, 1), GetSrc("Monster","test")));
    }

    public void InitItemEvent()
    {
        Items = new();
        Items.Add(new(GetSrc("ItemEvent", "test"), "물약을 주웠다.\n\n체력이 회복된다!"));
    }

    Sprite GetSrc(string folder, string name)
    {
        return Resources.Load<Sprite>($"{folder}/{name}");
    }
}