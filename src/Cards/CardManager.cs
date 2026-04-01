using System;
using System.Collections.Generic;

namespace BlackValley.Cards;
/// <summary>
/// 卡牌流转系统的核心管理器
/// 负责处理抽牌、弃牌、洗牌以及卡组初始化
/// </summary>
public class CardManager
{
    public List<CardData> DrawPile { get; private set; } = new();
    public List<CardData> DiscardPile { get; private set; } = new();
    public List<CardInstance> Hand { get; private set; } = new();

    private Random _rng = new Random();

    /// <summary>
    /// 根据静态卡牌配置初始化玩家牌库
    /// 会按 CopyCount 展开为实际牌组，并在开始前洗牌
    /// </summary>
    /// <param name="cards">当前可用的卡牌配置集合</param>
    public void InitializePlayerDeck(IEnumerable<CardData> cards)
    {
        DrawPile.Clear();
        DiscardPile.Clear();
        Hand.Clear();

        foreach (CardData card in cards)
        {
            if (!string.Equals(card.Owner, "Player", StringComparison.Ordinal))
            {
                continue;
            }

            for (int index = 0; index < card.CopyCount; index++)
            {
                DrawPile.Add(card);
            }
        }

        ShuffleDrawPile();
    }

    /// <summary>
    /// 初始化测试卡组
    /// 当前仅用于原型验证，后续需要改为从存档或 ModEntry 解析出的 Json 字典中构建真实牌库
    /// </summary>
    public void InitializeTestDeck()
    {
        DrawPile.Clear();
        DiscardPile.Clear();
        Hand.Clear();

        // 当前直接塞入固定测试牌，便于在没有完整配置流之前验证抽牌和出牌流程
        for (int i = 0; i < 4; i++)
        {
            // 攻击牌的目标是敌人
            DrawPile.Add(new CardData { Id = $"test_strike_{i}", Name = "Basic Strike", Owner = "Player", Type = "Attack", Cost = 1, TargetType = "Enemy", Damage = 6 });
        }
        for (int i = 0; i < 2; i++)
        {
            // 抽牌牌不需要目标，打出后直接抽 1 张
            DrawPile.Add(new CardData
            {
                Id = $"test_draw_{i}",
                Name = "Quick Draw",
                Owner = "Player",
                Type = "Draw",
                Cost = 0,
                TargetType = "None",
                DrawCount = 1
            });
        }
        for (int i = 0; i < 4; i++)
        {
            // 种子牌的目标是网格，形状为单格 [0,0]
            DrawPile.Add(new CardData
            {
                Id = $"test_seed_{i}",
                Name = "Parsnip Seed",
                Owner = "Player",
                Type = "Seed",
                Cost = 1,
                TargetType = "Grid_Empty",
                Shape = new List<List<int>> { new List<int> { 0, 0 } }
            });
        }
        for (int i = 0; i < 2; i++)
        {
            // 防御牌的目标是网格，形状为十字区域
            DrawPile.Add(new CardData
            {
                Id = $"test_cross_{i}",
                Name = "Cross Shield",
                Owner = "Player",
                Type = "Defense",
                Cost = 2,
                TargetType = "Grid_Any",
                Shape = new List<List<int>> {
                    new List<int>{0, 0}, new List<int>{0, -1}, new List<int>{0, 1}, new List<int>{-1, 0}, new List<int>{1, 0}
                }
            });
        }
    }

    // 使用 Fisher-Yates 洗牌，确保牌库顺序在每次初始化时是随机的
    private void ShuffleDrawPile()
    {
        for (int index = DrawPile.Count - 1; index > 0; index--)
        {
            int swapIndex = _rng.Next(index + 1);
            CardData temp = DrawPile[index];
            DrawPile[index] = DrawPile[swapIndex];
            DrawPile[swapIndex] = temp;
        }
    }

    /// <summary>
    /// 从牌库抽取指定数量的卡牌加入手牌
    /// 当抽牌堆为空时，会自动将弃牌堆洗回抽牌堆
    /// </summary>
    /// <param name="amount">尝试抽取的数量</param>
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            // 手牌达到上限后停止抽牌，避免越界写入手牌槽
            if (Hand.Count >= 5) break;

            // 当抽牌堆为空时，将弃牌堆洗回，保证长局对战仍能继续抽牌
            RefillDrawPileFromDiscardIfNeeded();
            if (DrawPile.Count == 0) break;

            // 当前按照列表头部视为牌库顶进行抽牌
            CardData drawnCard = DrawPile[0];
            DrawPile.RemoveAt(0);

            // 抽到的卡牌会包装成运行时实例后放入手牌
            Hand.Add(new CardInstance(drawnCard));
        }
    }

    /// <summary>
    /// 将当前手牌全部移动到弃牌堆
    /// 通常在回合结束时调用
    /// </summary>
    public void DiscardHand()
    {
        foreach (var card in Hand)
        {
            DiscardPile.Add(card.Data);
        }
        Hand.Clear();
    }

    // 当抽牌堆用尽时，将弃牌堆重新洗回抽牌堆
    private void RefillDrawPileFromDiscardIfNeeded()
    {
        if (DrawPile.Count > 0 || DiscardPile.Count == 0)
        {
            return;
        }

        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        ShuffleDrawPile();
    }
}
