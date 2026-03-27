using System;
using System.Collections.Generic;

namespace BlackValley;
/// <summary>
/// 卡牌流转系统的核心管理器，负责处理抽牌、弃牌、洗牌以及卡组的初始化。
/// </summary>
public class CardManager
{
    public List<CardData> DrawPile { get; private set; } = new();
    public List<CardData> DiscardPile { get; private set; } = new();
    public List<CardInstance> Hand { get; private set; } = new();

    private Random _rng = new Random();

    /// <summary>
    /// 初始化测试卡组。
    ///  TODO: [未实现功能] 后期需要由你的队友改为“从玩家存档/ModEntry解析的Json全局字典中”构建真实牌库。
    /// </summary>
    public void InitializeTestDeck()
    {
        DrawPile.Clear();
        DiscardPile.Clear();
        Hand.Clear();

        // 强行塞入一些不同类型的测试牌
        for (int i = 0; i < 4; i++)
        {
            // 攻击牌：目标是敌人
            DrawPile.Add(new CardData { Id = $"test_strike_{i}", Name = "普通攻击", Type = "Attack", Cost = 1, TargetType = "Enemy", Damage = 6 });
        }
        for (int i = 0; i < 4; i++)
        {
            // 种子牌：目标是格子，形状是单格[0,0]
            DrawPile.Add(new CardData
            {
                Id = $"test_seed_{i}",
                Name = "防风草种子",
                Type = "Seed",
                Cost = 1,
                TargetType = "Grid_Empty",
                Shape = new List<List<int>> { new List<int> { 0, 0 } }
            });
        }
        for (int i = 0; i < 2; i++)
        {
            // 防御牌：目标是格子，形状是十字形 (中心、上、下、左、右)
            DrawPile.Add(new CardData
            {
                Id = $"test_cross_{i}",
                Name = "十字护盾",
                Type = "Skill",
                Cost = 2,
                TargetType = "Grid_Any",
                Shape = new List<List<int>> {
                    new List<int>{0, 0}, new List<int>{0, -1}, new List<int>{0, 1}, new List<int>{-1, 0}, new List<int>{1, 0}
                }
            });
        }
    }

    /// <summary>
    /// 从牌库抽取指定数量的卡牌加入手牌。
    /// TODO:[未实现功能] 当牌库数量不足时，需要将弃牌堆(DiscardPile)洗牌并重新加入牌库(DrawPile)。
    /// </summary>
    /// <param name="amount">尝试抽取的数量</param>
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            // 如果手牌满了5张，就不抽了
            if (Hand.Count >= 5) break;

            // 如果牌库空了，就洗牌 (这里先简化，以后再写洗牌逻辑)
            if (DrawPile.Count == 0) break;

            // 从牌库顶抽一张
            CardData drawnCard = DrawPile[0];
            DrawPile.RemoveAt(0);

            // 放入手牌
            Hand.Add(new CardInstance(drawnCard));
        }
    }

    /// <summary>
    /// 将当前手牌全部移动到弃牌堆（通常在回合结束时调用）。
    /// </summary>
    public void DiscardHand()
    {
        foreach (var card in Hand)
        {
            DiscardPile.Add(card.Data);
        }
        Hand.Clear();
    }
}