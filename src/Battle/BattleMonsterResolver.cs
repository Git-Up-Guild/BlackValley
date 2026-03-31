using System;
using System.Collections.Generic;
using BlackValley.Cards;
using BlackValley.Grid;
using BlackValley.Monsters;
using Microsoft.Xna.Framework;

namespace BlackValley.Battle;

/// <summary>
/// 怪物结算模块
/// 负责怪物意图生成、感染降档、感染结算和怪物感染牌堆流转
/// </summary>
internal sealed class BattleMonsterResolver
{
    private const string InfectionCardType = "Infection";
    private const string EnemyOwner = "Enemy";

    private readonly BattleState _state;
    private readonly BattleFieldService _fieldService;
    private readonly IReadOnlyDictionary<string, CardData> _cardDatabase;
    private readonly EnemyBattleData _enemyData;
    private readonly Random _random = new();
    private readonly List<CardData> _enemyInfectionDrawPile;
    private readonly List<CardData> _enemyInfectionDiscardPile = new();

    public BattleMonsterResolver(
        BattleState state,
        BattleFieldService fieldService,
        IReadOnlyDictionary<string, CardData> cardDatabase,
        EnemyBattleData enemyData)
    {
        _state = state;
        _fieldService = fieldService;
        _cardDatabase = cardDatabase;
        _enemyData = enemyData;
        _enemyInfectionDrawPile = CreateEnemyInfectionDrawPile();
    }

    /// <summary>
    /// 生成当前玩家回合可见的怪物感染意图
    /// </summary>
    public void PrepareMonsterIntents()
    {
        _state.MonsterIntents.Clear();

        List<Point> availableCenters = GetUninfectedTiles();
        int infectionCount = GetMonsterInfectionsPerTurn();
        for (int index = 0; index < infectionCount && availableCenters.Count > 0; index++)
        {
            CardData infectionCard = DrawEnemyInfectionCard();
            int centerIndex = _random.Next(availableCenters.Count);
            Point centerTile = availableCenters[centerIndex];

            availableCenters.RemoveAt(centerIndex);
            _state.MonsterIntents.Add(new MonsterIntent(infectionCard, ResolveInfectionCard(infectionCard), centerTile));
        }
    }

    /// <summary>
    /// 在虚弱等临时效果改变后，刷新当前回合已经显示出来的怪物意图
    /// 这样预览和实际结算会稳定使用同一份降档结果
    /// </summary>
    public void RefreshPreparedMonsterIntents()
    {
        foreach (MonsterIntent monsterIntent in _state.MonsterIntents)
        {
            monsterIntent.SetResolvedCard(ResolveInfectionCard(monsterIntent.SourceCard));
        }
    }

    /// <summary>
    /// 执行当前回合全部怪物感染意图
    /// </summary>
    public void ExecuteMonsterTurn()
    {
        foreach (MonsterIntent monsterIntent in _state.MonsterIntents)
        {
            ExecuteMonsterIntent(monsterIntent);
            if (_state.FarmHealth <= BattleRules.FarmDefeatHealthThreshold)
            {
                break;
            }
        }

        _state.MonsterIntents.Clear();
    }

    /// <summary>
    /// 获取某个格子当前被多少个怪物意图覆盖
    /// </summary>
    /// <param name="column">目标列</param>
    /// <param name="row">目标行</param>
    public int GetMonsterIntentHitCount(int column, int row)
    {
        int hitCount = 0;
        Point targetTile = new(column, row);

        foreach (MonsterIntent monsterIntent in _state.MonsterIntents)
        {
            if (monsterIntent.ResolvedCard == null)
            {
                continue;
            }

            foreach (Point affectedTile in _fieldService.GetShapeTargets(monsterIntent.CenterTile, monsterIntent.ResolvedCard.Shape))
            {
                if (affectedTile == targetTile)
                {
                    hitCount++;
                }
            }
        }

        return hitCount;
    }

    // 强化从满足回合数的那个回合开始生效
    private int GetMonsterInfectionsPerTurn()
    {
        if (_enemyData.EnrageIntervalTurns <= 0)
        {
            return _enemyData.BaseInfectionsPerTurn;
        }

        int enrageStacks = _state.CurrentTurn / _enemyData.EnrageIntervalTurns;
        return _enemyData.BaseInfectionsPerTurn + enrageStacks * _enemyData.ExtraInfectionsPerEnrage;
    }

    // 杨桃收割带来的降档会统一作用于本回合怪物的每一次感染
    private void ExecuteMonsterIntent(MonsterIntent monsterIntent)
    {
        if (monsterIntent.ResolvedCard == null)
        {
            return;
        }

        foreach (Point targetTile in _fieldService.GetShapeTargets(monsterIntent.CenterTile, monsterIntent.ResolvedCard.Shape))
        {
            ApplyInfectionToTile(targetTile);
            if (_state.FarmHealth <= BattleRules.FarmDefeatHealthThreshold)
            {
                return;
            }
        }
    }

