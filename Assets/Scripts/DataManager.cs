using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    //MapManager
    public List<Vector2Int> fieldSizeList;
    public List<Vector3Int> monsterNumberPerFloor;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;            
        }
        else
        {
            Destroy(this);
        }
    }

    [Header("Knight Status")]
    [SerializeField]
    public int StatusPoint;
    public List<int> ExpNeedForLevelUp = new()
        { 1, 5, 7, 9, 11, 14, 17, 20, 23, 27, 31, 35, 39, 43, 47, 51 };
    
    [Header("Player Skill")]
    
    public int PrincessMaxCost;
    public int KnightMaxCost;
    public int[] princessSkillCost;
    public int[] knightSkillCost;


    [Header("게임")]
    /// <summary>
    /// index 0 = 1층에서 도트 데미지를 받기 시작하는 턴 
    /// </summary>
    public List<int> WaveCountByFloor = new()
    { 10, 10, 10 };
}
    
    