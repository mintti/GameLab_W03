using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private ResourceManager _resourceManager;
    public CameraManager CameraManager { get; private set; }
    public MapManager MapManager    { get; private set; }
    
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
    private FieldPiece[,] knightFields;
    private FieldPiece[,] princessFields;

    // [Header("맵 관련")]
    // public Transform MapGenTransform;
    // private MapPiece[,] KnightMaps;
    // private MapPiece[,] PrincessMaps;

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
        MapManager = GetComponentInChildren<MapManager>();
        CameraManager = Camera.main.GetComponent<CameraManager>();

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
        MapManager.CreateMap();
    }

    void InitPlayerPosition()
    {
        knight.transform.position = MapManager.GridToWorldPosition(new Vector2(0,0));
        MapManager.LightField(FieldType.Knight, new Vector2(0,0));
        princess.transform.position = MapManager.GridToWorldPosition(new Vector2(19,19));
        MapManager.LightField(FieldType.Princess, new Vector2(19,19));
    }

    IEnumerator PlayGame()
    {
        while (true)
        {
            whoseTurn = nameof(princess);
            yield return StartCoroutine(PlayPlayer(princess));

            whoseTurn = nameof(knight);
            yield return StartCoroutine(PlayPlayer(knight));

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
        } while (!player.IsTurnEnd);
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
            switch (field.MapType)
            {
                case MapType.Empty : break; // 이벤트가 없으면 종료
                default : 
                    ExecuteMapEvent(field);
                    field.UpdateMapType(MapType.Empty);
                    break;
            }
            
            // 이동'
            knight.transform.position = MapManager.GridToWorldPosition(field.gridPosition);
            knight.CurrentFieldPiece = field;
        }

        // 맵을 밝힘
        TurnOnMapPiece(field, true);

        return result;
    }

    private bool TurnOnMapPiece(FieldPiece field, bool isKnight = false)
    {
        bool result = true;

        if (!field.IsLight)
        {
            field.IsLight = true;
            if(isKnight) 
                MapManager.LightField(FieldType.Knight, field.gridPosition);
            else
                MapManager.LightField(FieldType.Princess, field.gridPosition);
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

        if (field.MapType == MapType.Empty)
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



    #region Map-Related
    
    /// <summary>
    /// 행동이 변경될 때, 행동 사용 가능 지역을 표시
    /// </summary>
    /// <param name="index"></param>
    public void ChangeBehavior(int index)
    {
        List<FieldPiece> changePiece = new();
        FieldPiece[,] baseFields = whoseTurn.Equals(nameof(princess)) ? MapManager.princessFields : MapManager.knightFields;
        FieldPiece     curPiece  = whoseTurn.Equals(nameof(princess)) ? princess.CurrentFieldPiece : knight.CurrentFieldPiece;
        
        if (whoseTurn.Equals(nameof(princess)))
        {
            switch (index)
            {
                case 0:
                    foreach (var piece in MapManager.princessFields)
                    {
                        if (piece.IsLight)
                        {
                            changePiece = changePiece.Concat(GetFieldKnightSkill1(MapManager.princessFields, curPiece,
                                new[] { -1, 1, 0, 0 }, new[] { 0, 0, -1, 1 })).ToList();
                        }
                    }
                    
                    break;
                case 1:
                    foreach (var piece in MapManager.princessFields)
                    {
                        if(piece.IsLight) changePiece.Add(piece);
                    }
                    break;
                case 2:
                    changePiece.Add(princess.CurrentFieldPiece);
                    break;
                    return;
            }
        }
        else
        {
            switch (index)
            {
                case 0:
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x-1, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x+1, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y-1);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y+1);
                    break;
                case 1:
                    changePiece = GetFieldKnightSkill1(MapManager.knightFields, curPiece, new []{-1, 1, 0, 0}, new[]{0, 0, -1, 1}).ToList();
                    break;
                case 2:
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x-2, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x+2, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y-2);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y+2);
                    break;
            }
        }
        
        
        // [TODO] MapManger에게 changePiece 전달
        MapManager.showCanSelectField(changePiece);
    }

    #region Map Area


    /// <summary>
    /// way X, way Y 조합에 해당하는 방향내 Field Piece 리스트를 반환
    /// </summary>
    /// <param name="baseFields"></param>
    /// <param name="CurPiece"></param>
    /// <param name="wayX"></param>
    /// <param name="wayY"></param>
    /// <returns></returns>
    IEnumerable<FieldPiece> GetFieldKnightSkill1(FieldPiece[,] baseFields, FieldPiece CurPiece, int[] wayX, int[] wayY)
    {
        List<FieldPiece> list = new();

        for (int xIdx = 0; xIdx < wayX.Length; xIdx++)
        {
            for (int yIdx = 0; yIdx < wayY.Length; yIdx++)
            {
                (int x, int y) pivot = (CurPiece.gridPosition.x + wayX[xIdx], CurPiece.gridPosition.y + wayY[yIdx]);

                AddPieceInList(list, baseFields, pivot.x, pivot.y);
            }
        }
        
        return list;
    }

    void AddPieceInList(List<FieldPiece> list, FieldPiece[,] baseFields, int x, int y)
    {
        if (!(x < 0 || x >= baseFields.GetLength(0) || y < 0 || y >= baseFields.GetLength(1)))
        {
            var piece = baseFields[x,y];
            if(!list.Contains(piece)) list.Add(piece);
        }
    }
    #endregion
    
    /// <summary>
    /// 맵 이동 후 이벤트가 존재하면 수행 
    /// </summary>
    /// <param name="field"></param>
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