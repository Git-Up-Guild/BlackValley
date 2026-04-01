using Microsoft.Xna.Framework;

namespace BlackValley.UI.Battle;

/// <summary>
/// 战斗卡牌的视觉与排版配置
/// 统一维护卡面颜色 名称位置和换行阈值
/// </summary>
internal static class BattleCardStyle
{
    public static readonly Color PanelFillColor = new(236, 210, 136);
    public static readonly Color PanelBorderColor = new(122, 88, 39);
    public static readonly Color PanelShadowColor = new(63, 46, 22);
    public static readonly Color CostFillColor = new(220, 182, 77);
    public static readonly Color CostBorderColor = new(145, 104, 34);
    public static readonly Color IconFillColor = new(233, 206, 138);
    public static readonly Color IconBorderColor = new(137, 103, 53);

    public const int ContentHorizontalPadding = 10;
    public const int ContentTopPadding = 5;
    public const int ContentBottomPadding = 80;
    public const int SectionGap = 4;
    public const int IconDescriptionGap = 16;
    public const int IconInnerPadding = 3;
    public const int DescriptionMinHeight = 72;

    public const int NameHorizontalOffset = 25;
    public const int NameTopOffset = -8;
    public const int NameHeight = 46;
    public const float NameWrapWidthFactor = 0.8f;
    public const float DescriptionWrapWidthFactor = 1f;
}
