using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; set; }

    //MapManager
    public List<Vector2Int> fieldSizeList;

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
}
