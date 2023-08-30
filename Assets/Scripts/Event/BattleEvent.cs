using System.Collections;
using UnityEngine;

public class BattleEvent : MonoBehaviour
{
    private Player _player;
    private Monster _monster;
    
    public void Init(Player player, Monster monster)
    {
        _player = player;
        _monster = monster;
    }

    public void Execute()
    {
        StartCoroutine(Battle());
    }

    IEnumerator Battle()
    {
        while (true)
        {
            
        }
    }
}