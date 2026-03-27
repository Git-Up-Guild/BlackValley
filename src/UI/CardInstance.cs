using Microsoft.Xna.Framework;

namespace BlackValley;

// 游戏中实际存在的一张卡牌实体

/// <summary>
/// 游戏中实际存在于手牌、牌库或弃牌堆中的一张“卡牌实体”。
/// 包含卡牌数据以及它在屏幕上的渲染状态。
/// </summary>
public class CardInstance
{
    public CardData Data { get; }      // 这张牌对应的底层数据模板
    public Rectangle Bounds { get; set; } // 这张牌当前在屏幕上的碰撞与绘制边框
    public bool IsHovered { get; set; } // 鼠标是否悬停其上（用于 UI 放大动画）

    public CardInstance(CardData data)
    {
        Data = data;
    }
}