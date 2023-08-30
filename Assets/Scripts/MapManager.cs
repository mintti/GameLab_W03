using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;

public enum FieldType
{
    Field,
    Princess, 
    Knight,
    PrincessMap,
    KnightMap,
}

public class MapManager : MonoBehaviour
{
    public GameManager gameManager;
    public Camera fieldCamera;
    // public Camera princessMapCamera;
    // public Camera knightMapCamera;

    [Header("Generator")]
    public GameObject generatorManagerObj;
    public int fieldWidth = 20;
    public int fieldHeight = 20;
    FieldPiece[,] FieldMapData;
    public float mapEmptyRatio = 0.35f;
    public float mapMonsterRatio = 0.25f;
    public float mapEventRatio = 0.2f;
    public float mapItemboxRatio = 0.1f;
    public float mapBlockRatio = 0.1f;

    
    [Header("Field")]
    public GameObject ObjectField;
    public Tilemap FieldTileMap;
    public Tilemap UITileMap;
    public TileBase ItemTile;
    public TileBase EventTile;
    public TileBase MonsterTile;
    public TileBase HideTile;
    public TileBase HealTile;
    public TileBase CanSelectTile;
    public TileBase BlockTile;
    public TileBase EmptyTile;

    [Header("Princess Map")]
    public GameObject PrincessMapObject;
    public Tilemap PrincessTileMap;
    public TileBase PrincessItemTile;
    public TileBase PrincessEventTile;
    public TileBase PrincessMonsterTile;
    public TileBase PrincessHideTile;
    public TileBase PrincessBlockTile;
    public TileBase PrincessEmptyTile;
    public TileBase PrincessSelfTile;

    

    [Header("Knight Map")]
    public GameObject KnightMapObject;
    public Tilemap KnightTileMap;
    public TileBase KnightItemTile;
    public TileBase KnightEventTile;
    public TileBase KnightMonsterTile;
    public TileBase KnightHideTile;
    public TileBase KnightBlockTile;
    public TileBase KnightEmptyTile;
    public TileBase KnightSelfTile;

    bool isCanFieldSelect = true;
    bool isPrincessMapSelect = true;
    bool isKnightMapSelect = true;


    GameObject selectCusorObj;
    Vector2 currentMouseGridPos;
    float cellSize = 1.28f;


    public FieldPiece[,] princessFields;
    public FieldPiece[,] knightFields;


    FieldType currentField;
    bool isMapOpen = false;

    private void Awake() {
        selectCusorObj = Instantiate(Resources.Load<GameObject>("SelectCursorObject"));
    }
    public void CreateMap(FieldType type){

        GenerateField();
        princessFields = new FieldPiece[fieldHeight, fieldWidth];
        knightFields = new FieldPiece[fieldHeight, fieldWidth];

        
        for (int i = 0; i < fieldWidth; i++)
        {
            for (int j = 0; j < fieldHeight; j++)
            {
                princessFields[j, i]= new FieldPiece
                { 
                    IsLight = false,
                    MapType = MapType.Hide,
                    gridPosition = new Vector2Int(i, j)
                };
                princessFields[j, i].Init(gameManager, this);
                knightFields[j, i] = new FieldPiece
                { 
                    IsLight = false,
                    MapType = MapType.Hide,
                    gridPosition = new Vector2Int(i, j)
                };
                knightFields[j, i].Init(gameManager, this);
    
            }
        }
        BuildAllField(type);
    }
    
