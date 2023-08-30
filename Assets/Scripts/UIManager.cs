using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
    }

    public void B_Map()
    {
        
    }
}