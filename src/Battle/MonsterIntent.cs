using BlackValley.Cards;
using Microsoft.Xna.Framework;

namespace BlackValley.Battle;

/// <summary>
/// 怪物即将执行的一次感染意图
/// 同时保存原始牌和当前已经解析好的实际结算牌
/// </summary>
internal sealed class MonsterIntent
{
    public CardData SourceCard { get; }

    public CardData? ResolvedCard { get; private set; }

    public Point CenterTile { get; }

    public MonsterIntent(CardData sourceCard, CardData? resolvedCard, Point centerTile)
    {
        SourceCard = sourceCard;
        ResolvedCard = resolvedCard;
        CenterTile = centerTile;
    }

    /// <summary>
    /// 更新当前意图在本回合中的实际感染牌
    /// </summary>
    /// <param name="resolvedCard">已经结合降档效果解析后的感染牌</param>
    public void SetResolvedCard(CardData? resolvedCard)
    {
        ResolvedCard = resolvedCard;
    }
}
