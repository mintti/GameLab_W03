using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private ResourceManager _resourceManager;
    public CameraManager CameraManager { get; private set; }
    
    [Header("게임 관련")] 
    public bool GameEnd = false;

    public string whoseTurn;
    
    [Header("설정 값")] 
    public Vector2 mapSize;
    public int emptyMapProportion;
    public int battleMapProportion; 
    public int eventMapProportion;
    public int boxMapProportion;
    public int blockMapProportion;
    
    [Header("필드 관련")]
    public Transform FieldGenTransform;
    private FieldPiece[,] Fields;

    [Header("맵 관련")]
    public Transform MapGenTransform;
    private MapPiece[,] Maps;

    [Header("플레이어")]
    public Player knight;
    public Player princess;

    [Header("이벤트 관련")]
    public BattleEvent battleEvent;
    public FieldEvent fieldEvent;
    public ItemEvent itemEvent;
    public HealEvent healEvent;
    
    public void Start()
    {
        _resourceManager = GetComponentInChildren<ResourceManager>();
        CameraManager = GameObject.Find(nameof(CameraManager)).GetComponent<CameraManager>();
        Init();
    }

    /// <summary>
    /// 기본 설정 초기화
    /// </summary>
    private void Init()
    {
        CreateMap();
        InitPlayerPosition();
        StartCoroutine(nameof(PlayGame));
    }

    void CreateMap()
    {
        
    }

    void InitPlayerPosition()
    {
        
    }

    IEnumerator PlayGame()
    {
        while (true)
        {
            whoseTurn = nameof(princess);
            StartCoroutine(PlayPlayer(princess));
            
            whoseTurn = nameof(knight);
            StartCoroutine(PlayPlayer(knight));

            if (GameEnd)
            {
                // 게임이 종료되면 실행한다.
                // 왜 종료되었는 지는 각 오브젝트에서 설정해준다.
                yield break;
            }
        }
    }

    IEnumerator PlayPlayer(Player player)
    {
        do
        {
            player.StartTurn();
            CameraManager.Target = player.transform;
            yield return new WaitUntil(() => player.IsTurnEnd);
        } while (player.IsTurnEnd);
    }


    public int[] princessSkillCost;
    public int[] knightSkillCost;

    /// <summary>
    /// true가 반환 된 경우, 스킬 사용이 유효한 상태로 코스트 차감
    /// false가 반환된 경우, 스킬 사용이 실패한 경우로 코스트를 차감하지 않음
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public bool ClickMap(FieldPiece field)
    {
        bool complete = true;
        // if (field.CanSelect) // 필드에서 판단
        {
            
            if(whoseTurn.Equals(nameof(princess))) // 공주의 턴
            {
                if (princess.Cost >= princessSkillCost[princess.SelectedIdx])
                {
                    complete = princess.SelectedIdx switch
                    {
                        1 => TurnOnMapPiece(field),
                        0 or 2 => MoveKnight(field),
                        _ => false,
                    };
                    
                    if (complete) princess.Cost -= princessSkillCost[princess.SelectedIdx];
                }
                else
                {
                    Log("코스트가 부족하여 실행 할 수 없습니다.");
                    complete = false;
                }
            }
            else // 용사의 턴
            {
                if (knight.Cost >= knightSkillCost[knight.SelectedIdx])
                {
                    complete = knight.SelectedIdx switch
                    {
                        0 => TurnOnMapPiece(field, true),
                        1 => MakeHealZone(field),
                        2 => BuffKnight(),
                        _ => false,
                    };

                    if (complete) knight.Cost -= knightSkillCost[knight.SelectedIdx];
                }
                else
                {
                    Log("코스트가 부족하여 실행 할 수 없습니다.");
                    complete = false;
                }
            }
            
            if (!complete)
            {
                Log($"스킬이 실행되지 않음.");
            }
        }


        return complete;
    }

    #region Player Skill
    private bool MoveKnight(FieldPiece field)
    {
        bool result = true;

        if (field.MapType == MapType.Block)
        {
            Log("이동할 수 없는 지형입니다.");
        }
        else
        {
            // 이동
            knight.transform.SetParent(field.gameObject.transform); 
            knight.transform.position = Vector3.zero;

            switch (field.MapType)
            {
                case MapType.Ignore : break; // 이벤트가 없으면 종료
                default : 
                    ExecuteMapEvent(field);
                    field.UpdateMapType(MapType.Ignore);
                    break;
            }
        }

        // 맵을 밝힘
        TurnOnMapPiece(field);

        return result;
    }

    private bool TurnOnMapPiece(FieldPiece field, bool isKnight = false)
    {
        bool result = true;

        if (!field.IsLight)
        {
            field.IsLight = true;
            field.MapPiece.IsLight = true;
        }
        else
        {
            Log("이미 밝혀져 있는 맵은 밝힐 수 없습니다.");
            result = false;
        }

        return result;
    }

    private bool MakeHealZone(FieldPiece field)
    {
        bool result = true;

        if (field.MapType == MapType.Ignore)
        {
            field.UpdateMapType(MapType.Heal);
        }
        else
        {
            Log("빈 땅에만 힐존을 생성할 수 있습니다.");
            result = false;
        }

        return result;
    }

    private  bool BuffKnight()
    {
        knight.transform.BroadcastMessage("Buff");
        return true;
    }

    #endregion



    #region Execute Map Event
    private void ExecuteMapEvent(FieldPiece field)
    {
        switch (field.MapType)
        {
            case MapType.BattleMonster : 
                battleEvent.Init(knight.GetComponent<Knight>(), _resourceManager.Monsters[field.EventIndex]);
                battleEvent.Execute();
                break;
            case MapType.Event :
                var fevt = _resourceManager.FieldEvents[field.EventIndex];
                fieldEvent.Execute(fevt);
                break;
            case MapType.Item : 
                var ievt = _resourceManager.Items[field.EventIndex];
                itemEvent.Execute(ievt);
                break;
            case MapType.Heal :
                healEvent.Execute(
                    knight.GetComponent<Knight>(),
                    _resourceManager.healEventSprite);
                break;
        }
    }
    #endregion
    
    private void Log(string text)
    {
        Debug.Log(text);
    }
}