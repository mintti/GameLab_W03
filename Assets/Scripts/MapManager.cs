using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using Mono.Cecil.Cil;
using System.Reflection;

public enum FieldType
{
    Field,
    Princess, 
    Knight,
}

public class MapManager : MonoBehaviour
{
    public GameManager gameManager;
    public UIManager _UIManager;
    public Camera fieldCamera;

    [Header("Generator")]
    public GameObject generatorManagerObj;
    public List<Vector2Int> fieldSizeList;
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
    public TileBase DoorTile;
    public TileBase DragonTile;

    public Dictionary<int, FieldPiece[,]> AllFieldMapData = new Dictionary<int, FieldPiece[,]>();
    int floorCount = 0;
    int currentFloor = 0;



    GameObject selectCusorObj;
    public List<FieldPiece> canSelectList = new List<FieldPiece>();
    float cellSize = 1.28f;


    public FieldType currentField = FieldType.Field;

    private void Awake() {
        selectCusorObj = Instantiate(Resources.Load<GameObject>("SelectCursorObject"));
    }

    public void InitMap(){
        floorCount = fieldSizeList.Count;
        currentFloor = 0;
        for(int i = 0; i < floorCount; ++i){
            if (!AllFieldMapData.ContainsKey(i))
                AllFieldMapData.Add(i, CreateMap(fieldSizeList[i]));
        }

    }
    public FieldPiece GetFieldPiece(int floor, Vector2Int position){
            return AllFieldMapData[floor][position.x, position.y];
    }
    public FieldPiece GetFieldPiece(Vector2Int position){
            return AllFieldMapData[currentFloor][position.x, position.y];
    }
    public FieldPiece[,] CreateMap(Vector2Int fieldSize){

        GeneratorManager generatorManager = generatorManagerObj.GetComponent<GeneratorManager>(); 
        generatorManager.width = fieldSize.x + 2;
        generatorManager.height = fieldSize.y + 2;
        generatorManager.ClearAllMaps();
        generatorManager.chanceOfEmptySpace = 1- mapBlockRatio;
        generatorManager.GenerateNewMap("Maze"); 

        FieldPiece[,] MapData = new FieldPiece[fieldSize.x,fieldSize.y];
        for (int i = 0; i < fieldSize.x; i++)
        {
            for (int j = 0; j < fieldSize.y; j++)
            {
                MapData[i, j] = new FieldPiece();
                MapData[i, j].Init(new Vector2Int(i, j), generatorManager.MapData[i + 1, j + 1] ? MapType.Block : MapType.Empty);                
            }
        }
        MapData[0, 0].SetMapType(MapType.Player);
        while(true){
            int i = (int)(Random.value * fieldSizeList[currentFloor].x);
            int j = (int)(Random.value * fieldSizeList[currentFloor].y);
            if(MapData[i, j].MapType == MapType.Empty){
                MapData[i, j].SetMapType(MapType.Door);
                break;
            }
        }
        float remainRatio = 1-mapBlockRatio;
        GenerateFieldObjects(MapData, mapItemboxRatio/remainRatio, MapType.Item);
        remainRatio -= mapItemboxRatio;
        GenerateFieldObjects(MapData, mapEventRatio/remainRatio, MapType.Event);
        remainRatio -= mapEventRatio;
        GenerateFieldObjects(MapData, mapMonsterRatio/remainRatio, MapType.Monster);

        return MapData;
    }
    
    void GenerateFieldObjects(FieldPiece[,] mapData, float generateRatio, MapType value)
    {
        for (int i = 0; i < fieldSizeList[currentFloor].x; i++)
        {
            for (int j = 0; j < fieldSizeList[currentFloor].y; j++)
            {
                if((i == 0 && j == 0) || (i == fieldSizeList[currentFloor].x -1 && j == fieldSizeList[currentFloor].y -1)){
                    continue;
                }
                if(mapData[i, j].MapType == MapType.Empty){
                    float val = Random.value;
                    if(val < generateRatio){
                        UpdateMapType(mapData[i, j], value);
                    }
                }
            }
        }
    }
    private void Update()
    {
        if (!gameManager.EventPrinting)
        {
            Vector2 mousePosition = fieldCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int grid = WorldPositionToGrid(mousePosition, ObjectField.transform.position);
            if(isInGrid(grid)){
                PlaceSelectCursor(mousePosition, ObjectField.transform.position);
                if(Input.GetMouseButtonDown(0)){
                    gameManager.ClickMap(AllFieldMapData[currentFloor][(int)grid.x, (int)grid.y]);
                }
                
                if(AllFieldMapData[currentFloor][grid.x, grid.y].MapType == MapType.Monster){
                    _UIManager.TileInfUI(MapType.Monster, AllFieldMapData[currentFloor][grid.x, grid.y].monsterInfo);
                    Debug.Log(AllFieldMapData[currentFloor][grid.x, grid.y].monsterInfo.Name);
                }
                else if(AllFieldMapData[currentFloor][grid.x, grid.y].MapType == MapType.Item){
                    //_UIManager.SetInfoUI(MapType.Item, null);
                    Debug.Log(AllFieldMapData[currentFloor][grid.x, grid.y].itemInfo.Type);
                }
                else if(AllFieldMapData[currentFloor][grid.x, grid.y].MapType == MapType.Event){
                    //_UIManager.SetInfoUI(MapType.Event, null);
                    Debug.Log(AllFieldMapData[currentFloor][grid.x, grid.y].fieldEventInfo.Type);
                }
            }
        }
    }
    public void LightField(FieldType type, Vector2Int position){
        if(type == FieldType.Princess){
            AllFieldMapData[currentFloor][position.x, position.y].PrincessIsLight = true;
        }
        else if(type == FieldType.Knight){
            AllFieldMapData[currentFloor][position.x, position.y].KnightIsLight = true;
        }
    }

