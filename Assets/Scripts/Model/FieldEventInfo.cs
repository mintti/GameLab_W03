using UnityEngine;

public class FieldEventInfo
{
    public EventType Type{ get; set; }
    public int EffectAmount{ get; set; }
    public Sprite Sprite { get; set; }
    private string Text { get; set; }
    public string[] GetText => Text.Split("/");

    public FieldEventInfo(EventType type, int effectAmount, Sprite sprite, string text)
    {
        Type = type;
        EffectAmount = effectAmount;
        Sprite = sprite;
        Text = text;
    }
}

