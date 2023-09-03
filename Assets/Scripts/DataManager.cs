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
    public List<int> ExpNeedForLevelUp = new()
        { 1, 7, 10, 13, 15, 18, 23, 28, 33 };
    
    [Header("Player Skill")]
    public int[] princessSkillCost;
    public int[] knightSkillCost;
}
    
    