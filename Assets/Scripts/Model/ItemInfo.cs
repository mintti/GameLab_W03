using UnityEngine;
public class ItemInfo
{
    public EventType Type{ get; set; }
    public int EffectAmount{ get; set; }
    public Sprite Sprite { get; set; }
    public string Text { get; set; }

    public ItemInfo(EventType type, int effectAmount, Sprite sprite, string text)
    {
        
        Type = type;
        EffectAmount = effectAmount;
        Sprite = sprite;
        Text = text;
    }
}