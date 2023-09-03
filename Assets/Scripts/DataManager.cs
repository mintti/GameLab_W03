using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

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
        { 1, 7, 10, 13, 15, 18, 23, 28, 33 };
    
    [Header("Player Skill")]
    public int[] princessSkillCost;
    public int[] knightSkillCost;


    [Header("게임")]
    /// <summary>
    /// index 0 = 1층에서 도트 데미지를 받기 시작하는 턴 
    /// </summary>
    public List<int> WaveCountByFloor = new()
    { 10, 10, 10 };
}
    
    