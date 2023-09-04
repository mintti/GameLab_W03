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
    public Tilemap WallTileMap1;
    public Tilemap WallTileMap2;
    public Tilemap WallTileMap3;
    public Tilemap LightTileMap;
    public TileBase IsLightTile;
    public TileBase ItemTile;
    public TileBase EventTile;
    public TileBase MonsterTile;
    public TileBase HideTile;
    public TileBase HealTile;
    public TileBase CanSelectTile;
    public TileBase BlockTile;
    public TileBase EmptyTile;
    public TileBase DoorTile;
    public TileBase BossTile;
    public TileBase DragonTile;

    public Dictionary<int, FieldPiece[,]> AllFieldMapData = new Dictionary<int, FieldPiece[,]>();
    int floorCount = 0;
    int currentFloor = 0;
    Vector2Int currentHoverGrid;



    GameObject selectCusorObj;
    public List<FieldPiece> canSelectList = new List<FieldPiece>();
    public List<FieldPiece> KnightTempLight = new List<FieldPiece>(9);
    public List<FieldPiece> PrincessTempLight = new List<FieldPiece>(4);
    float cellSize = 1.28f;
    GeneratorManager generatorManager;



    public Vector3[] fieldFloorOffset;
    
    public bool LightCellMode { get; set; }

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
        PrincessTempLight.Add(AllFieldMapData[2][19,19]);
        PrincessTempLight.Add(AllFieldMapData[2][19,18]);
        PrincessTempLight.Add(AllFieldMapData[2][18,19]);
        PrincessTempLight.Add(AllFieldMapData[2][18,18]);
        currentFloor = 0;
    }
    
    public void MakeWallTileMap(){
        if(currentFloor == 0){
            for (int i = 0; i < _fieldSizeList[currentFloor].x + 2; i++)
            {
                string str = "";
                for (int j = 0; j < _fieldSizeList[currentFloor].y + 2; j++)
                {
                    if( WallTileMap2.GetTile(new Vector3Int(i, j, 0)) == null){
                        str +=  " null";    

                    }
                    else
                    str +=  " "  + WallTileMap2.GetTile(new Vector3Int(i, j, 0)).ToString();      
                }
                Debug.Log(str); 
            }
        }
        
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
        MapData[_fieldSizeList[floor].x-1, _fieldSizeList[floor].y-1].SetMapType(MapType.Door);
        Debug.Log("boss");
        while(true){
            int i = (int)(Random.value * _fieldSizeList[currentFloor].x);
            int j = (int)(Random.value * _fieldSizeList[currentFloor].y);
            if(MapData[i, j].MapType == MapType.Empty){
                MapData[i, j].SetMapType(MapType.Boss);
                if(floor == 0) MapData[i, j].monsterInfo = gameManager._resourceManager.Ruggle;
                else if(floor == 1) MapData[i, j].monsterInfo = gameManager._resourceManager.DeathKnight;
                break;
            }
        }
        if(floor == 2){ 
            MapData[19,19].SetMapType(MapType.Princess);
            MapData[19,18].SetMapType(MapType.Dragon);
            MapData[19,18].monsterInfo = gameManager._resourceManager.Dragon;
            MapData[18,19].SetMapType(MapType.Block);
            MapData[18,18].SetMapType(MapType.Block);
        }
        float remainRatio = 1-mapBlockRatio;
        int floorMonsterNum = 0;
        // create monster
        List<Vector3Int> floorRemainMonsterNum = new List<Vector3Int>();
        foreach(Vector3Int monsterNum in DataManager.Instance.monsterNumberPerFloor){
            if(monsterNum.x-1 == floor){
                floorMonsterNum += monsterNum.z;
                floorRemainMonsterNum.Add(monsterNum);
            }
        }
        float monsterRatio = (float)floorMonsterNum / (float)(_fieldSizeList[floor].x*_fieldSizeList[floor].y);
        while(floorMonsterNum != 0){
            for (int i = 0; i < _fieldSizeList[currentFloor].x; i++)
            {
                for (int j = 0; j < _fieldSizeList[currentFloor].y; j++)
                {
                    if((i == 0 && j == 0) || (i == _fieldSizeList[currentFloor].x -1 && j == _fieldSizeList[currentFloor].y -1)){
                        continue;
                    }
                    if(MapData[i, j].MapType == MapType.Empty){
                        float val = Random.value;
                        if(val < monsterRatio){
                            MapData[i,j].SetMapType(MapType.Monster);
                            int selectMonsterLevelIdx = Random.Range(0, floorRemainMonsterNum.Count);
                            while(floorRemainMonsterNum[selectMonsterLevelIdx].z == 0 && floorMonsterNum != 0){
                                selectMonsterLevelIdx = (selectMonsterLevelIdx + 1) % floorRemainMonsterNum.Count;
                            }
                            MapData[i,j].monsterInfo = gameManager._resourceManager.GetRandomMonster(floorRemainMonsterNum[selectMonsterLevelIdx].y);
                            floorRemainMonsterNum[selectMonsterLevelIdx] -= new Vector3Int(0,0,1);
                            floorMonsterNum -= 1;
                            if(floorMonsterNum == 0) break;
                        }
                    }
                }
                if(floorMonsterNum == 0) break;
            }
        }
        remainRatio -= mapMonsterRatio;

        GenerateFieldObjects(MapData, mapItemboxRatio/remainRatio, MapType.Item);
        remainRatio -= mapItemboxRatio;

        GenerateFieldObjects(MapData, mapEventRatio/remainRatio, MapType.Event);
        MapData[0, 0].SetMapType(MapType.Empty);


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

    private Vector2Int beforeVector;
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
                    FieldPiece fieldPiece = AllFieldMapData[currentFloor][grid.x, grid.y];
                    if(fieldPiece.IsLight || KnightTempLight.Contains(fieldPiece)){
                        if(fieldPiece.MapType == MapType.Monster){
                            _UIManager.TileInfUI(MapType.Monster, fieldPiece.monsterInfo);
                        }
                        else if(fieldPiece.MapType == MapType.Boss){
                            _UIManager.TileInfUI(MapType.Boss, fieldPiece.monsterInfo);
                        }
                        else if(fieldPiece.MapType == MapType.Dragon){
                            _UIManager.TileInfUI(MapType.Dragon, fieldPiece.monsterInfo);
                        }
                        else if(fieldPiece.MapType == MapType.Item){
                                _UIManager.TileInfUI(MapType.Item, null);
                        }
                        else if(fieldPiece.MapType == MapType.Event){
                            _UIManager.TileInfUI(MapType.Event, null);
                        }
                        else _UIManager.TileInfUI(MapType.Empty);
                        currentHoverGrid = grid;
                    }
                }

                // 공주 스킬 밝히기 사용 중, 사용 가능 스킬 범위를 표시 방법 변경
                if (LightCellMode)
                {
                    if (beforeVector != grid)
                    {
                        FieldPiece piece = AllFieldMapData[currentFloor][grid.x, grid.y];

                        var list = new List<FieldPiece>() { piece };

                        if (isInGrid(new Vector2Int(grid.x+1, grid.y))) list.Add(AllFieldMapData[currentFloor][grid.x + 1, grid.y]);
                        if (isInGrid(new Vector2Int(grid.x, grid.y+1))) list.Add(AllFieldMapData[currentFloor][grid.x, grid.y + 1]);
                        if (isInGrid(new Vector2Int(grid.x+1, grid.y+1))) list.Add(AllFieldMapData[currentFloor][grid.x + 1, grid.y + 1]);
                        
                        showCanSelectField(list); // 스킬 사용 시, 밝혀질 범위를 표시
                    }
                }
            }
            else{
                currentHoverGrid = new Vector2Int(-100, -100);
            }
        }
    }

    /// <summary>
    /// 공주의 밝히기 스킬 사용 가능 여부를 반환
    /// </summary>
    public bool CheckCanUsedLightSkill(FieldPiece piece)
    {
        bool result = false;
        Vector2Int vec = piece.gridPosition;
        
        result |= !piece.IsLight;
        if(isInGrid(new Vector2(vec.x + 1, vec.y))) result |= !AllFieldMapData[currentFloor][vec.x + 1, vec.y].IsLight;
        if(isInGrid(new Vector2(vec.x, vec.y + 1))) result |= !AllFieldMapData[currentFloor][vec.x, vec.y + 1].IsLight;
        if(isInGrid(new Vector2(vec.x + 1, vec.y + 1))) result |= !AllFieldMapData[currentFloor][vec.x + 1, vec.y + 1].IsLight;

        return result;
    }
    
    public void LightFieldPrincess(Vector2Int position){
        AllFieldMapData[currentFloor][position.x, position.y].IsLight = true;
        if(isInGrid(new Vector2Int(position.x, position.y+1)))AllFieldMapData[currentFloor][position.x, position.y+1].IsLight = true;
        if(isInGrid(new Vector2Int(position.x+1, position.y)))AllFieldMapData[currentFloor][position.x+1, position.y].IsLight = true;
        if(isInGrid(new Vector2Int(position.x+1, position.y+1)))AllFieldMapData[currentFloor][position.x+1, position.y+1].IsLight = true;
    }
    public void LightTempKnightMove(Vector2Int position){
        KnightTempLight.Clear();
        const int knightSeeRange = 1;
        for (int x = -knightSeeRange; x <= knightSeeRange; x++) {
            for (int y = -knightSeeRange; y <= knightSeeRange; y++) {
                int checkX = position.x + x;
                int checkY = position.y + y;
                if (isInGrid(new Vector2Int(checkX, checkY)))
                    KnightTempLight.Add(AllFieldMapData[currentFloor][checkX, checkY]);
            }
        }
        RefreshMap();
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
            if(isInGrid(piece.gridPosition)){
                UITileMap.SetTile(new Vector3Int(piece.gridPosition.x + 1, piece.gridPosition.y+ 1, 0), CanSelectTile);
                canSelectList.Add(AllFieldMapData[currentFloor][piece.gridPosition.x, piece.gridPosition.y]);
            }
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

    public void BuildAllField(){
        ClearAllMaps();
        
        WallTileMap.size = new Vector3Int(_fieldSizeList[currentFloor].x+2, _fieldSizeList[currentFloor].y+2, 0);
        WallTileMap.BoxFill(new Vector3Int(0, 0, 0), BlockTile, 0, 0, _fieldSizeList[currentFloor].x+2, _fieldSizeList[currentFloor].y+2 );
        FloorTileMap.size = new Vector3Int(_fieldSizeList[currentFloor].x+1, _fieldSizeList[currentFloor].y+1, 0);
        FloorTileMap.BoxFill(new Vector3Int(1, 1, 0), EmptyTile, 1, 1, _fieldSizeList[currentFloor].x+1, _fieldSizeList[currentFloor].y+1 );

        var typeToTile = new Dictionary<MapType, TileBase>() {
            {MapType.Block, BlockTile}, 
            {MapType.Item, ItemTile}, 
            {MapType.Empty, EmptyTile}, 
            {MapType.Monster, MonsterTile}, 
            {MapType.Event, EventTile}, 
            {MapType.Heal, HealTile}, 
            {MapType.Door, DoorTile}, 
            {MapType.Boss, BossTile}, 
            {MapType.Dragon, DragonTile}
        };

        NewBuildMap(FieldTileMap, typeToTile);
    }

    public void NewBuildMap(Tilemap map, Dictionary<MapType, TileBase> typeToTile)
    {
        for (int x = 0; x < _fieldSizeList[currentFloor].x; x++){
            for (int y = 0; y < _fieldSizeList[currentFloor].y; y++){
                var targetTile = AllFieldMapData[currentFloor][x, y];
                if (!typeToTile.ContainsKey(targetTile.MapType))
                    continue;

                var tile = typeToTile[targetTile.MapType];
                
                if(targetTile.IsLight){ 
                    LightTileMap.SetTile(new Vector3Int(x + 1, y + 1, 0), IsLightTile);    
                }
                else if(!KnightTempLight.Contains(targetTile) && !PrincessTempLight.Contains(targetTile)){
                    tile = HideTile;
                }

                map.SetTile(new Vector3Int(x + 1, y + 1, 0), tile);
            }
        }
    }
    
    public void BuildMap(MapType mapType, Tilemap map, TileBase tile)
    {
        for (int x = 0; x < _fieldSizeList[currentFloor].x; x++){
            for (int y = 0; y < _fieldSizeList[currentFloor].y; y++){
                if (AllFieldMapData[currentFloor][x, y].MapType == mapType)
                {
                    if(AllFieldMapData[currentFloor][x, y].IsLight){ 
                        LightTileMap.SetTile(new Vector3Int(x+1, y+1, 0), IsLightTile);    
                        map.SetTile(new Vector3Int(x+1, y+1, 0), tile);
                    }
                    else if(KnightTempLight.Contains(AllFieldMapData[currentFloor][x, y]) || PrincessTempLight.Contains(AllFieldMapData[currentFloor][x, y])){
                        map.SetTile(new Vector3Int(x+1, y+1, 0), tile);
                    }
                    else{
                        map.SetTile(new Vector3Int(x+1, y+1, 0), HideTile);
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
        LightTileMap.ClearAllTiles();
        FloorTileMap.ClearAllTiles();
        WallTileMap.ClearAllTiles();
    }

    public void RefreshMap(){
        BuildAllField();
    }

    public void UpdateMapType(FieldPiece fieldPiece, MapType type){
        fieldPiece.SetMapType(type);
        
        if(type == MapType.Item){
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