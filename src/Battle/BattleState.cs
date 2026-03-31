using System.Collections.Generic;
using BlackValley.Cards;
using BlackValley.Grid;

namespace BlackValley.Battle;

/// <summary>
/// 战斗运行时状态
/// 负责集中保存当前对局中的数值、牌堆、地块和怪物意图
/// </summary>
internal sealed class BattleState
{
    public CardManager CardManager { get; }

    public BattleGridTileState[,] FarmGrid { get; }

    public int GhostMaxHp { get; }

    public int MaxFarmHealth { get; }

    public int CurrentTurn { get; set; } = 1;

    public int CurrentEnergy { get; set; }

    public int MaxEnergy { get; }

    public int FarmHealth { get; set; }

    public int GhostCurrentHp { get; set; }

    public int BonusAttackDamageThisTurn { get; set; }

    public int InfectionTierReductionThisTurn { get; set; }

    public BattleTurnPhase CurrentPhase { get; set; } = BattleTurnPhase.PlayerAction;

    public string BattleResultText { get; set; } = string.Empty;

    public bool IsBattleOver => CurrentPhase == BattleTurnPhase.BattleEnded;

    public int MonsterIntentCount
    {
        get
        {
            int activeIntentCount = 0;

            foreach (MonsterIntent monsterIntent in MonsterIntents)
            {
                if (monsterIntent.ResolvedCard != null)
                {
                    activeIntentCount++;
                }
            }

            return activeIntentCount;
        }
    }

    internal List<MonsterIntent> MonsterIntents { get; } = new();

    public BattleState(
        CardManager cardManager,
        BattleGridTileState[,] farmGrid,
        int startingEnergy,
        int maxFarmHealth,
        int ghostMaxHp)
    {
        CardManager = cardManager;
        FarmGrid = farmGrid;
        MaxEnergy = startingEnergy;
        CurrentEnergy = startingEnergy;
        MaxFarmHealth = maxFarmHealth;
        FarmHealth = maxFarmHealth;
        GhostMaxHp = ghostMaxHp;
        GhostCurrentHp = ghostMaxHp;
    }
}
