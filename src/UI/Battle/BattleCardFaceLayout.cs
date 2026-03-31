using Microsoft.Xna.Framework;

namespace BlackValley.UI.Battle;

/// <summary>
/// 一张卡牌在界面中的内部排版区域
/// 用于统一费用 名称 图标和描述的绘制矩形
/// </summary>
internal readonly struct BattleCardFaceLayout
{
    public Rectangle CardBounds { get; }
    public Rectangle CostBounds { get; }
    public Rectangle NameBounds { get; }
    public Rectangle IconBounds { get; }
    public Rectangle DescriptionBounds { get; }

    public BattleCardFaceLayout(
        Rectangle cardBounds,
        Rectangle costBounds,
        Rectangle nameBounds,
        Rectangle iconBounds,
        Rectangle descriptionBounds)
    {
        CardBounds = cardBounds;
        CostBounds = costBounds;
        NameBounds = nameBounds;
        IconBounds = iconBounds;
        DescriptionBounds = descriptionBounds;
    }
}