    // private List<FieldPiece> _backup;
    public void showCanSelectField(List<FieldPiece> _canSelectFields)
    {
        // canSelectList.Clear();
        canSelectList = new List<FieldPiece>(_canSelectFields);
        
        UITileMap.ClearAllTiles();
        foreach (FieldPiece piece in _canSelectFields)
        {   
            UITileMap.SetTile(new Vector3Int(piece.gridPosition.x + 1, piece.gridPosition.y+ 1, 0), CanSelectTile);
            
            canSelectList.Add(AllFieldMapData[currentFloor][piece.gridPosition.x, piece.gridPosition.y]);
        }
        
    }
    bool isInGrid(Vector2 gridPosition){
        if(gridPosition.x >= 0 && gridPosition.x < fieldSizeList[currentFloor].x && gridPosition.y >= 0 && gridPosition.y < fieldSizeList[currentFloor].y){
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

    public Vector2Int WorldPositionToGrid(Vector2 worldPosition, Vector2 offset){
        Vector2 tmp = worldPosition - offset;
        return  new Vector2Int((int)(tmp.x / cellSize) - 1,(int)(tmp.y / cellSize) - 1);   
    }

    

    public void BuildAllField(FieldType type){
        currentField = type;
        FieldTileMap.ClearAllTiles();
        BuildMap(MapType.Block, FieldTileMap, BlockTile);
        BuildMap(MapType.Item, FieldTileMap, ItemTile);
        BuildMap(MapType.Empty, FieldTileMap, EmptyTile);
        BuildMap(MapType.Monster, FieldTileMap, MonsterTile);
        BuildMap(MapType.Event, FieldTileMap, EventTile);
        BuildMap(MapType.Heal, FieldTileMap, HealTile);
        BuildMap(MapType.Door, FieldTileMap, DoorTile);
        BuildMap(MapType.Dragon, FieldTileMap, DragonTile);
    }
    
    public void BuildMap(MapType mapType, Tilemap map, TileBase tile)
    {
        for (int x = 0; x < fieldSizeList[currentFloor].x; x++){
            for (int y = 0; y < fieldSizeList[currentFloor].y; y++){
                if (AllFieldMapData[currentFloor][x, y].MapType == mapType)
                {
                    if((currentField == FieldType.Princess && !AllFieldMapData[currentFloor][x, y].PrincessIsLight) || (currentField == FieldType.Knight && !AllFieldMapData[currentFloor][x, y].KnightIsLight)){ 
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
        
        fieldPiece.SetMapType(MapType.Empty);
        RefreshMap();
    }

    public void RefreshMap(){
        BuildAllField(currentField);

    }

    public void UpdateMapType(FieldPiece fieldPiece, MapType type){
        fieldPiece.SetMapType(type);
        
        if(type == MapType.Monster){
            // fieldPiece.monsterInfo = moneter;
            fieldPiece.monsterInfo = gameManager._resourceManager.GetRandomMonster();
        }
        else if(type == MapType.Item){
            // fieldPiece.itemInfo = itemInfo;
            fieldPiece.itemInfo = gameManager._resourceManager.GetRandomItemEvent();
        }
        else if(type == MapType.Event){
            // fieldPiece.fieldEventInfo = eventInfo;
            fieldPiece.fieldEventInfo = gameManager._resourceManager.GetRandomFieldEvent();
        }
    }

    void printMap(FieldPiece[,] pieces){
        string arrayStr = "";
            for (int j = 0; j < fieldSizeList[currentFloor].y; j++)
            {
                for (int i = 0; i < fieldSizeList[currentFloor].x; i++)
                {
                    arrayStr += pieces[i,j].MapType + " ";
                }
                arrayStr += "\n";
            }
        Debug.Log(arrayStr);
    }
    public FieldPiece[,] GetFloorField(int floor){
        return AllFieldMapData[floor];
    }
    public FieldPiece[,] GetCurrentFloorField(){
        return AllFieldMapData[currentFloor];
    }
}


public class FieldPiece
{
    private MapType _mapType = MapType.Empty;
    public MapType MapType {
        get { return _mapType; }
        private set { _mapType = value; }
    }

    public void Init(Vector2Int _gridPosition, MapType type){
        gridPosition = _gridPosition;
        _mapType = type;
    }
    public void SetMapType(MapType type){
        _mapType = type;
    }

    public bool PrincessIsLight = false;
    public bool KnightIsLight = false;

    public Vector2Int gridPosition{private set; get;}

    public Monster monsterInfo;
    public FieldEventInfo fieldEventInfo;
    public ItemInfo itemInfo;

}