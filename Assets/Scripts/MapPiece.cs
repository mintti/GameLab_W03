using System;
using UnityEngine;

/// <summary>
/// 지도 조각
/// </summary>
public class MapPiece : MonoBehaviour
{
    private SpriteRenderer _renderer;
    
    public MapType MapType;
    public Sprite Sprite;
    public bool IsLight;

    private bool _isClear;

    public bool IsClear
    {
        get => _isClear;
        set
        {
            _isClear = value;
            if (!_isClear)
            {
                Sprite = null;
            }
        }
    }

    public void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Init()
    {
        
    }
}