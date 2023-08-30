using System.Collections.Generic;
using UnityEngine;

public class FieldEvent : IEvent
{
    /// <summary>
    /// 텍스트
    /// </summary>
    public string Text { get; set; }
    public Sprite Sprite { get; set; }
    
    #region 선택지
    /// <summary>
    /// 선택지 (선택지과 인덱스 동일할 것)
    /// </summary>
    public string Question { get; set; }
    
    /// <summary>
    /// 확률 (선택지과 인덱스 동일할 것)
    /// </summary>
    public string Percent { get; set; }
    
    /// <summary>
    /// 응답 (선택지과 인덱스 동일할 것)
    /// </summary>
    public string Answer { get; set; }
    #endregion
    
    public void Execute()
    {
           
    }
}