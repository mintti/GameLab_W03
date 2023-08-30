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
        FieldEvents.Add(new(GetSrc("FieldEvent","blessinglake"),"축복의 샘입니다.\n\n체력이 회복됩니다."));
        FieldEvents.Add(new(GetSrc("FieldEvent", "goblinevent"), "수풀을 헤치며 지나가고 있었습니다. 갑자기 주위에서 고블린 덮칩니다. 힘겹게 급습을 막아냈으나, 너무나 지칩니다. \n\n체력이 줄어듭니다."));
        FieldEvents.Add(new(GetSrc("FieldEvent", "fruitevent"), "수풀을 헤치며 지나가고 있었습니다. 기다렸다는 듯, 탐스러워 보이는 열매를 찾았습니다. 아직 신은 나를 버리지 않았나 봅니다. \n\n체력이 회복됩니다."));
        FieldEvents.Add(new(GetSrc("FieldEvent", "boyevent"), "늑대들에게 둘러싸여 있는 한 소년을 발견하고, 검을 뽑고 달려가 늑대들을 물리쳤습니다. 소년은 감사하다는 인사를 하며, 자기도 꼭 커서 용사가 될 것이라 다짐합니다. 흐뭇한 표정을 지으며 갈 길을 이어서 갑니다. \n\n공격력이 올라갑니다."));

    }


    public void InitMonster()
    {
        Monsters = new();
        Monsters.Add(new("슬라임", new(2, 1), GetSrc("Monster","slime")));
        Monsters.Add(new("고블린", new(2, 2), GetSrc("Monster", "goblin")));
        Monsters.Add(new("오크", new(5, 2), GetSrc("Monster", "orc")));
        Monsters.Add(new("드레이크", new(6, 1), GetSrc("Monster", "drake")));
        Monsters.Add(new("스텀프", new(3, 1), GetSrc("Monster", "stump")));

    }

    public void InitItemEvent()
    {
        Items = new();
        Items.Add(new(GetSrc("ItemEvent", "fruit"), "열매를 주웠다.\n\n체력이 회복된다!"));
    }

    Sprite GetSrc(string folder, string name)
    {
        return Resources.Load<Sprite>($"{folder}/{name}");
    }
}