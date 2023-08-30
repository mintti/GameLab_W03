using System.Data;
using System.Reflection;
using UnityEngine;

public class FieldPiece : MonoBehaviour
{
    private GameManager _gameManager;
    private SpriteRenderer _renderer;

    private MapType _mapType;
    public MapType MapType => _mapType;
    
    public Sprite Sprite;

    public MapPiece MapPiece;
    public bool IsLight;

    public int X;
    public int Y;

    private bool _canSelect;

    public bool CanSelect
    {
        get => _canSelect;
        set
        {
            _canSelect = value;
        }
    }

    public int EventIndex { get; set; }

    public void Init(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void UpdateMapType(MapType type, int index = -1)
    {
        _mapType = type;
        MapPiece.MapType = type;

        EventIndex = index;
        
        // 타입정보에 따라 맵 오브젝트 업데이트 필요
    }
         

    public void OnMouseInput()
    {
        if (CanSelect)
        {
            _gameManager.ClickMap(this);
        }
    }   
}