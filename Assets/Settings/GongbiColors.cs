using UnityEngine;

/// <summary>
/// Traditional Chinese mineral-pigment (矿物颜料) colour palette
/// used in 工笔/重彩 (Gongbi / Zhongcai) painting.
/// Each colour includes a derived shadow tone for cel-shading.
/// </summary>
public static class GongbiColors
{
    // ── Walls / Structure ──
    public static readonly Color BeigeWall     = HexColor(0xE8, 0xD5, 0xB5); // 米色
    public static readonly Color OchreWall      = HexColor(0xC4, 0x9A, 0x6C); // 赭石
    public static readonly Color WhiteWall      = HexColor(0xF5, 0xF0, 0xE8); // 蛤粉白

    // ── Roofs ──
    public static readonly Color CinnabarRoof   = HexColor(0xC7, 0x3E, 0x3A); // 朱砂
    public static readonly Color DarkRedRoof     = HexColor(0x8B, 0x2C, 0x2C); // 深朱
    public static readonly Color AzuriteRoof     = HexColor(0x2B, 0x4C, 0x7E); // 石青
    public static readonly Color MalachiteRoof   = HexColor(0x2E, 0x8B, 0x57); // 石绿
    public static readonly Color GrayTileRoof    = HexColor(0x4A, 0x4A, 0x52); // 灰瓦

    // ── Wood / Pillars ──
    public static readonly Color DarkWood       = HexColor(0x5C, 0x33, 0x17); // 深褐木
    public static readonly Color Vermillion     = HexColor(0xD4, 0x3F, 0x3A); // 朱红

    // ── Accents ──
    public static readonly Color Gold           = HexColor(0xD4, 0xAF, 0x37); // 金
    public static readonly Color BrightRed      = HexColor(0xE6, 0x00, 0x12); // 大红
    public static readonly Color OrpimentYellow = HexColor(0xFF, 0xB1, 0x1B); // 雄黄

    // ── Ground / Stone ──
    public static readonly Color Bluestone      = HexColor(0x6B, 0x7B, 0x8A); // 青石
    public static readonly Color GrayStone      = HexColor(0x8B, 0x8B, 0x8B); // 灰石
    public static readonly Color DarkEarth      = HexColor(0x5A, 0x4A, 0x3A); // 深土

    // ── Foliage ──
    public static readonly Color EmeraldGreen   = HexColor(0x2E, 0xAF, 0x45); // 翠绿
    public static readonly Color DeepGreen      = HexColor(0x1A, 0x6E, 0x2B); // 深绿
    public static readonly Color BambooGreen    = HexColor(0x5C, 0xA8, 0x3E); // 竹青

    // ── Player ──
    public static readonly Color Indigo         = HexColor(0x3D, 0x5A, 0x80); // 花青

    // ── Outline ──
    public static readonly Color InkOutline     = HexColor(0x1A, 0x14, 0x10, 0.9f); // 墨

    // ── Lighting ──
    public static readonly Color WarmLight      = HexColor(0xFF, 0xF5, 0xE6); // warm key light
    public static readonly Color WarmAmbient   = HexColor(0xF5, 0xE6, 0xD3); // warm ambient (silk)

    /// <summary>Derive a shadow colour by darkening the main colour.</summary>
    public static Color Shadow(Color main, float factor = 0.55f)
    {
        return new Color(main.r * factor, main.g * factor, main.b * factor, main.a);
    }

    /// <summary>Derive a slightly darker shadow for warm-toned colours (bias toward warm shadow).</summary>
    public static Color WarmShadow(Color main, float factor = 0.55f)
    {
        return new Color(
            main.r * factor,
            main.g * factor * 0.92f,
            main.b * factor * 0.85f,
            main.a);
    }

    private static Color HexColor(int r, int g, int b, float a = 1f)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a);
    }
}
