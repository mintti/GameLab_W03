using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GeneratorManager : MonoBehaviour
{
    private Generator generator;
    //Common generator options
    public string[] mapGenerators = { "Tunneling", "CellularAutomata", "BSP", "Maze" };
    public int generatorIndex = 0;
    public int width, height;
    public Tilemap floor, wall;
    public TileBase floorTile, wallTile;
    public float chanceOfDecorate, chanceOfEnvironment;
    public List<TileBase> decorationsList, environmentsList;
    public bool placeDecor, placeEnvironment;
    //Enviroment generation options
    public int envDeathLimit, envBirthLimit, envNumberOfSteps;
    public float envChanceToStartAlive;
    //Tunneling options
    public int maxTunnels, maxTunnelLength, minTunnelLength, tunnelWidth;
    public int maxRRoomSize, minRRoomSize, maxCRoomRadius, minCRoomRadius;
    public bool buildRectRoom, buildCircleRoom, randomTunnelWidth;
    //Cellular options
    public int deathLimit, birthLimit, numberOfSteps;
    public float chanceToStartAlive;
    public bool fillBorder;
    //BSP options
    public int maxLeafSize, minLeafSize, hallsWidth;
    public bool randomHallWidth;
    //Maze options
    public float chanceOfEmptySpace;

    public bool[,] MapData;

    // private void Start() {
    //     MapData = new bool[height, width];
    // }
    public void GenerateNewMap(string mapType)
    {
        generator = new Generator();
        switch (mapType)
        {
            case "Tunneling":
                generator = new Tunneling(width, height, maxTunnels, maxTunnelLength, minTunnelLength, tunnelWidth,
                                       maxRRoomSize, minRRoomSize, maxCRoomRadius, minCRoomRadius,
                                       buildRectRoom, buildCircleRoom, randomTunnelWidth,
                                       floor, wall, floorTile, wallTile);
                generator.NewMap();
                break;
            case "CellularAutomata":
                generator = new Cellurar(width, height, deathLimit, birthLimit, numberOfSteps,
                                           chanceToStartAlive, fillBorder, floor, wall, floorTile, wallTile);
                generator.NewMap();
                break;
            case "BSP":
                generator = new BSPGenerator(width, height, maxLeafSize, minLeafSize,
                                           hallsWidth, randomHallWidth, floor, wall, floorTile, wallTile);
                generator.NewMap();
                break;
            case "Maze":
                generator = new MazeGenerator(width, height, chanceOfEmptySpace, floor, wall, floorTile, wallTile);
                generator.NewMap();
                
                MapData = (bool[,])generator.MapData.Clone();
                break;
        }
        if (placeDecor) PlaceDecorations();
        if (placeEnvironment) GenerateEnvironment();
    }

    public void ClearAllMaps()
    {
        var maps = FindObjectsOfType(typeof(Tilemap));
        foreach (Tilemap m in maps)
        {
            m.ClearAllTiles();
            // if (m.gameObject.CompareTag("EnvironmentMap") || m.gameObject.CompareTag("DecorMap")) GameObject.DestroyImmediate(m.gameObject);
        }
    }

    public void PlaceDecorations()
    {
        foreach (var d in decorationsList)
        {
            GameObject decMap = new GameObject("Decor map");
            decMap.tag = "DecorMap";
            decMap.transform.SetParent(this.transform);
            decMap.AddComponent<Tilemap>();
            decMap.AddComponent<TilemapRenderer>();
            var decTileMap = decMap.GetComponent<Tilemap>();

            foreach (var point in GetAvailablePoints())
                if (Random.Range(0f, 1f) < chanceOfDecorate) decTileMap.SetTile(point, d);            
        }
    }    

    public void GenerateEnvironment()
    {     
        foreach (var e in environmentsList)
        {
            GameObject envMap = new GameObject("Environment map");
            envMap.tag = "EnvironmentMap";
            envMap.transform.SetParent(this.transform);            
            envMap.AddComponent<Tilemap>();
            envMap.AddComponent<TilemapRenderer>();
            var envTileMap = envMap.GetComponent<Tilemap>();

            var cellularEnviroment = new Cellurar(width, height, envDeathLimit, envBirthLimit, envNumberOfSteps,
                                               envChanceToStartAlive, false, envTileMap, envTileMap, null, e);
            cellularEnviroment.InitialiseMap();
            cellularEnviroment.BuildMapInsideAnother(generator.MapData);
        }
    }

    List<Vector3Int> GetAvailablePoints()
    {
        var availablePoints = new List<Vector3Int>();
        var floorMap = floor.GetComponent<Tilemap>();
        for (int i = floorMap.cellBounds.xMin; i <= floorMap.cellBounds.xMax; i++)
            for (int j = floorMap.cellBounds.yMin; j <= floorMap.cellBounds.yMax; j++)
            {
                Vector3Int place = new Vector3Int(i, j, (int)floorMap.transform.position.z);
                if (floorMap.HasTile(place)) availablePoints.Add(place);
            }
        return availablePoints;
    }
}