    // 当前的感染降档是临时效果，因此在真正结算或预览时再解析
    private CardData? ResolveInfectionCard(CardData sourceCard)
    {
        CardData? resolvedCard = sourceCard;

        for (int step = 0; step < _state.InfectionTierReductionThisTurn && resolvedCard != null; step++)
        {
            resolvedCard = DowngradeInfectionCard(resolvedCard);
        }

        return resolvedCard;
    }

    // 五格降到横三或竖三，三格再降到单格，单格继续降则视为本次不感染
    private CardData? DowngradeInfectionCard(CardData sourceCard)
    {
        if (sourceCard.DowngradeCardIds.Count == 0)
        {
            return null;
        }

        List<CardData> candidateCards = new();
        foreach (string cardId in sourceCard.DowngradeCardIds)
        {
            if (_cardDatabase.TryGetValue(cardId, out CardData? cardData))
            {
                candidateCards.Add(cardData);
            }
        }

        if (candidateCards.Count == 0)
        {
            return null;
        }

        int targetIndex = _random.Next(candidateCards.Count);
        return candidateCards[targetIndex];
    }

    // 感染会优先被本回合护盾拦下，其次由地块上的作物代替土地承受
    private void ApplyInfectionToTile(Point targetTile)
    {
        BattleGridTileState tileState = _state.FarmGrid[targetTile.X, targetTile.Y];
        if (tileState.IsInfected)
        {
            return;
        }

        if (tileState.TemporaryProtectionCharges > 0)
        {
            tileState.TemporaryProtectionCharges--;
            return;
        }

        if (tileState.PersistentProtectionCharges > 0)
        {
            tileState.PersistentProtectionCharges--;
            return;
        }

        if (tileState.HasPlant)
        {
            if (tileState.RemainingProtectionCharges > 1)
            {
                tileState.RemainingProtectionCharges--;
                return;
            }

            _fieldService.ClearPlant(tileState);
            return;
        }

        tileState.IsInfected = true;
        _fieldService.SyncFarmHealth();
    }

    // 敌人感染牌使用独立牌堆，空了之后再将弃牌堆洗回
    private CardData DrawEnemyInfectionCard()
    {
        RefillEnemyInfectionDrawPileIfNeeded();
        if (_enemyInfectionDrawPile.Count == 0)
        {
            return CreateFallbackInfectionCard();
        }

        CardData cardData = _enemyInfectionDrawPile[0];
        _enemyInfectionDrawPile.RemoveAt(0);
        _enemyInfectionDiscardPile.Add(cardData);
        return cardData;
    }

    // 优先从 Json 配置表构建敌人的感染牌堆
    private List<CardData> CreateEnemyInfectionDrawPile()
    {
        List<CardData> drawPile = new();

        foreach (CardData cardData in _cardDatabase.Values)
        {
            if (!string.Equals(cardData.Owner, EnemyOwner, StringComparison.Ordinal)
                || !string.Equals(cardData.Type, InfectionCardType, StringComparison.Ordinal))
            {
                continue;
            }

            for (int index = 0; index < Math.Max(1, cardData.CopyCount); index++)
            {
                drawPile.Add(cardData);
            }
        }

        if (drawPile.Count == 0)
        {
            drawPile.Add(CreateFallbackInfectionCard());
        }

        ShuffleCards(drawPile);
        return drawPile;
    }

    // 当敌人抽牌堆耗尽时，将已经打出的感染牌重新洗回
    private void RefillEnemyInfectionDrawPileIfNeeded()
    {
        if (_enemyInfectionDrawPile.Count > 0 || _enemyInfectionDiscardPile.Count == 0)
        {
            return;
        }

        _enemyInfectionDrawPile.AddRange(_enemyInfectionDiscardPile);
        _enemyInfectionDiscardPile.Clear();
        ShuffleCards(_enemyInfectionDrawPile);
    }

    // 如果配置缺失，至少保证怪物还能执行最基础的单格感染
    private static CardData CreateFallbackInfectionCard()
    {
        return new CardData
        {
            Id = "fallback_enemy_infection_single",
            Name = "Single Infection",
            Owner = EnemyOwner,
            Type = InfectionCardType,
            CopyCount = 1,
            TargetType = "Grid_UninfectedCenter",
            ShapeId = "Single",
            InfectionTier = 1,
            Shape = new List<List<int>>
            {
                new() { 0, 0 }
            }
        };
    }

    // 预生成意图时只会选择当前未感染的格子作为中心
    private List<Point> GetUninfectedTiles()
    {
        List<Point> availableTiles = new();

        for (int column = 0; column < BattleRules.FieldColumnCount; column++)
        {
            for (int row = 0; row < BattleRules.FieldRowCount; row++)
            {
                if (_state.FarmGrid[column, row].IsInfected)
                {
                    continue;
                }

                availableTiles.Add(new Point(column, row));
            }
        }

        return availableTiles;
    }

    // 通用洗牌逻辑同时复用给敌人感染牌堆
    private void ShuffleCards<T>(List<T> cards)
    {
        for (int index = cards.Count - 1; index > 0; index--)
        {
            int swapIndex = _random.Next(index + 1);
            (cards[index], cards[swapIndex]) = (cards[swapIndex], cards[index]);
        }
    }
}
