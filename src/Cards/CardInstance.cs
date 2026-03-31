using Microsoft.Xna.Framework;

namespace BlackValley.Cards;

/// <summary>
/// 运行时的一张卡牌实体
/// 用于表示手牌、牌库或弃牌堆中的具体卡牌实例
/// </summary>
public class CardInstance
{
    public CardData Data { get; } // 当前卡牌对应的基础配置
    public Rectangle Bounds { get; set; } // 当前卡牌在屏幕上的碰撞与绘制区域
    public bool IsHovered { get; set; } // 当前卡牌是否处于鼠标悬停状态

    /// <summary>
    /// 根据基础卡牌配置创建一张运行时卡牌实例
    /// </summary>
    /// <param name="data">卡牌基础配置</param>
    public CardInstance(CardData data)
    {
        Data = data;
    }
}
