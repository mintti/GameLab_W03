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
    public List<Vector2Int> _fieldSizeList;
    public float mapEmptyRatio = 0.35f;
    public float mapMonsterRatio = 0.25f;
    public float mapEventRatio = 0.2f;
    public float mapItemboxRatio = 0.1f;
    public float mapBlockRatio = 0.1f;

    
    [Header("Field")]
    public GameObject ObjectField;
    public Tilemap FieldTileMap;
    public Tilemap UITileMap;
    public Tilemap FloorTileMap;
    public Tilemap WallTileMap;
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
    Vector2Int currentHoverGrid;



    GameObject selectCusorObj;
    public List<FieldPiece> canSelectList = new List<FieldPiece>();
    float cellSize = 1.28f;
    GeneratorManager generatorManager;


    public FieldType currentField = FieldType.Field;

    public Vector3[] fieldFloorOffset;

    private void Awake() {
        selectCusorObj = Instantiate(Resources.Load<GameObject>("SelectCursorObject"));
    }

    public void InitMap(){
        _fieldSizeList = new List<Vector2Int>(DataManager.Instance.fieldSizeList);
        floorCount = _fieldSizeList.Count;
        fieldFloorOffset = new Vector3[floorCount];
        for(int i = 0; i < floorCount; ++i){
            currentFloor = i;
            if (!AllFieldMapData.ContainsKey(i))
                AllFieldMapData.Add(i, CreateMap(i, _fieldSizeList[i]));
            fieldFloorOffset[i] = new Vector3((20 -_fieldSizeList[i].x)/2, (20 -_fieldSizeList[i].x)/2, 0);
        }
        
        currentFloor = 0;

    }
    public FieldPiece GetFieldPiece(int floor, Vector2Int position){
            return AllFieldMapData[floor-1][position.x, position.y];
    }
    public FieldPiece GetFieldPiece(Vector2Int position){
            return AllFieldMapData[currentFloor][position.x, position.y];
    }
    public FieldPiece[,] CreateMap(int floor, Vector2Int fieldSize){

        generatorManager = generatorManagerObj.GetComponent<GeneratorManager>(); 
        generatorManager.width = fieldSize.x + 2;
        generatorManager.height = fieldSize.y + 2;
        generatorManager.chanceOfEmptySpace = 1- mapBlockRatio;
        generatorManager.GenerateNewMap("Maze"); 

        FieldPiece[,] MapData = new FieldPiece[fieldSize.x,fieldSize.y];
        for (int i = 0; i < fieldSize.x; i++)
        {
            for (int j = 0; j < fieldSize.y; j++)
            {
                MapData[i, j] = new FieldPiece();
                MapData[i, j].Init(floor, new Vector2Int(i, j), generatorManager.MapData[i + 1, j + 1] ? MapType.Block : MapType.Empty);                
            }
        }
        MapData[0, 0].SetMapType(MapType.Knight);
        while(true){
            int i = (int)(Random.value * _fieldSizeList[currentFloor].x);
            int j = (int)(Random.value * _fieldSizeList[currentFloor].y);
            if(MapData[i, j].MapType == MapType.Empty){
                MapData[i, j].SetMapType(MapType.Door);
                break;
            }
        }
        if(floor == 2){
            MapData[19,19].SetMapType(MapType.Princess);
            MapData[19,19].IsLight = true;
            MapData[19,18].SetMapType(MapType.Dragon);
            MapData[19,19].IsLight = true;
            MapData[18,19].SetMapType(MapType.Block);
            MapData[19,19].IsLight = true;
            MapData[18,18].SetMapType(MapType.Block);
            MapData[19,19].IsLight = true;
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
        for (int i = 0; i < _fieldSizeList[currentFloor].x; i++)
        {
            for (int j = 0; j < _fieldSizeList[currentFloor].y; j++)
            {
                if((i == 0 && j == 0) || (i == _fieldSizeList[currentFloor].x -1 && j == _fieldSizeList[currentFloor].y -1)){
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
                if(Input.GetMouseButtonDown(0)){
                    gameManager.ClickMap(AllFieldMapData[currentFloor][(int)grid.x, (int)grid.y]);
                }
                if(!grid.Equals(currentHoverGrid)){
                    PlaceSelectCursor(mousePosition, ObjectField.transform.position);
                    // Debug.Log(currentFloor);
                    FieldPiece fieldPiece = AllFieldMapData[currentFloor][grid.x, grid.y];
                    if(fieldPiece.IsLight){
                        if(fieldPiece.MapType == MapType.Monster){
                            _UIManager.TileInfUI(MapType.Monster, fieldPiece.monsterInfo);
                            // Debug.Log(fieldPiece.monsterInfo.Name);
                        }
                        else if(fieldPiece.MapType == MapType.Item){
                                _UIManager.TileInfUI(MapType.Item, null);
                                // Debug.Log(fieldPiece.itemInfo.Type);
                        }
                        else if(fieldPiece.MapType == MapType.Event){
                            _UIManager.TileInfUI(MapType.Event, null);
                            // Debug.Log(fieldPiece.fieldEventInfo.Type);
                        }
                        else _UIManager.TileInfUI(MapType.Empty);
                        currentHoverGrid = grid;
                    }
                }
            }
            else{
                currentHoverGrid = new Vector2Int(-100, -100);
            }
        }
    }
    public void LightField(FieldType type, Vector2Int position){
            AllFieldMapData[currentFloor][position.x, position.y].IsLight = true;
    }
    public void LightFieldKnightMove(Vector2Int position){
            AllFieldMapData[currentFloor][position.x, position.y].IsLight = true;
            if(isInGrid(new Vector2Int(position.x, position.y-1)))AllFieldMapData[currentFloor][position.x, position.y-1].IsLight = true;
            if(isInGrid(new Vector2Int(position.x, position.y+1)))AllFieldMapData[currentFloor][position.x, position.y+1].IsLight = true;
            if(isInGrid(new Vector2Int(position.x-1, position.y)))AllFieldMapData[currentFloor][position.x-1, position.y].IsLight = true;
            if(isInGrid(new Vector2Int(position.x-1, position.y-1)))AllFieldMapData[currentFloor][position.x-1, position.y-1].IsLight = true;
            if(isInGrid(new Vector2Int(position.x-1, position.y+1)))AllFieldMapData[currentFloor][position.x-1, position.y+1].IsLight = true;
            if(isInGrid(new Vector2Int(position.x+1, position.y)))AllFieldMapData[currentFloor][position.x+1, position.y].IsLight = true;
            if(isInGrid(new Vector2Int(position.x+1, position.y-1)))AllFieldMapData[currentFloor][position.x+1, position.y-1].IsLight = true;
            if(isInGrid(new Vector2Int(position.x+1, position.y+1)))AllFieldMapData[currentFloor][position.x+1, position.y+1].IsLight = true;
    }

    public void ChangeFloor(int floor){
        currentFloor = floor -1;
        ObjectField.transform.position = fieldFloorOffset[currentFloor] * cellSize;
        RefreshMap();
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
        if(gridPosition.x >= 0 && gridPosition.x < _fieldSizeList[currentFloor].x && gridPosition.y >= 0 && gridPosition.y < _fieldSizeList[currentFloor].y){
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
        ClearAllMaps();
        
        WallTileMap.size = new Vector3Int(_fieldSizeList[currentFloor].x+2, _fieldSizeList[currentFloor].y+2, 0);
        WallTileMap.BoxFill(new Vector3Int(0, 0, 0), BlockTile, 0, 0, _fieldSizeList[currentFloor].x+2, _fieldSizeList[currentFloor].y+2 );
        FloorTileMap.size = new Vector3Int(_fieldSizeList[currentFloor].x+1, _fieldSizeList[currentFloor].y+1, 0);
        FloorTileMap.BoxFill(new Vector3Int(1, 1, 0), EmptyTile, 1, 1, _fieldSizeList[currentFloor].x+1, _fieldSizeList[currentFloor].y+1 );
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
        for (int x = 0; x < _fieldSizeList[currentFloor].x; x++){
            for (int y = 0; y < _fieldSizeList[currentFloor].y; y++){
                if (AllFieldMapData[currentFloor][x, y].MapType == mapType)
                {
                    if(!AllFieldMapData[currentFloor][x, y].IsLight){ 
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
        generatorManager.ClearAllMaps();
        fieldPiece.SetMapType(MapType.Empty);
        RefreshMap();
    }
    public void ClearAllMaps(){
        
        FieldTileMap.ClearAllTiles();
        UITileMap.ClearAllTiles();
        FloorTileMap.ClearAllTiles();
        WallTileMap.ClearAllTiles();
    }

    public void RefreshMap(){
        ClearAllMaps();
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
            for (int j = 0; j < _fieldSizeList[currentFloor].y; j++)
            {
                for (int i = 0; i < _fieldSizeList[currentFloor].x; i++)
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

    public void Init(int _currentFloor, Vector2Int _gridPosition, MapType type){
        currentFloor = _currentFloor;
        gridPosition = _gridPosition;
        _mapType = type;
    }
    public void SetMapType(MapType type){
        _mapType = type;
    }

    public bool IsLight = false;

    public Vector2Int gridPosition{private set; get;}
    public int currentFloor{private set; get;}

    public Monster monsterInfo;
    public FieldEventInfo fieldEventInfo;
    public ItemInfo itemInfo;

}