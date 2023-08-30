using UnityEngine;
public class ItemInfo
{
    public Sprite Sprite { get; set; }
    public string Text { get; set; }

    public ItemInfo(Sprite sprite, string text)
    {
        Sprite = sprite;
        Text = text;
    }
}