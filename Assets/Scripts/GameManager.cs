using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ResourceManager _resourceManager;
    private DataManager _dataManager;
    private UIManager _uiManager;
    public CameraManager CameraManager { get; private set; }
    public MapManager MapManager    { get; private set; }
    
    public bool HasKey { get; set; }
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
                MapManager.ChangeFloor(_displayFloor);
                
                // 현재 표시된 층에 따라 Player 오브젝트 표시 
                knight.SetSpriteRenderer(_displayFloor == CurrentKnightFloor);
                princess.SetSpriteRenderer(_displayFloor == 3);

                // 층에 따라 업데이트
                if (whoseTurn.Equals(nameof(princess)))
                {
                    ChangeBehavior(princess.SelectedIdx);
                }
                else
                {
                    ChangeBehavior(knight.SelectedIdx);
                }
                
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
    
    public int PrincessMaxCost;
    public int KnightMaxCost;

    [Header("이벤트 관련")]
    public BattleEvent battleEvent;
    public FieldEvent fieldEvent;
    public ItemEvent itemEvent;
    public HealEvent healEvent;

    public bool EventPrinting { get; set; }

    [Header("웨이브 시스템")] private bool _dotDamageTime;

    private bool DotDamageTime
    {
        get => _dotDamageTime;
        set
        {
            _dotDamageTime = value;
            _uiManager.BurningObj.SetActive(_dotDamageTime);
        }
    }
    
    
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

        PrincessMaxCost = _dataManager.PrincessMaxCost;
        KnightMaxCost = _dataManager.KnightMaxCost;
        knight.SelectedFloor = CurrentKnightFloor = 1;
        princess.SelectedFloor = 3;
        MapManager.ChangeFloor(CurrentKnightFloor);
        InitPlayerPosition();

        // 게임 정보 초기화
        Turn = 1;
        StatusPoint = 0;
        
        // 시작
        StartCoroutine(nameof(PlayGame));

        // Map Test 위에줄 주석치고 밑에거 주석풀기
        // MapManager.BuildAllField(FieldType.Field);
    }


    void InitPlayerPosition()
    {
        knight.transform.position = MapManager.GridToWorldPosition(new Vector2(0,0));
        knight.CurrentFieldPiece = MapManager.GetFieldPiece(knight.SelectedFloor, new Vector2Int(0,0));
        MapManager.LightTempKnightMove(knight.CurrentFieldPiece.gridPosition);
        
        MapManager.ChangeFloor(princess.SelectedFloor);
        princess.transform.position = MapManager.GridToWorldPosition(new Vector2(19,19));
        princess.CurrentFieldPiece = MapManager.GetFieldPiece(princess.SelectedFloor, new Vector2Int(19,19));
        // MapManager.LightField(FieldType.Princess, new Vector2Int(19,19));

        MapManager.ChangeFloor(CurrentKnightFloor);
    }

    IEnumerator PlayGame()
    {
        while (true)
        {
            Camera.main.backgroundColor = new Color(0.3537736f, 0.401642f, 1, 1);
            whoseTurn = nameof(knight);
            MapManager.BuildAllField();
            yield return StartCoroutine(PlayPlayer(knight));
            if (DotDamageTime)
            {
                // [TODO] 도트 데미지 액션 출력
                knight.Status.CurrentHp -= GetDotDam();
            }

            Camera.main.backgroundColor = new Color(1, 0.6650944f, 0.9062265f, 1);
            whoseTurn = nameof(princess);
            MapManager.BuildAllField();
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
                if (!DotDamageTime)
                {
                    Log($"<color=#D73502>{CurrentKnightFloor}층이 불타기 시작합니다.</color>");
                    DotDamageTime = true;
                }
            }
        }
    }

    public int GetDotDam()
    {
        var usedTurnThisFloor = Turn - turnsBeforeAscend;
        return -(_dataManager.WaveCountByFloor[CurrentKnightFloor - 1] - usedTurnThisFloor);   // 이전에 오르는데 사용됬던 턴수는 차감 
    }

    public bool CheckDotDam() => GetDotDam() > 0;

    IEnumerator PlayPlayer(Player player)
    {
        do
        {
            if(player == knight)
                player.StartTurn(KnightMaxCost);
            else
                player.StartTurn(PrincessMaxCost);
            
            DisplayFloor = player.SelectedFloor; // 이전 바라보고 있던 대상 층으로 이동
            ChangeBehavior(player.SelectedIdx);
            
            
            CameraManager.Target = player.transform;
            yield return new WaitUntil(() => player.IsTurnEnd);
        } while (!player.IsTurnEnd);
    }

    public void B_SelectedFloor(int floor)
    {
        if (whoseTurn == nameof(princess))
        {
            DisplayFloor = floor;
            princess.SelectedFloor = floor;
        }
        else
        {
            // [TODO] 층 변경 불가하다는 텍스트 출력
            // knight.SelectedFloor = floor;
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
                    //1 => BuffKnight(),
                    _ => false,
                };
            }
            else // 용사의 턴
            {
                complete = knight.SelectedIdx switch
                {
                    0 => MoveKnight(field),
                    //1 => Rest(),
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
        int cost = field.IsLight ? 0 : _dataManager.knightSkillCost[knight.SelectedIdx]; 
      
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
                    
                        if(field.MapType != MapType.Door)
                            MapManager.UpdateMapType(field, MapType.Empty);
                        break;
                }
            
                // 이동
                Debug.Log("move" +field.gridPosition );
                knight.transform.position = MapManager.GridToWorldPosition(new Vector2(field.gridPosition.x,field.gridPosition.y));
                knight.CurrentFieldPiece = field;
                // 맵을 밝힘
                MapManager.LightTempKnightMove(field.gridPosition);
            }

            // TurnOnMapPiece(field, true, false);

            // 이동 가능 영역 업데이트
            ChangeBehavior(knight.SelectedIdx);
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
        int cost = _dataManager.princessSkillCost[0];
            
        if (!isKnight && princess.Cost >= cost)
        {
            if (MapManager.CheckCanUsedLightSkill(field))
            {
                // field.IsLight = true;
	            //MapManager.LightField(FieldType.Princess, field.gridPosition);
	            MapManager.LightFieldPrincess(field.gridPosition);
	            MapManager.RefreshMap();
	            ChangeBehavior(princess.SelectedIdx);
	            princess.Cost -= _dataManager.princessSkillCost[princess.SelectedIdx];
            }
            else
            {
                Log("스킬 범위 내 영역이 전부 밝혀져있습니다.");
                result = false;
            }
        }
        else
        {
            Log("코스트가 부족하여 실행 할 수 없습니다.");
            result = false;
        }
        
        return result;
    }

    private void Rest()
    {
        bool result = true;

        if (knight.Cost >= 1)
        {
            knight.Status.CurrentHp += knight.Cost + _dataManager.KnightRestRecoveryHpAddValue;
            knight.Cost = 0;
        }
        else
        {
            Log("휴식에 필요한 코스트가 충분하지 않습니다.");
            result = false;
        }

        //return result;
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

    private void BuffKnight()
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
        
        //return result;
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
        
        MapManager.LightCellMode = false;
        if (whoseTurn.Equals(nameof(princess)))
        {
            switch (index)
            {
                case 0:
                    MapManager.LightCellMode = true;
                    // 공주가 밝히지 않은 칸 전달
                    // foreach (var piece in baseFields)
                    // {
                    //     if (!piece.IsLight)
                    //     {
                    //         changePiece = changePiece.Concat(GetFieldKnightSkill1(baseFields, piece,
                    //             new[] { -1, 1, 0, 0 }, new[] { 0, 0, -1, 1 })).ToList();
                    //     }
                    // }
                    break;
                case 1:
                    // 공주가 서 있는 위치 전달
                    // changePiece.Add(princess.CurrentFieldPiece);
                    _uiManager.ActiveSomeThingBox("용사를 강화하시겠습니까?", BuffKnight);
                    break;
                case 2:
                    // 비어있는 칸만 전달
                    foreach (var piece in baseFields)
                    {
                        if(piece.IsLight && piece.MapType == MapType.Empty) changePiece.Add(piece);
                    }
                    break;
                case 3:
                    // 공주가 밝힌 칸에서의 8방향 값을 전달
                    foreach (var piece in baseFields)
                    {
                        if (piece.IsLight)
                        {
                            // changePiece = changePiece.Concat(GetFieldKnightSkill1(MapManager.princessFields, piece,
                            changePiece = changePiece.Concat(GetFieldKnightSkill1(baseFields, piece,
                                new[] { -1, 1, 0, 0 }, new[] { 0, 0, -1, 1 })).ToList();
                        }
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
                    if (knight.Cost > 0)
                    {
                        _uiManager.ActiveSomeThingBox($"휴식하시겠습니까?\n({knight.Cost}만큼 체력이 회복됩니다.)", Rest);
                    }
                    else
                    {
                        Log("휴식을 취할 코스트가 충분하지 않습니다.");
                    }
                    //changePiece.Add(knight.CurrentFieldPiece);
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
                fieldEvent.Execute(field.fieldEventInfo);
                break;
            case MapType.Item : 
                itemEvent.Execute(field.itemInfo);
                break;
            case MapType.Heal :
                healEvent.Execute(
                    knight, _resourceManager.healEventSprite);
                break;
            case MapType.Dragon :
                battleEvent.Init(knight, _resourceManager.Dragon);
                battleEvent.Execute(true);
                break;
            case MapType.Boss :
                switch (CurrentKnightFloor)
                {
                    case 1 :
                        battleEvent.Init(knight, _resourceManager.Ruggle);
                        battleEvent.Execute(true);
                        break;
                    case 2 :
                        battleEvent.Init(knight, _resourceManager.DeathKnight);
                        battleEvent.Execute(true);
                        break;
                    case 3 :
                        battleEvent.Init(knight, _resourceManager.Dragon);
                        battleEvent.Execute(true);
                        break;
                }
                break;
            case MapType.Door :
                // _uiManager.ActiveSomeThingBox("다음 층으로 올라가시겠습니까?", MoveNextFloor);
                if (HasKey)
                {
                    _uiManager.ActiveSomeThingBox("다음 층으로 올라가시겠습니까?", MoveNextFloor);
                }
                else
                {
                    Log("열쇠가 없어 문을 열 수 없습니다.");
                }
                EventPrinting = false;
                break;
            case MapType.Princess :
                _uiManager.ActiveEndingScene();
                break;
        }
    }
    #endregion

    #region 층 이동 관련
    /// <summary>
    /// 층 관련
    /// </summary>
    private void MoveNextFloor()
    {
        HasKey = false;
        
        // 도트 데미지 관련 초기화
        DotDamageTime = false;
        turnsBeforeAscend = Turn;
        _uiManager.UpdateTurnText(Turn);
        // 층 이동
        CurrentKnightFloor++;
        DisplayFloor = CurrentKnightFloor; 
        knight.SelectedFloor = CurrentKnightFloor;

        var nextFloorInitPosField = MapManager.AllFieldMapData[CurrentKnightFloor-1][0, 0];
        
        knight.transform.position = MapManager.GridToWorldPosition(nextFloorInitPosField.gridPosition);
        knight.CurrentFieldPiece = nextFloorInitPosField;
        MapManager.LightTempKnightMove(nextFloorInitPosField.gridPosition);

        knight.Status.CurrentHp = knight.Status.MaxHp; // 체력 맥스로
        
        MapManager.RefreshMap();
        ChangeBehavior(knight.SelectedIdx);
    }

    public void ShowSelectArtifact(int count = 3 )
    {
        var canSelectArtifactList = _resourceManager.Artifacts.Where(x => !_dataManager.HasArtifactList.Contains(x)).ToList();
        var indexs = new List<int>();

        do
        {
            int index = Random.Range(0, canSelectArtifactList.Count);
            if (!indexs.Contains(index)) indexs.Add(index);

        } while (indexs.Count < count);
        
        // 화면 설정
        _uiManager.artifactSelectorObj.SetActive(true);
        for (int idx = 0; idx < count; idx++)
        {
            _uiManager.UIArtifacts[idx].Init(canSelectArtifactList[idx]);
        }
    }
    
    public void GetArtifact(Artifact artifact)
    {
        _dataManager.HasArtifactList.Add(artifact);
        _uiManager.AddHasArtifactUI(artifact);

        switch (artifact.Type)
        {
            case ArtifactType.AllStatUp :
                knight.Status.MaxHp     += _dataManager.ARTI_AllStatUp_Value;
                knight.Status.CurrentHp += _dataManager.ARTI_AllStatUp_Value;
                knight.Status.Dex       += _dataManager.ARTI_AllStatUp_Value;
                knight.Status.Power     += _dataManager.ARTI_AllStatUp_Value;
                knight.Status.Defense   += _dataManager.ARTI_AllStatUp_Value;
                break;
            case ArtifactType.DexUp :
                knight.Status.Dex += _dataManager.ARTI_DEXUP_Value;
                break;
            case ArtifactType.HpUp :
                knight.Status.CurrentHp += _dataManager.ARTI_HPUP_Value;
                knight.Status.MaxHp += _dataManager.ARTI_HPUP_Value;
                break;
            case ArtifactType.CostUp :
                KnightMaxCost++;
                PrincessMaxCost++;
                break;
            case ArtifactType.PrincessSkillUp :
                _dataManager.PrincessPowerSkillValue += _dataManager.ARTI_PrincessSkillUP_Value;
                break;
            case ArtifactType.KnightSkillUp :
                _dataManager.KnightRestRecoveryHpAddValue += _dataManager.ARTI_KnightSkillUP_Value;
                break;
            case ArtifactType.AddAttack :
                _dataManager.ARTI_AddAtack = true;
                break;
        }
    }

    public void CompleteSelectArtifact()
    {
        _uiManager.artifactSelectorObj.SetActive(false);
    }

    public void ClearBoss()
    {
        HasKey = true;
        _uiManager.UpdateKnightStatusInfo();
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