using System.Collections;
using UnityEngine;

public class BattleEvent : MonoBehaviour
{
    private UIManager _uiManager;
    private Knight _knight;
    private Monster _monster;

    public void Init(Knight knight, Monster monster)
    {
        _knight = knight;
        _monster = monster;
    }

    public void Execute()
    {
        CreatTemp();

        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();

        _uiManager.CombatActive(_monster.Sprite);
        StartCoroutine(Battle());
    }
    //test
    private void CreatTemp()
    {
        _knight = new Knight()
        {
            Status = new Status()
            {
                MaxHp = 15,
                CurrentHp = 10,
                Power = 1
            }
        };

        _monster = new Monster()
        {
            Name = "<color=#FF0000>고블린</color>",
            Status = new Status()
            {
                MaxHp = 5,
                Power = 3
            }
        };
    }

    IEnumerator Battle()
    {
        bool knightTurn = true;
        bool monsterTurn = false;
        int currentPlayerHP = _knight.Status.CurrentHp;
        int currentMonsterHP = _monster.Status.MaxHp;

        while (currentPlayerHP > 0 && currentMonsterHP > 0)
        {
            if (knightTurn)
            {
                currentMonsterHP -= _knight.Status.Power;
                _uiManager.OutputCombatText("<color=#33FF33>용사</color>", _monster.Name, _knight.Status.Power, currentMonsterHP);
                knightTurn = false;
                monsterTurn = true;
            }
            else if (monsterTurn)
            {
                currentPlayerHP -= _monster.Status.Power;
                _uiManager.OutputCombatText(_monster.Name, "<color=#33FF33>용사</color>", _monster.Status.Power, currentPlayerHP);
                monsterTurn = false;
                knightTurn = true;
            }

            yield return new WaitForSeconds(0.5f);
        }

        if (currentPlayerHP <= 0)
        {
            _uiManager.CombatMonsterWinText();
            yield return new WaitForSeconds(2f);
            _uiManager.gameOverButton.SetActive(true);
        }
        else if (currentMonsterHP <= 0)
        {
            _uiManager.CombatPlayerWinText(_monster.Name);
            _knight.Status.CurrentHp = currentPlayerHP;
            _uiManager.combatPanelExitButton.SetActive(true);
        }
    }

}