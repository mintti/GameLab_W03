using System;
using UnityEngine;

public class Knight : MonoBehaviour
{
    public Status Status { get; set; }

    public int defaultHp;
    public int defaultPower;

    public void Start()
    {
        Status = new Status(defaultHp, defaultPower, true);
    }

    public void Buff()
    {
        Debug.Log("버프를 받았으나 구현이 안됬으니 구현 필요!!!!");
    }
}