using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

enum FieldType
{
    Field,
    Princess, 
    Knight,
    PrincessMap,
    KnightMap,
}

public class MapManager : MonoBehaviour
{
    public Camera fieldCamera;
    public Camera princessMapCamera;
    public Camera knightMapCamera;
    [Header("Generator")]
    public GameObject generatorManagerObj;
    public int fieldWidth = 20;
    public int fieldHeight = 20;
    int width;
    int height;
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
    public TileBase CanSelectTile;
    // public TileBase BlockTile;
    // public TileBase EmptyTile;

    [Header("Princess Map")]
    public GameObject PrincessMapObject;
    public Tilemap PrincessTileMap;
    public TileBase PrincessItemTile;
    public TileBase PrincessEventTile;
    public TileBase PrincessMonsterTile;
    public TileBase PrincessHideTile;
    public TileBase PrincessBlockTile;
    public TileBase PrincessEmptyTile;

    

    [Header("Knight Map")]
    public GameObject KnightMapObject;
    public Tilemap KnightTileMap;
    public TileBase KnightItemTile;
    public TileBase KnightEventTile;
    public TileBase KnightMonsterTile;
    public TileBase KnightHideTile;
    public TileBase KnightBlockTile;
    public TileBase KnightEmptyTile;

    bool isCanFieldSelect = true;
    bool isPrincessMapSelect = true;
    bool isKnightMapSelect = true;


    GameObject selectCusorObj;
    Vector2 currentMouseGridPos;
    float cellSize = 0.32f;


    FieldPiece[,] princessMapData;
    FieldPiece[,] knightMapData;

    FieldType currentField;
    List<FieldPiece> canSelectFields = new List<FieldPiece>();

    private void Awake() {
        selectCusorObj = Instantiate(Resources.Load<GameObject>("SelectCursorObject"));
    }
    private void Start() {
        // tileMapData.MapData;
        width = fieldWidth + 2;
        height = fieldHeight + 2;
        GenerateField();

        // tmp
        printMap();
        princessMapData = (FieldPiece[,])FieldMapData.Clone();
        for (int i = 0; i < width/2; i++)
        {
            for (int j = 0; j < height/2; j++)
            {
                princessMapData[j,i].MapType = MapType.Hide;
            }
        }
        canSelectFields.Add(FieldMapData[15,15]);
        canSelectFields.Add(FieldMapData[15,14]);
        canSelectFields.Add(FieldMapData[15,13]);
        BuildAllField(FieldType.Princess);
        // GenerateMap();
    }
    
    private void Update()
    {
        Vector2 mousePosition = fieldCamera.ScreenToWorldPoint(Input.mousePosition);
        if(isCanFieldSelect){
            PlaceSelectCursor(mousePosition, ObjectField.transform.position);
            showCanSelectField();
        }
        if(Input.GetMouseButtonDown(0)){
            Vector2 grid = WorldPositionToGrid(mousePosition, ObjectField.transform.position);
            Debug.Log(grid + " " + isInGrid(grid));
            if(isInGrid(grid)) Debug.Log(FieldMapData[(int)grid.y, (int)grid.x]);
        }
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            currentField = FieldType.Field;
            BuildAllField(currentField);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2)){
            currentField = FieldType.Princess;
            BuildAllField(currentField);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3)){
            currentField = FieldType.PrincessMap;
            BuildAllField(currentField);
        }
        else if(Input.GetKeyDown(KeyCode.Z)){
            currentField = FieldType.PrincessMap;
            BuildAllField(currentField);
        }
        else if(Input.GetKeyDown(KeyCode.X)){
            currentField = FieldType.PrincessMap;
            BuildAllField(currentField);
        }
    }

    void showCanSelectField(){
        foreach (FieldPiece piece in canSelectFields)
        {
            
            UITileMap.SetTile(new Vector3Int(piece.gridPosition.x, piece.gridPosition.y, 0), CanSelectTile);
        }
    }
    bool isInGrid(Vector2 gridPosition){
        if(gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height){
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

    Vector2 GridToWorldPosition(Vector2 gridPosition, Vector2 offset){
        return gridPosition * cellSize + new Vector2(cellSize / 2, cellSize / 2) + offset ;
    }

    Vector2 WorldPositionToGrid(Vector2 worldPosition, Vector2 offset){
        Vector2 tmp = worldPosition - offset;
        return  new Vector2((int)(tmp.x / cellSize),(int)(tmp.y / cellSize));   
    }

    void GenerateField(){
        GeneratorManager generatorManager = generatorManagerObj.GetComponent<GeneratorManager>(); 
        generatorManager.ClearAllMaps();
        generatorManager.width = width;
        generatorManager.height = height;
        generatorManager.chanceOfEmptySpace = 1- mapBlockRatio;
        generatorManager.GenerateNewMap("Maze"); 

        FieldMapData = new FieldPiece[height,width];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                FieldMapData[j, i] = new FieldPiece
                {
                    MapType = generatorManager.MapData[j, i] ? MapType.Block : MapType.Empty,
                    gridPosition = new Vector2Int(i, j)
                };
            }
        }
        float remainRatio = 1-mapBlockRatio;
        GenerateFieldObjects(mapItemboxRatio/remainRatio, MapType.Item);
        remainRatio -= mapItemboxRatio;
        GenerateFieldObjects(mapEventRatio/remainRatio, MapType.Event);
        remainRatio -= mapEventRatio;
        GenerateFieldObjects(mapMonsterRatio/remainRatio, MapType.Monster);
        // printMap();
    }

    void BuildAllField(FieldType type){
        if(type == FieldType.Field){
            FieldTileMap.ClearAllTiles();
            BuildMap(FieldMapData, MapType.Monster, FieldTileMap, MonsterTile);
            BuildMap(FieldMapData, MapType.Item, FieldTileMap, ItemTile);
            BuildMap(FieldMapData, MapType.Event, FieldTileMap, EventTile);
        }
        else if(type == FieldType.Princess){
            FieldTileMap.ClearAllTiles();
            BuildMap(princessMapData, MapType.Monster, FieldTileMap, MonsterTile);
            BuildMap(princessMapData, MapType.Item, FieldTileMap, ItemTile);
            BuildMap(princessMapData, MapType.Event, FieldTileMap, EventTile);
            BuildMap(princessMapData, MapType.Hide, FieldTileMap, HideTile);
        }
        else if(type == FieldType.PrincessMap){
            PrincessTileMap.ClearAllTiles();
            BuildMap(princessMapData, MapType.Hide, PrincessTileMap, PrincessHideTile);
            BuildMap(princessMapData, MapType.Block, PrincessTileMap, PrincessBlockTile);
            BuildMap(princessMapData, MapType.Empty, PrincessTileMap, PrincessEmptyTile);
            BuildMap(princessMapData, MapType.Monster, PrincessTileMap, PrincessMonsterTile);
            BuildMap(princessMapData, MapType.Item, PrincessTileMap, PrincessItemTile);
            BuildMap(princessMapData, MapType.Event, PrincessTileMap, PrincessEventTile);
        }

    }
    
    void GenerateFieldObjects(float generateRatio, MapType value)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
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
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++){
                if (mapData[y, x].MapType == type)
                {
                    map.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
    void printMap(){
        string arrayStr = "";
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    arrayStr += FieldMapData[j,i] + " ";
                }
                arrayStr += "\n";
            }

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