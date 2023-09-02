using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; set; }

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

    public List<int> ExpNeedForLevelUp = new()
        { 1, 7, 10, 13, 15, 18, 23, 28, 33 };
    
}