    private void Update()
    {
        Vector2 mousePosition = fieldCamera.ScreenToWorldPoint(Input.mousePosition);
        if(isCanFieldSelect){
            PlaceSelectCursor(mousePosition, ObjectField.transform.position);
        }
        if(Input.GetMouseButtonDown(0)){
            Vector2 grid = WorldPositionToGrid(mousePosition, ObjectField.transform.position);
            if(isInGrid(grid)){
                gameManager.ClickMap(FieldMapData[(int)grid.y, (int)grid.x]);
                isCanFieldSelect = false;
            } 
        }
        // else if(Input.GetKeyDown(KeyCode.Z)){
        //     currentField = FieldType.PrincessMap;Knight
        //     BuildAllField(currentField);
        // }
        // else if(Input.GetKeyDown(KeyCode.X)){
        //     currentField = FieldType.PrincessMap;
        //     BuildAllField(currentField);
        // }
    }
    public void LightField(FieldType type, Vector2 position){
        if(type == FieldType.Princess){
            princessFields[(int) position.y,(int) position.x].IsLight = true;
            princessFields[(int) position.y,(int) position.x].MapType = FieldMapData[(int) position.y,(int) position.x].MapType;
            if(position.x != 0){
                princessFields[(int) position.y,(int) position.x-1].IsLight = true;
                princessFields[(int) position.y,(int) position.x-1].MapType = FieldMapData[(int) position.y,(int) position.x - 1].MapType;
            }
            if(position.x != 19){
                princessFields[(int) position.y,(int) position.x+1].IsLight = true;
                princessFields[(int) position.y,(int) position.x+1].MapType = FieldMapData[(int) position.y,(int) position.x+1 + 1].MapType;

            }
            if(position.y != 19){
                princessFields[(int) position.y+1,(int) position.x].IsLight = true;
                princessFields[(int) position.y+1,(int) position.x].MapType = FieldMapData[(int) position.y+1,(int) position.x].MapType;
            }
            if(position.y != 0){
                princessFields[(int) position.y-1,(int) position.x].IsLight = true;
                princessFields[(int) position.y-1,(int) position.x].MapType = FieldMapData[(int) position.y-1,(int) position.x].MapType;
            }
        }
        else if(type == FieldType.Knight){
            knightFields[(int) position.y,(int) position.x].IsLight = true;
            knightFields[(int) position.y,(int) position.x].MapType = FieldMapData[(int) position.y ,(int) position.x].MapType;
            if(position.x != 0){
                knightFields[(int) position.y,(int) position.x-1].IsLight = true;
                knightFields[(int) position.y,(int) position.x-1].MapType = FieldMapData[(int) position.y ,(int) position.x-1].MapType;
            }
            if(position.x != 19){
                knightFields[(int) position.y,(int) position.x+1].IsLight = true;
                knightFields[(int) position.y,(int) position.x+1].MapType = FieldMapData[(int) position.y ,(int) position.x+1].MapType;

            }
            if(position.y != 0){
                knightFields[(int) position.y-1,(int) position.x].IsLight = true;
                knightFields[(int) position.y-1,(int) position.x].MapType = FieldMapData[(int) position.y-1 ,(int) position.x].MapType;
            }
            if(position.y != 19){
                knightFields[(int) position.y+1,(int) position.x].IsLight = true;
                knightFields[(int) position.y+1,(int) position.x].MapType = FieldMapData[(int) position.y+1 ,(int) position.x ].MapType;
            }
        }
        BuildAllField(type);

    }
    public void showCanSelectField(List<FieldPiece> canSelectFields){
        UITileMap.ClearAllTiles();
        foreach (FieldPiece piece in canSelectFields)
        {
            UITileMap.SetTile(new Vector3Int(piece.gridPosition.x + 1, piece.gridPosition.y+ 1, 0), CanSelectTile);
            isCanFieldSelect = true;
        }
    }
    bool isInGrid(Vector2 gridPosition){
        if(gridPosition.x >= 0 && gridPosition.x < fieldWidth && gridPosition.y >= 0 && gridPosition.y < fieldHeight){
            return true;
        }
        return false;
    }
    void PlaceSelectCursor(Vector2 position, Vector2 offset){
        Vector2 grid = WorldPositionToGrid(position, offset);
        if(isInGrid(grid)){
            selectCusorObj.transform.position = GridToWorldPosition(grid, offset);
        }
        else{
            selectCusorObj.transform.position = new Vector2(100,100);
        }
    }
    // public void OpenMap(){
    //     if(!isMapOpen){
    //         if(currentField == FieldType.Princess){
    //             fieldCamera.enabled = false; 
    //             princessMapCamera.enabled = true;
    //             Vector2 pos = gameManager.princess.CurrentFieldPiece.gridPosition;
    //             princessFields[(int)pos.y, (int)pos.x].MapType = MapType.Self;
    //             BuildAllField(FieldType.PrincessMap);
    //         }
    //         else if(currentField == FieldType.Knight){
    //             fieldCamera.enabled = false; 
    //             knightMapCamera.enabled = true;
    //             Vector2 pos = gameManager.knight.CurrentFieldPiece.gridPosition;
    //             knightFields[(int)pos.y, (int)pos.x].MapType = MapType.Self;
    //             BuildAllField(FieldType.KnightMap);
    //         }
    //         isMapOpen = true;
    //     }
        
    //     else{
    //         if(currentField == FieldType.PrincessMap){
    //             princessMapCamera.enabled = false;
    //             fieldCamera.enabled = true; 
    //             Vector2 pos = gameManager.princess.CurrentFieldPiece.gridPosition;
    //             princessFields[(int)pos.y, (int)pos.x].MapType = MapType.Empty;
    //             BuildAllField(FieldType.Princess);
    //         }
    //         else if(currentField == FieldType.KnightMap){
    //             knightMapCamera.enabled = false;
    //             fieldCamera.enabled = true; 
    //             Vector2 pos = gameManager.knight.CurrentFieldPiece.gridPosition;
    //             knightFields[(int)pos.y, (int)pos.x].MapType = MapType.Empty;
    //             BuildAllField(FieldType.Knight);
    //         }
    //         isMapOpen = false;
    //     }
    // }

