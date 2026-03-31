using System;
using System.Collections.Generic;
using BlackValley.Cards;
using BlackValley.Grid;
using BlackValley.Monsters;
using BlackValley.Plants;
using Microsoft.Xna.Framework;

namespace BlackValley.Battle;

/// <summary>
/// 战斗结算控制器
/// 负责对外暴露战斗操作入口，并编排植物、怪物和地块等规则模块
/// </summary>
internal sealed class BattleController
{
    private const string SeedCardType = "Seed";
    private const string DefenseCardType = "Defense";
    private const string BattleVictoryText = "Victory";
    private const string BattleDefeatText = "Farm Lost";

    private readonly EnemyBattleData _enemyData;
    private readonly BattleFieldService _fieldService;
    private readonly BattlePlantResolver _plantResolver;
    private readonly BattleMonsterResolver _monsterResolver;

    public BattleState State { get; }

    public EnemyBattleData EnemyData => _enemyData;

    public BattleController(
        IReadOnlyDictionary<string, CardData> cardDatabase,
        IReadOnlyDictionary<string, PlantData> plantDatabase,
        EnemyBattleData enemyData)
    {
        _enemyData = enemyData;

        CardManager cardManager = CreateCardManager(cardDatabase.Values);
        BattleGridTileState[,] farmGrid = CreateFarmGrid();

        State = new BattleState(
            cardManager,
            farmGrid,
            BattleRules.StartingEnergy,
            BattleRules.StartingFarmHealth,
            enemyData.MaxHealth);

        _fieldService = new BattleFieldService(State);
        _plantResolver = new BattlePlantResolver(State, _fieldService, plantDatabase);
        _monsterResolver = new BattleMonsterResolver(State, _fieldService, cardDatabase, enemyData);
        _fieldService.SyncFarmHealth();
        _monsterResolver.PrepareMonsterIntents();
        State.CurrentPhase = BattleTurnPhase.RoundIntro;
    }

    /// <summary>
    /// 尝试将一张攻击牌打到怪物身上
    /// 成功后会由控制器统一扣能量并移出手牌
    /// </summary>
    /// <param name="card">当前打出的手牌实例</param>
    public bool TryPlayCardOnEnemy(CardInstance card)
    {
        if (!CanPlayCard(card))
        {
            return false;
        }

        int totalDamage = Math.Max(0, card.Data.Damage + State.BonusAttackDamageThisTurn);
        State.GhostCurrentHp = Math.Max(0, State.GhostCurrentHp - totalDamage);
        CompleteCardPlay(card);
        TryResolveVictory();
        return true;
    }

    /// <summary>
    /// 尝试将一张网格牌打到指定地块
    /// 当前支持种子牌和防御牌
    /// </summary>
    /// <param name="card">当前打出的手牌实例</param>
    /// <param name="tile">目标地块坐标</param>
    public bool TryPlayCardOnGrid(CardInstance card, Point tile)
    {
        if (!CanPlayCard(card))
        {
            return false;
        }

        bool isValidTarget = false;
        if (string.Equals(card.Data.Type, SeedCardType, StringComparison.Ordinal))
        {
            isValidTarget = _plantResolver.TryPlantSeed(card, tile);
        }
        else if (string.Equals(card.Data.Type, DefenseCardType, StringComparison.Ordinal))
        {
            isValidTarget = TryApplyDefenseCard(card, tile);
        }

        if (!isValidTarget)
        {
            return false;
        }

        CompleteCardPlay(card);
        return true;
    }

    /// <summary>
    /// 尝试收割指定地块上的成熟植物
    /// </summary>
    /// <param name="tile">目标地块坐标</param>
    public bool TryHarvestPlant(Point tile)
    {
        int previousInfectionTierReduction = State.InfectionTierReductionThisTurn;
        bool harvestSucceeded = _plantResolver.TryHarvestPlant(tile);
        if (harvestSucceeded && State.InfectionTierReductionThisTurn != previousInfectionTierReduction)
        {
            _monsterResolver.RefreshPreparedMonsterIntents();
        }

        return harvestSucceeded;
    }

    /// <summary>
    /// 结束当前玩家回合并推进整轮结算
    /// </summary>
    public void EndTurn()
    {
        if (State.IsBattleOver || State.CurrentPhase != BattleTurnPhase.PlayerAction)
        {
            return;
        }

        State.CurrentPhase = BattleTurnPhase.ResolvingEndTurn;
        ResolveEndOfPlayerTurn();

        if (!State.IsBattleOver)
        {
            StartNextPlayerTurn();
        }
    }

    /// <summary>
    /// 获取卡牌在指定中心点下的实际影响格子
    /// 用于界面层绘制拖拽预览
    /// </summary>
    /// <param name="card">当前卡牌配置</param>
    /// <param name="centerTile">形状中心点</param>
    public List<Point> GetPreviewTargets(CardData card, Point centerTile)
    {
        return _fieldService.GetShapeTargets(centerTile, card.Shape);
    }

