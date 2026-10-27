using System.Collections.Generic;
using UnityEngine;

public static class ThemeAssets
{
    public const bool Styled = false;
    private static readonly Dictionary<string, Sprite> RuntimeSprites = new Dictionary<string, Sprite>();

    public static Sprite Sprite(string name)
    {
        string theme = Styled ? "Final" : "Prototype";
        Sprite sprite = Resources.Load<Sprite>($"Art/{theme}/{name}");
        return sprite != null ? sprite : CreateFallback(name);
    }

    public static Color Background => Styled ? new Color(0.035f, 0.055f, 0.09f) : new Color(0.10f, 0.12f, 0.15f);
    public static Color Panel => Styled ? new Color(0.09f, 0.12f, 0.18f, 0.96f) : new Color(0.16f, 0.18f, 0.22f, 0.96f);
    public static Color Cyan => new Color(0.20f, 0.80f, 1.00f);
    public static Color Yellow => new Color(1.00f, 0.82f, 0.16f);
    public static Color Red => new Color(1.00f, 0.24f, 0.28f);
    public static Color Green => new Color(0.24f, 0.88f, 0.48f);

    private static Sprite CreateFallback(string name)
    {
        if (RuntimeSprites.TryGetValue(name, out Sprite cached))
        {
            return cached;
        }

        Color color = Color.white;
        if (name.Contains("player")) color = Cyan;
        else if (name.Contains("crystal")) color = Yellow;
        else if (name.Contains("hazard")) color = Red;
        else if (name.Contains("open")) color = Green;
        else if (name.Contains("closed")) color = new Color(0.55f, 0.60f, 0.68f);
        else if (name.Contains("background")) color = Background;

        Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        texture.name = $"Fallback_{name}";
        Sprite created = UnityEngine.Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        created.name = $"Fallback_{name}";
        RuntimeSprites[name] = created;
        return created;
    }
}

