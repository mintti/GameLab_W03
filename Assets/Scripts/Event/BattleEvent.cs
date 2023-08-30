using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleEvent : MonoBehaviour
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    private Knight _knight;
    private Monster _monster;

    public Image img;

    public void Init(Knight knight, Monster monster)
    {
        _gameManager ??= GameObject.Find(nameof(GameManager)).GetComponent<GameManager>(); 
        _knight = knight;
        _monster = monster;
        img.sprite = _monster.Sprite;
    }

    public void Execute()
    {
        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();

        _uiManager.CombatActive();
        StartCoroutine(Battle());
    }
    
    IEnumerator Battle()
    {
        bool knightTurn = true;
        bool monsterTurn = false;
        ;

        while (_knight.Status.CurrentHp > 0 &&  _monster.Status.MaxHp > 0)
        {
            if (knightTurn)
            {
                _monster.Status.MaxHp -= _knight.Status.Power;
                _uiManager.OutputCombatText("<color=#33FF33>용사</color>", _monster.Name, _knight.Status.Power,  _monster.Status.MaxHp);
                
                knightTurn = false;
                monsterTurn = true;
                
                if ( _monster.Status.MaxHp <= 0)
                {
                    _uiManager.CombatPlayerWinText(_monster.Name);
                    _knight.Status.CurrentHp = _knight.Status.CurrentHp;
                    _uiManager.combatPanelExitButton.SetActive(true);
                    _gameManager.EventPrinting = false;
                    yield break;
                }
            }
            else if (monsterTurn)
            {
                _knight.Status.CurrentHp -= _monster.Status.Power;
                _uiManager.OutputCombatText(_monster.Name, "<color=#33FF33>용사</color>", _monster.Status.Power, _knight.Status.CurrentHp);
                
                monsterTurn = false;
                knightTurn = true;
                
                if (_knight.Status.CurrentHp <= 0)
                {
                    _uiManager.CombatMonsterWinText();
                    yield return new WaitForSeconds(2f);
                    _uiManager.gameOverButton.SetActive(true);
                    _gameManager.EventPrinting = false;
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

}