    /// <summary>
    /// 获取某个格子当前被多少个怪物意图覆盖
    /// 用于界面绘制预警
    /// </summary>
    /// <param name="column">目标列</param>
    /// <param name="row">目标行</param>
    public int GetMonsterIntentHitCount(int column, int row)
    {
        return _monsterResolver.GetMonsterIntentHitCount(column, row);
    }

    /// <summary>
    /// 回合开场动画结束后进入玩家可操作阶段
    /// </summary>
    public void CompleteRoundIntro()
    {
        if (State.IsBattleOver || State.CurrentPhase != BattleTurnPhase.RoundIntro)
        {
            return;
        }

        State.CurrentPhase = BattleTurnPhase.PlayerAction;
    }

    // 只有在玩家行动阶段，且能量足够时，才允许打出卡牌
    private bool CanPlayCard(CardInstance card)
    {
        return !State.IsBattleOver
            && State.CurrentPhase == BattleTurnPhase.PlayerAction
            && State.CurrentEnergy >= card.Data.Cost;
    }

    // 成功打牌后统一在这里扣费、移除手牌并进入弃牌堆
    private void CompleteCardPlay(CardInstance card)
    {
        State.CurrentEnergy -= card.Data.Cost;
        State.CardManager.Hand.Remove(card);
        State.CardManager.DiscardPile.Add(card.Data);
    }

    // 玩家结束行动后，依次推进植物生长、怪物感染和临时状态清理
    private void ResolveEndOfPlayerTurn()
    {
        _plantResolver.AdvancePlantGrowth();
        _monsterResolver.ExecuteMonsterTurn();
        ResetTurnTemporaryStates();
        TryResolveDefeat();
    }

    // 当前回合完整结算完后，统一在这里准备下一回合的手牌和怪物意图
    private void StartNextPlayerTurn()
    {
        State.CurrentTurn++;
        State.CurrentEnergy = State.MaxEnergy;
        State.CardManager.DiscardHand();
        State.CardManager.DrawCards(BattleRules.PlayerHandSize);
        State.CurrentPhase = BattleTurnPhase.RoundIntro;
        _monsterResolver.PrepareMonsterIntents();
    }

    // 玩家防御牌会给覆盖到的每个格子增加一次本回合有效的抵挡次数
    private bool TryApplyDefenseCard(CardInstance card, Point centerTile)
    {
        bool hasValidTarget = false;
        int protectionCharges = Math.Max(1, card.Data.ProtectionChargesPerTile);

        foreach (Point targetTile in _fieldService.GetShapeTargets(centerTile, card.Data.Shape))
        {
            State.FarmGrid[targetTile.X, targetTile.Y].TemporaryProtectionCharges += protectionCharges;
            hasValidTarget = true;
        }

        return hasValidTarget;
    }

    // 当前回合的攻击加成、感染降档和临时护盾都会在怪物回合后失效
    private void ResetTurnTemporaryStates()
    {
        State.BonusAttackDamageThisTurn = 0;
        State.InfectionTierReductionThisTurn = 0;

        for (int column = 0; column < BattleRules.FieldColumnCount; column++)
        {
            for (int row = 0; row < BattleRules.FieldRowCount; row++)
            {
                State.FarmGrid[column, row].TemporaryProtectionCharges = 0;
            }
        }
    }

    // 胜利在怪物血量归零时立即触发，不需要等到回合结束
    private void TryResolveVictory()
    {
        if (State.GhostCurrentHp <= 0)
        {
            SetBattleResult(BattleVictoryText);
        }
    }

    // 失败条件是 16 格全部感染
    private void TryResolveDefeat()
    {
        if (State.FarmHealth <= 0)
        {
            SetBattleResult(BattleDefeatText);
        }
    }

    // 战斗结束后停止继续生成意图或处理新的交互
    private void SetBattleResult(string resultText)
    {
        if (State.IsBattleOver)
        {
            return;
        }

        State.CurrentPhase = BattleTurnPhase.BattleEnded;
        State.BattleResultText = resultText;
        State.MonsterIntents.Clear();
    }

    // 优先使用 Json 配置构建真实玩家牌库
    // 当配置尚未准备好时，回退到原有的测试牌组，保证原型仍可运行
    private static CardManager CreateCardManager(IEnumerable<CardData> cards)
    {
        CardManager cardManager = new();

        using IEnumerator<CardData> enumerator = cards.GetEnumerator();
        if (enumerator.MoveNext())
        {
            cardManager.InitializePlayerDeck(cards);
        }
        else
        {
            cardManager.InitializeTestDeck();
        }

        cardManager.DrawCards(BattleRules.PlayerHandSize);
        return cardManager;
    }

    // 农田状态是运行时数据，进入菜单时统一重置，避免跨局残留
    private static BattleGridTileState[,] CreateFarmGrid()
    {
        BattleGridTileState[,] farmGrid = new BattleGridTileState[BattleRules.FieldColumnCount, BattleRules.FieldRowCount];

        for (int column = 0; column < BattleRules.FieldColumnCount; column++)
        {
            for (int row = 0; row < BattleRules.FieldRowCount; row++)
            {
                farmGrid[column, row] = new BattleGridTileState();
            }
        }

        return farmGrid;
    }
}
