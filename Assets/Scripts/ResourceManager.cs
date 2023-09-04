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

    public Monster GetRandomMonster(int monlevel)
    {
        int index = 0;

        if (monlevel == 1)
        {
            index = Random.Range(0, 5);
        }

        return index switch
        {
            0 => new Monster("Lv.1 <color=#FF0000>슬라임</color>", new(2, 1, 0, 5), GetSrc("Monster", "slime")),
            //******************************************↑Level1↑******************************************
            1 => new Monster("Lv.2 <color=#FF0000>고블린</color>", new(3, 1, 1, 7), GetSrc("Monster", "goblin")),
            2 => new Monster("Lv.2 <color=#FF0000>슬라임</color>", new(3, 1, 0, 6), GetSrc("Monster", "slime")),
            //******************************************↑Level2↑******************************************
            3 => new Monster("Lv.3 <color=#FF0000>고블린</color>", new(4, 2, 1, 9), GetSrc("Monster", "goblin")),
            4 => new Monster("Lv.3 <color=#FF0000>슬라임</color>", new(4, 1, 1, 8), GetSrc("Monster", "slime")),
            //******************************************↑Level3↑******************************************
            5 => new Monster("Lv.4 <color=#FF0000>고블린</color>", new(4, 2, 2, 11), GetSrc("Monster", "goblin")),
            6 => new Monster("Lv.4 <color=#FF0000>슬라임</color>", new(4, 2, 1, 11), GetSrc("Monster", "slime")),
            7 => new Monster("Lv.4 <color=#FF0000>오크</color>", new(5, 3, 2, 9), GetSrc("Monster", "orc")),
            //******************************************↑Level4↑******************************************
            8 => new Monster("Lv.5 <color=#FF0000>고블린</color>", new(4, 3, 2, 11), GetSrc("Monster", "goblin")),
            9 => new Monster("Lv.5 <color=#FF0000>오크</color>", new(6, 2, 3, 9), GetSrc("Monster", "orc")),
            //******************************************↑Level5↑******************************************
            10 => new Monster("Lv.6 <color=#FF0000>고블린</color>", new(4, 3, 3, 11), GetSrc("Monster", "goblin")),
            11 => new Monster("Lv.6 <color=#FF0000>오크</color>", new(6, 3, 3, 9), GetSrc("Monster", "orc")),
            12 => new Monster("Lv.6 <color=#FF0000>스텀프</color>", new(6, 2, 4, 15), GetSrc("Monster", "stump")),
            //******************************************↑Level6↑******************************************
            13 => new Monster("Lv.7 <color=#FF0000>고블린</color>", new(4, 3, 3, 11), GetSrc("Monster", "goblin")),
            14 => new Monster("Lv.7 <color=#FF0000>오크</color>", new(6, 3, 3, 9), GetSrc("Monster", "orc")),
            15 => new Monster("Lv.7 <color=#FF0000>스텀프</color>", new(6, 2, 4, 15), GetSrc("Monster", "stump")),
            //******************************************↑Level7↑******************************************
            16 => new Monster("Lv.8 <color=#FF0000>고블린</color>", new(4, 3, 3, 11), GetSrc("Monster", "goblin")),
            17 => new Monster("Lv.8 <color=#FF0000>오크</color>", new(6, 3, 3, 9), GetSrc("Monster", "orc")),
            18 => new Monster("Lv.8 <color=#FF0000>스텀프</color>", new(6, 2, 4, 15), GetSrc("Monster", "stump")),
            19 => new Monster("Lv.8 <color=#FF0000>드레이크</color>", new(6, 1, 1, 1), GetSrc("Monster", "drake")),
            //******************************************↑Level8↑******************************************
            20 => new Monster("Lv.9 <color=#FF0000>오크</color>", new(6, 3, 3, 9), GetSrc("Monster", "orc")),
            21 => new Monster("Lv.9 <color=#FF0000>스텀프</color>", new(6, 2, 4, 15), GetSrc("Monster", "stump")),
            22 => new Monster("Lv.9 <color=#FF0000>드레이크</color>", new(6, 1, 1, 1), GetSrc("Monster", "drake")),
            //******************************************↑Level9↑******************************************
            23 => new Monster("Lv.9 <color=#FF0000>스텀프</color>", new(6, 2, 4, 15), GetSrc("Monster", "stump")),
            24 => new Monster("Lv.9 <color=#FF0000>드레이크</color>", new(6, 1, 1, 1), GetSrc("Monster", "drake")),
            //******************************************↑Level10↑******************************************
            25 => new Monster("Lv.9 <color=#FF0000>스텀프</color>", new(6, 2, 4, 15), GetSrc("Monster", "stump")),
            26 => new Monster("Lv.9 <color=#FF0000>드레이크</color>", new(6, 1, 1, 1), GetSrc("Monster", "drake")),
            //******************************************↑Level11↑******************************************



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