using UnityEngine;

public class FieldEventInfo
{
    public Sprite Sprite { get; set; }
    private string Text { get; set; }
    public string[] GetText => Text.Split("/");
}