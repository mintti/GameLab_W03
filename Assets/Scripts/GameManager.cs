using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CameraManager CameraManager { get; private set; }
    
    [Header("게임 관련")] 
    public bool GameEnd = false;
    
    [Header("설정 값")] 
    public Vector2 mapSize;
    public int emptyMapProportion;
    public int battleMapProportion; 
    public int eventMapProportion;
    public int boxMapProportion;
    public int blockMapProportion;
    
    [Header("필드 관련")]
    public Transform FieldGenTransform;
    private FieldPiece[,] field;

    [Header("맵 관련")]
    public Transform MapGenTransform;
    private MapPiece[,] map;

    [Header("플레이어")]
    public Player knight;
    public Player princess;
    
    public void Start()
    {
        CameraManager = GameObject.Find(nameof(CameraManager)).GetComponent<CameraManager>();
        Init();
    }

    /// <summary>
    /// 기본 설정 초기화
    /// </summary>
    private void Init()
    {
        CreateMap();
        SetPlayerPosition();
        StartCoroutine(nameof(PlayGame));
    }

    void CreateMap()
    {
        
    }

    void SetPlayerPosition()
    {
        
    }

    IEnumerator PlayGame()
    {
        while (true)
        {
            princess.StartTurn();
            CameraManager.Target = princess.transform;
            yield return new WaitUntil(() => princess.IsTurnEnd);
            
            knight.StartTurn();
            CameraManager.Target = knight.transform;
            yield return new WaitUntil(() => knight.IsTurnEnd);


            if (GameEnd)
            {
                // 게임이 종료되면 실행한다.
                // 왜 종료되었는 지는 각 오브젝트에서 설정해준다.
                yield break;
            }
        }
    }
}