    public Vector2 GridToWorldPosition(Vector2 gridPosition, Vector2 offset){
        Vector2 position = new Vector2(gridPosition.x + 1, gridPosition.y + 1);
        return position * cellSize + new Vector2(cellSize / 2, cellSize / 2) + offset;
    }
    public Vector2 GridToWorldPosition(Vector2 gridPosition){
        Vector2 position = new Vector2(gridPosition.x + 1, gridPosition.y + 1);
        return position * cellSize + new Vector2(cellSize / 2 + ObjectField.transform.position.x, cellSize / 2 + ObjectField.transform.position.y);
    }

    public Vector2 WorldPositionToGrid(Vector2 worldPosition, Vector2 offset){
        Vector2 tmp = worldPosition - offset;
        return  new Vector2((int)(tmp.x / cellSize) - 1,(int)(tmp.y / cellSize) - 1);   
    }

    void GenerateField(){
        GeneratorManager generatorManager = generatorManagerObj.GetComponent<GeneratorManager>(); 
        generatorManager.width = fieldWidth + 2;
        generatorManager.height = fieldHeight + 2;
        while(true){
            generatorManager.ClearAllMaps();
            generatorManager.chanceOfEmptySpace = 1- mapBlockRatio;
            generatorManager.GenerateNewMap("Maze"); 
            if(!generatorManager.MapData[1, 1] && !generatorManager.MapData[fieldHeight, fieldWidth]){
                break;
            }
        }

        FieldMapData = new FieldPiece[fieldHeight,fieldWidth];
        for (int i = 0; i < fieldWidth; i++)
        {
            for (int j = 0; j < fieldHeight; j++)
            {
                FieldMapData[j, i] = new FieldPiece
                {
                    MapType = generatorManager.MapData[j + 1, i + 1] ? MapType.Block : MapType.Empty,
                    gridPosition = new Vector2Int(i, j)
                };
                
                FieldMapData[j, i].Init(gameManager, this);
            }
        }
        float remainRatio = 1-mapBlockRatio;
        GenerateFieldObjects(mapItemboxRatio/remainRatio, MapType.Item);
        remainRatio -= mapItemboxRatio;
        GenerateFieldObjects(mapEventRatio/remainRatio, MapType.Event);
        remainRatio -= mapEventRatio;
        GenerateFieldObjects(mapMonsterRatio/remainRatio, MapType.Monster);
        // BuildAllField(FieldType.Field);
    }

    public void BuildAllField(FieldType type){
        // if(type == FieldType.Field){
        //     FieldTileMap.ClearAllTiles();
        //     BuildMap(FieldMapData, MapType.Monster, FieldTileMap, MonsterTile);
        //     BuildMap(FieldMapData, MapType.Item, FieldTileMap, ItemTile);
        //     BuildMap(FieldMapData, MapType.Event, FieldTileMap, EventTile);
        // }
        currentField = type;
        if(type == FieldType.Princess){
            FieldTileMap.ClearAllTiles();
            BuildMap(princessFields, MapType.Monster, FieldTileMap, MonsterTile);
            BuildMap(princessFields, MapType.Item, FieldTileMap, ItemTile);
            BuildMap(princessFields, MapType.Block, FieldTileMap, BlockTile);
            BuildMap(princessFields, MapType.Empty, FieldTileMap, EmptyTile);
            BuildMap(princessFields, MapType.Event, FieldTileMap, EventTile);
            BuildMap(princessFields, MapType.Heal, FieldTileMap, HealTile);
            BuildMap(princessFields, MapType.Hide, FieldTileMap, HideTile);
        }
        else if(type == FieldType.Knight){
            FieldTileMap.ClearAllTiles();
            BuildMap(knightFields, MapType.Block, FieldTileMap, KnightBlockTile);
            BuildMap(knightFields, MapType.Empty, FieldTileMap, KnightEmptyTile);
            BuildMap(knightFields, MapType.Monster, FieldTileMap, KnightMonsterTile);
            BuildMap(knightFields, MapType.Item, FieldTileMap, KnightItemTile);
            BuildMap(knightFields, MapType.Event, FieldTileMap, KnightEventTile);
            BuildMap(knightFields, MapType.Heal, FieldTileMap, HealTile);
            BuildMap(knightFields, MapType.Hide, FieldTileMap, KnightHideTile);
        }
        else if(type == FieldType.PrincessMap){
            PrincessTileMap.ClearAllTiles();
            BuildMap(princessFields, MapType.Heal, PrincessTileMap, HealTile);
            BuildMap(princessFields, MapType.Block, PrincessTileMap, PrincessBlockTile);
            BuildMap(princessFields, MapType.Empty, PrincessTileMap, PrincessEmptyTile);
            BuildMap(princessFields, MapType.Monster, PrincessTileMap, PrincessMonsterTile);
            BuildMap(princessFields, MapType.Item, PrincessTileMap, PrincessItemTile);
            BuildMap(princessFields, MapType.Event, PrincessTileMap, PrincessEventTile);
            BuildMap(princessFields, MapType.Self, PrincessTileMap, PrincessSelfTile);
            BuildMap(princessFields, MapType.Hide, PrincessTileMap, PrincessHideTile);
        }
        else if(type == FieldType.KnightMap){
            PrincessTileMap.ClearAllTiles();
            BuildMap(knightFields, MapType.Heal, KnightTileMap, HealTile);
            BuildMap(knightFields, MapType.Block, KnightTileMap, KnightBlockTile);
            BuildMap(knightFields, MapType.Empty, KnightTileMap, KnightEmptyTile);
            BuildMap(knightFields, MapType.Monster, KnightTileMap, KnightMonsterTile);
            BuildMap(knightFields, MapType.Item, KnightTileMap, KnightItemTile);
            BuildMap(knightFields, MapType.Self, KnightTileMap, KnightSelfTile);
            BuildMap(knightFields, MapType.Hide, KnightTileMap, KnightHideTile);
        }

    }

    
    
