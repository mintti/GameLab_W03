using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{
    public List<FieldEventInfo> FieldEvents { get; private set; }

    public List<ItemInfo> Items { get; private set; }

    public Sprite healEventSprite;


    public List<int> ExpNeedForLevelUp = new()
        { 1, 7, 10, 13, 15, 18, 23, 28, 33 };
    
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

    void InitFieldEvent()
    {
        FieldEvents = new();
        FieldEvents.Add(new(EventType.HP, 2, GetSrc("FieldEvent", "blessinglake"), "축복의 샘입니다.\n\n체력이 2 회복됩니다."));
        FieldEvents.Add(new(EventType.HP, -2, GetSrc("FieldEvent", "goblinevent"), "수풀을 헤치며 지나가고 있었습니다. 갑자기 주위에서 고블린 덮칩니다./힘겹게 급습을 막아냈으나, 너무나 지칩니다.\n\n체력이 -2 줄어듭니다."));
        FieldEvents.Add(new(EventType.HP, 3, GetSrc("FieldEvent", "fruitevent"), "수풀을 헤치며 지나가고 있었습니다. 기다렸다는 듯, 탐스러워 보이는 열매를 찾았습니다./아직 신은 나를 버리지 않았나 봅니다. \n\n체력이 3 회복됩니다."));
        FieldEvents.Add(new(EventType.Power, 1, GetSrc("FieldEvent", "boyevent"), "늑대들에게 둘러싸여 있는 한 소년을 발견하고, 검을 뽑고 달려가 늑대들을 물리쳤습니다./소년은 감사하다는 인사를 하며, 자기도 꼭 커서 용사가 될 것이라 다짐합니다. 흐뭇한 표정을 지으며 갈 길을 이어서 갑니다. \n\n파워가 1 올라갑니다."));

    }


    public Monster Boss;
    void InitMonster()
    {
        Boss = new Monster("드래곤", new(15, 2, 1, 1), GetSrc("Monster", "dragon"));
    }

    void InitItemEvent()
    {
        Items = new();
        Items.Add(new(EventType.HP, 3, GetSrc("ItemEvent", "fruit"), "열매를 주웠다.\n\n체력이 3 회복됩니다.!"));
    }

    Sprite GetSrc(string folder, string name)
    {
        return Resources.Load<Sprite>($"{folder}/{name}");
    }

    public Monster GetRandomMonster()
    {
        int index = Random.Range(0, 5);

        return index switch
        {
            0 => new Monster("<color=#FF0000>슬라임</color>", new(3, 1, 1, 1), GetSrc("Monster", "slime")),
            1 => new Monster("<color=#FF0000>고블린</color>", new(3, 2, 1, 1), GetSrc("Monster", "goblin")),
            2 => new Monster("<color=#FF0000>오크</color>", new(5, 2, 1, 1), GetSrc("Monster", "orc")),
            3 => new Monster("<color=#FF0000>드레이크</color>", new(6, 1, 1, 1), GetSrc("Monster", "drake")),
            4 => new Monster("<color=#FF0000>스텀프</color>", new(3, 1, 1, 1), GetSrc("Monster", "stump")),
            _ => null,
        };
    }

    public FieldEventInfo GetRandomFieldEvent()
    {
        int index = Random.Range(0, FieldEvents.Count);
        return FieldEvents[index];
    }

    public ItemInfo GetRandomItemEvent()
    {
        int index = Random.Range(0, Items.Count);
        return Items[index];
    }
}


public enum EventType
{
    HP,
    Power,

}