using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;

public enum FieldType
{
    Field,
    Princess, 
    Knight,
}

public class MapManager : MonoBehaviour
{
    public GameManager gameManager;
    public Camera fieldCamera;

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
    public TileBase DragonTile;

    bool isCanFieldSelect = true;
    bool isPrincessMapSelect = true;
    bool isKnightMapSelect = true;


    GameObject selectCusorObj;
    List<FieldPiece> canSelectList = new List<FieldPiece>();
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
        
        LightField(FieldType.Princess, new Vector2(fieldWidth-1,fieldHeight-1));
        LightField(FieldType.Princess, new Vector2(fieldWidth-2,fieldHeight-1));
        LightField(FieldType.Princess, new Vector2(fieldWidth-1,fieldHeight-2));
        LightField(FieldType.Princess, new Vector2(fieldWidth-2,fieldHeight-2));
        BuildAllField(type);
    }
    
    private void Update()
    {
        if (!gameManager.EventPrinting)
        {
            Vector2 mousePosition = fieldCamera.ScreenToWorldPoint(Input.mousePosition);
            if(isCanFieldSelect){
                PlaceSelectCursor(mousePosition, ObjectField.transform.position);
            }
            if(Input.GetMouseButtonDown(0)){
                Vector2 grid = WorldPositionToGrid(mousePosition, ObjectField.transform.position);
                if(isInGrid(grid)){
                    gameManager.ClickMap(FieldMapData[(int)grid.y, (int)grid.x]);
                    selectCusorObj.transform.position = new Vector2(100,100);
                    isCanFieldSelect = false;
                    // foreach(FieldPiece piece in canSelectList){
                    //     if(piece._canSelect){ piece._canSelect = false; }
                    // }
                    canSelectList.Clear();
                } 
            }
        }
    }
    public void LightField(FieldType type, Vector2 position){
        if(type == FieldType.Princess){
            princessFields[(int) position.y,(int) position.x].IsLight = true;
            princessFields[(int) position.y,(int) position.x].MapType = FieldMapData[(int) position.y,(int) position.x].MapType;
        }
        else if(type == FieldType.Knight){
            knightFields[(int) position.y,(int) position.x].IsLight = true;
            knightFields[(int) position.y,(int) position.x].MapType = FieldMapData[(int) position.y ,(int) position.x].MapType;
        }
        BuildAllField(type);

    }
    public void showCanSelectField(List<FieldPiece> canSelectFields){
        UITileMap.ClearAllTiles();
        foreach (FieldPiece piece in canSelectFields)
        {   
            Debug.Log(piece.gridPosition);
            UITileMap.SetTile(new Vector3Int(piece.gridPosition.x + 1, piece.gridPosition.y+ 1, 0), CanSelectTile);
            isCanFieldSelect = true;
            
            piece._canSelect = true;
            princessFields[piece.gridPosition.y, piece.gridPosition.x]._canSelect = true;
            canSelectList.Add(princessFields[piece.gridPosition.y, piece.gridPosition.x]);
            knightFields[piece.gridPosition.y, piece.gridPosition.x]._canSelect = true;
            canSelectList.Add(knightFields[piece.gridPosition.y, piece.gridPosition.x]);
            FieldMapData[piece.gridPosition.y, piece.gridPosition.x]._canSelect = true;
            canSelectList.Add(FieldMapData[piece.gridPosition.y, piece.gridPosition.x]);
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
        generatorManager.ClearAllMaps();
        generatorManager.chanceOfEmptySpace = 1- mapBlockRatio;
        generatorManager.GenerateNewMap("Maze"); 

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
        FieldMapData[0,0].MapType = MapType.Empty;
        FieldMapData[fieldHeight-1,fieldWidth-1].MapType = MapType.Empty;
        FieldMapData[fieldHeight-1,fieldWidth-2].MapType = MapType.Block;
        FieldMapData[fieldHeight-2,fieldWidth-1].MapType = MapType.Dragon;
        FieldMapData[fieldHeight-2,fieldWidth-2].MapType = MapType.Block;
        // BuildAllField(FieldType.Field);
    }

    public void BuildAllField(FieldType type){
        currentField = type;
        FieldPiece[,] fieldPieces = princessFields;
        FieldTileMap.ClearAllTiles();
        if(type == FieldType.Princess){
            fieldPieces = princessFields;
        }
        else if(type == FieldType.Knight){
            fieldPieces = knightFields;
        }
        FieldTileMap.ClearAllTiles();
        BuildMap(fieldPieces, MapType.Block, FieldTileMap, BlockTile);
        BuildMap(fieldPieces, MapType.Item, FieldTileMap, ItemTile);
        BuildMap(fieldPieces, MapType.Empty, FieldTileMap, EmptyTile);
        BuildMap(fieldPieces, MapType.Monster, FieldTileMap, MonsterTile);
        BuildMap(fieldPieces, MapType.Event, FieldTileMap, EventTile);
        BuildMap(fieldPieces, MapType.Heal, FieldTileMap, HealTile);
        BuildMap(fieldPieces, MapType.Dragon, FieldTileMap, DragonTile);
        BuildMap(fieldPieces, MapType.Hide, FieldTileMap, HideTile);

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
        RefreshMap();
    }

    public void RefreshMap(){
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

    public bool IsLight = false;

    public bool _canSelect = false;


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
}