    void GenerateFieldObjects(float generateRatio, MapType value)
    {
        for (int i = 0; i < fieldWidth; i++)
        {
            for (int j = 0; j < fieldHeight; j++)
            {
                if((i == 0 && j == 0) || (i == fieldWidth -1 && j == fieldHeight -1)){
                    continue;
                }
                if(FieldMapData[j, i].MapType == MapType.Empty){
                    float val = Random.value;
                    if(val < generateRatio){
                        FieldMapData[j, i].MapType = value;
                    }
                }
            }
        }
    }
    
    public void BuildMap(FieldPiece[,] mapData, MapType type, Tilemap map, TileBase tile)
    {
        for (int x = 0; x < fieldWidth; x++){
            for (int y = 0; y < fieldHeight; y++){
                if (mapData[y, x].MapType == type)
                {
                    if(!mapData[y, x].IsLight){ 
                        mapData[y, x].MapType = MapType.Hide;
                        map.SetTile(new Vector3Int(x+1, y+1, 0), HideTile);
                    }
                    else{
                        map.SetTile(new Vector3Int(x+1, y+1, 0), tile);
                    }
                }
            }
        }
    }
    
    public void ClearMapPiece(FieldPiece fieldPiece){
        
        princessFields[fieldPiece.gridPosition.y, fieldPiece.gridPosition.x].MapType = MapType.Empty;
        knightFields[fieldPiece.gridPosition.y, fieldPiece.gridPosition.x].MapType = MapType.Empty;
        FieldMapData[fieldPiece.gridPosition.y, fieldPiece.gridPosition.x].MapType = MapType.Empty;
        BuildAllField(currentField);
    }

    public void SetMapPiece(FieldPiece fieldPiece, MapType type){
        princessFields[fieldPiece.gridPosition.y, fieldPiece.gridPosition.x].MapType = type;
        knightFields[fieldPiece.gridPosition.y, fieldPiece.gridPosition.x].MapType = type;
    }
    void printMap(FieldPiece[,] pieces){
        string arrayStr = "";
            for (int j = 0; j < fieldHeight; j++)
            {
                for (int i = 0; i < fieldWidth; i++)
                {
                    arrayStr += pieces[j,i].MapType + " ";
                }
                arrayStr += "\n";
            }
        Debug.Log(arrayStr);
    }
}

public class FieldPiece
{
    private GameManager _gameManager;
    private MapManager _mapManager;
    // private SpriteRenderer _renderer;

    private MapType _mapType = MapType.Empty;
    public MapType MapType {
        get { return _mapType; }
        set { _mapType = value; }
    }
    // public Sprite Sprite;

    public bool IsLight = true;

    private bool _canSelect;


    public int EventIndex { get; set; }

    public Vector2Int gridPosition;

    public void Init(GameManager gameManager, MapManager mapManager)
    {
        _gameManager = gameManager;
        _mapManager = mapManager;
    }

    public void UpdateMapType(MapType type, int index = -1)
    {
        _mapType = type;
        Debug.Log(_mapManager);
        _mapManager.SetMapPiece(this, type);

        EventIndex = index;
        
        // 타입정보에 따라 맵 오브젝트 업데이트 필요
    }
         

    public void OnMouseInput()
    {
        // if (CanSelect)
        // {
        //     _gameManager.ClickMap(this);
        // }
    }   
}