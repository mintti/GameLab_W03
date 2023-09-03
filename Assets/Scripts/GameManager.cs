using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer.Internal.Converters;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ResourceManager _resourceManager;
    private DataManager _dataManager;
    private UIManager _uiManager;
    public CameraManager CameraManager { get; private set; }
    public MapManager MapManager    { get; private set; }
    
    [Header("게임 관련")] 
    public bool GameEnd = false;

    public string whoseTurn;

    public int _turn;

    public int Turn
    {
        get => _turn;
        set
        {
            _turn = value;
            UIManager.Instance.UpdateTurnText(_turn);
        }
    }

    public int waveInterval;
    
    [Header("설정 값")] 
    public Vector2 mapSize;
    public int emptyMapProportion;
    public int battleMapProportion; 
    public int eventMapProportion;
    public int boxMapProportion;
    public int blockMapProportion;
    
    [Header("필드 관련")]
    public Transform FieldGenTransform;
    // private FieldPiece[,] knightFields;
    // private FieldPiece[,] princessFields;

    // [Header("맵 관련")]
    // public Transform MapGenTransform;
    // private MapPiece[,] KnightMaps;
    // private MapPiece[,] PrincessMaps;

    private int _displayFloor;
    public int DisplayFloor
    {
        get => _displayFloor;
        set
        {
            _displayFloor = value;
            if (_displayFloor != 0)
            {
                UIManager.Instance.UpdateCurrentDisplayFloor(_displayFloor);
                // [TODO] MAP 업데이트? 변경?
            }
        }
    }

    /// <summary>
    /// 현재 용사가 존재하는 층의 정보
    /// </summary>
    public int CurrentKnightFloor;
    
    
    [Header("플레이어")]
    public Player knight;
    public Player princess;
    
    public int StatusPoint
    {
        get => DataManager.Instance.StatusPoint;
        set
        {
            DataManager.Instance.StatusPoint = value;
            _uiManager.UpdateKnightStatusInfo();
        }
    }
    
    public int MaxCost;

    [Header("이벤트 관련")]
    public BattleEvent battleEvent;
    public FieldEvent fieldEvent;
    public ItemEvent itemEvent;
    public HealEvent healEvent;

    public bool EventPrinting { get; set; }
    
    [Header("웨이브 시스템")]
    private bool dotDamageTime;
    private int turnsBeforeAscend;
    
    public void Start()
    {
        _resourceManager = GetComponentInChildren<ResourceManager>();
        _dataManager = GameObject.Find(nameof(DataManager)).GetComponent<DataManager>();
        MapManager = GetComponentInChildren<MapManager>();
        CameraManager = Camera.main.GetComponent<CameraManager>();
        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();
        
        Init();
    }
    
    /// <summary>
    /// 기본 설정 초기화
    /// </summary>
    private void Init()
    {
        MapManager.InitMap();
        InitPlayerPosition();

        // 게임 정보 초기화
        Turn = 1;
        StatusPoint = 0;

        knight.SelectedFloor = CurrentKnightFloor = 1;
        princess.SelectedFloor = 3;
        
        // 시작
        StartCoroutine(nameof(PlayGame));

        // Map Test 위에줄 주석치고 밑에거 주석풀기
        // MapManager.BuildAllField(FieldType.Field);
    }


    void InitPlayerPosition()
    {
        knight.fieldType = FieldType.Knight;
        knight.transform.position = MapManager.GridToWorldPosition(new Vector2(0,0));
        knight.CurrentFieldPiece = MapManager.GetFieldPiece(0, new Vector2Int(0,0));
        MapManager.LightFieldKnightMove(knight.CurrentFieldPiece.gridPosition);
        
        princess.fieldType = FieldType.Princess;
        princess.transform.position = MapManager.GridToWorldPosition(new Vector2(19,19));
        princess.CurrentFieldPiece = MapManager.GetFieldPiece(0, new Vector2Int(19,19));
        // MapManager.LightField(FieldType.Princess, new Vector2Int(19,19));

        MapManager.RefreshMap();
    }

    IEnumerator PlayGame()
    {
        while (true)
        {
            Camera.main.backgroundColor = new Color(0.3537736f, 0.401642f, 1, 1);
            whoseTurn = nameof(knight);
            MapManager.BuildAllField(FieldType.Knight);
            yield return StartCoroutine(PlayPlayer(knight));
            if (dotDamageTime)
            {
                // [TODO] 도트 데미지 액션 출력
                knight.Status.CurrentHp -= GetDotDam();
            }

            Camera.main.backgroundColor = new Color(1, 0.6650944f, 0.9062265f, 1);
            whoseTurn = nameof(princess);
            MapManager.BuildAllField(FieldType.Princess);
            yield return StartCoroutine(PlayPlayer(princess));

            if (GameEnd)
            {
                // 게임이 종료되면 실행한다.
                // 왜 종료되었는 지는 각 오브젝트에서 설정해준다.
                yield break;
            }
            
            Turn++;
            // if (Turn % waveInterval == 0)
            // {
            //     MapManager.DoWave(.1f);
            // }
            
            // 도트 데미지 여부 설정
            if (GetDotDam() > 0)
            {
                if (!dotDamageTime)
                {
                    // [TODO] 도트 데미지가 시작된다는 안내?
                    dotDamageTime = true;
                }
            }
        }
    }

    private int GetDotDam()
    {
        return _dataManager.WaveCountByFloor[CurrentKnightFloor - 1] - (turnsBeforeAscend - Turn);   // 이전에 오르는데 사용됬던 턴수는 차감 
    } 

    public void UpFloor()
    {
        dotDamageTime = false;
        turnsBeforeAscend = Turn;
    }

    IEnumerator PlayPlayer(Player player)
    {
        do
        {
            DisplayFloor = player.SelectedFloor; // 이전 바라보고 있던 대상 층으로 이동
            ChangeBehavior(player.SelectedIdx);
            
            player.StartTurn();
            CameraManager.Target = player.transform;
            yield return new WaitUntil(() => player.IsTurnEnd);
        } while (!player.IsTurnEnd);
    }

    public void B_SelectedFloor(int floor)
    {
        DisplayFloor = floor;
        if (whoseTurn == nameof(princess))
        {
            princess.SelectedFloor = floor;
        }
        else
        {
            knight.SelectedFloor = floor;
        }
    }

    /// <summary>
    /// true가 반환 된 경우, 스킬 사용이 유효한 상태로 코스트 차감
    /// false가 반환된 경우, 스킬 사용이 실패한 경우로 코스트를 차감하지 않음
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public bool ClickMap(FieldPiece field)
    {
        bool complete = true;
        // if (field._canSelect) // 필드에서 판단
        if (MapManager.canSelectList.Contains(field)) // 필드에서 판단
        {
            if (whoseTurn.Equals(nameof(princess))) // 공주의 턴
            {
                complete = princess.SelectedIdx switch
                {
                    0 => TurnOnMapPiece(field, false),
                    1 => BuffKnight(),
                    _ => false,
                };
            }
            else // 용사의 턴
            {
                complete = knight.SelectedIdx switch
                {
                    0 => MoveKnight(field),
                    1 => Rest(),
                    _ => false,
                };
            }

            if (!complete)
            {
                //Log($"스킬이 실행되지 않음.");
            }
        }
        else
        {
            Log($"선택 가능한 영역이 아님");
            complete = false;
        }


        return complete;
    }

    #region Player Skill
    private bool MoveKnight(FieldPiece field)
    {
        bool result = true;
        int cost = field.PrincessIsLight ? 0 : _dataManager.knightSkillCost[princess.SelectedIdx]; 
      
        if (cost <= knight.Cost)
        {
            knight.Cost -= cost;
            
            // [TODO] Door 접촉 로직 하단에 작성 후 제거
            // if ((field.gridPosition.x == 19 && field.gridPosition.y == 19) ||
            //     (field.gridPosition.x == 19 && field.gridPosition.y == 18))
            // {
            //     battleEvent.Init(knight, _resourceManager.Boss);
            //     battleEvent.Execute(true);
            //     return true;
            // }

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
                    
                        // field.UpdateMapType(MapType.Empty);
                        MapManager.UpdateMapType(field, MapType.Empty);
                        break;
                }
            
                // 이동
                Debug.Log("move" +field.gridPosition );
                // knight.transform.position = MapManager.GridToWorldPosition(field.gridPosition);
                knight.transform.position = MapManager.GridToWorldPosition(new Vector2(field.gridPosition.x,field.gridPosition.y));
                knight.CurrentFieldPiece = field;
            }

            // 맵을 밝힘
            MapManager.LightFieldKnightMove(field.gridPosition);
            TurnOnMapPiece(field, true, false);

            // 이동 가능 영역 업데이트
            if(whoseTurn.Equals(nameof(princess))) ChangeBehavior(princess.SelectedIdx);
            else ChangeBehavior(knight.SelectedIdx);
        }
        else
        {
            Log("코스트가 부족하여 실행 할 수 없습니다.");
            result = false;
        }
        
        return result;
    }

    private bool TurnOnMapPiece(FieldPiece field, bool isKnight = false, bool outputLog = true)
    {
        bool result = true;
        int cost;
        if(isKnight && !field.KnightIsLight)
        {
            // cost = _dataManager.knightSkillCost[1];
            //
            // if (knight.Cost >= cost)
            // {
            //     field.KnightIsLight = true;
            //     MapManager.LightField(FieldType.Knight, field.gridPosition);
            // }
        }
        else if(!isKnight && !field.PrincessIsLight)
        {
            cost = _dataManager.princessSkillCost[0];
            
            if (princess.Cost >= cost)
            {
                field.PrincessIsLight = true;
                MapManager.LightField(FieldType.Princess, field.gridPosition);
                ChangeBehavior(princess.SelectedIdx);
                princess.Cost -= _dataManager.princessSkillCost[princess.SelectedIdx];
            }
            else
            {
                Log("코스트가 부족하여 실행 할 수 없습니다.");
                result = false;
            }

        }
        else
        {
            if(outputLog) Log("이미 밝혀져 있는 맵은 밝힐 수 없습니다.");
            result = false;
        }
        
        MapManager.RefreshMap();
        return result;
    }

    private bool Rest()
    {
        bool result = true;

        if (knight.Cost >= 1)
        {
            knight.Status.CurrentHp += knight.Cost;
            knight.Cost = 0;
        }
        else
        {
            Log("휴식에 필요한 코스트가 충분하지 않습니다.");
            result = false;
        }

        return result;
    }

    private bool MakeHealZone(FieldPiece field)
    {
        bool result = true;

        if (field.MapType == MapType.Empty)
        {
            // field.UpdateMapType(MapType.Heal);
            MapManager.UpdateMapType(field, MapType.Heal);
            MapManager.RefreshMap();
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
        bool result = true;
        var knight_ = knight;

        if (!knight_.Status.Buff)
        {
            int  cost = _dataManager.princessSkillCost[0];

            if (princess.Cost >= cost)
            {
                knight_.Status.Buff = true;
                princess.Cost -= _dataManager.princessSkillCost[princess.SelectedIdx];
            }
            else
            {
                Log("코스트가 부족하여 실행 할 수 없습니다.");
                result = false;
            }
        }
        else
        {
            Log("버프는 한 번만 사용할 수 있습니다.");
            result = false;
        }
        
        return result;
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
        // FieldPiece[,] baseFields = whoseTurn.Equals(nameof(princess)) ? MapManager.princessFields : MapManager.knightFields;
        FieldPiece[,] baseFields = MapManager.GetCurrentFloorField();
        FieldPiece     curPiece  = whoseTurn.Equals(nameof(princess)) ? princess.CurrentFieldPiece : knight.CurrentFieldPiece;
        
        if (whoseTurn.Equals(nameof(princess)))
        {
            switch (index)
            {
                case 0:
                    // 공주가 밝힌 칸에서의 8방향 값을 전달
                    foreach (var piece in baseFields)
                    {
                        if (piece.PrincessIsLight)
                        {
                            // changePiece = changePiece.Concat(GetFieldKnightSkill1(MapManager.princessFields, piece,
                            changePiece = changePiece.Concat(GetFieldKnightSkill1(baseFields, piece,
                                new[] { -1, 1, 0, 0 }, new[] { 0, 0, -1, 1 })).ToList();
                        }
                    }
                    break;
                case 1:
                    // 공주가 서 있는 위치 전달
                    changePiece.Add(princess.CurrentFieldPiece);
                    break;
                case 2:
                    // 비어있는 칸만 전달
                    foreach (var piece in baseFields)
                    {
                        if(piece.PrincessIsLight && piece.MapType == MapType.Empty) changePiece.Add(piece);
                    }
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 0:
                    // 1칸 간격의 4방향 전달
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x-1, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x+1, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y-1);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y+1);
                    break;
                case 1:
                    // 용사가 서 있는 위치 전달 
                    changePiece.Add(knight.CurrentFieldPiece);
                    break;
                case 2:
                    // 2칸 간격으로 4방향 전당
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x-2, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x+2, curPiece.gridPosition.y);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y-2);
                    AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y+2);
                    break;
                case 3 :  // 8방향 전달 
                    changePiece = GetFieldKnightSkill1(baseFields, curPiece, new []{-1, 1, 0, 0}, new[]{0, 0, -1, 1}).ToList();
                    break;
            }
        }
        
        
        // MapManger에게 changePiece 전달
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
            var piece = baseFields[x, y];
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
        EventPrinting = true;
        switch (field.MapType)
        {
            case MapType.Monster : 
                battleEvent.Init(knight, field.monsterInfo);
                battleEvent.Execute();
                break;
            case MapType.Event :
                // var fevt = _resourceManager.GetRandomFieldEvent();
                // fieldEvent.Execute(fevt);
                fieldEvent.Execute(field.fieldEventInfo);
                break;
            case MapType.Item : 
                // var ievt = _resourceManager.GetRandomItemEvent();
                // itemEvent.Execute(ievt);
                itemEvent.Execute(field.itemInfo);
                break;
            case MapType.Heal :
                healEvent.Execute(
                    knight, _resourceManager.healEventSprite);
                break;
            
        }

        MapManager.UpdateMapType(field, MapType.Empty);
    }
    #endregion
    
    private void Ending()
    {
        EventPrinting = true;
        _uiManager.ActiveEndingScene();
    }
    
    private void Log(string text)
    {
        _uiManager.OutputInfo(text);
    }

    public void B_Restart()
    {
        Destroy(_uiManager.gameObject);
        Invoke(nameof(Restart), .5f);
    }

    void Restart()
    {
        SceneManager.LoadScene("Title");
    }
}