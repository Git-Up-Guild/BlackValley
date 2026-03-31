using System;
using System.Collections.Generic;
using BlackValley.Cards;
using BlackValley.Grid;
using BlackValley.Plants;
using Microsoft.Xna.Framework;

namespace BlackValley.Battle;

/// <summary>
/// 植物结算模块
/// 负责种植、生长、收割和植物效果处理
/// </summary>
internal sealed class BattlePlantResolver
{
    private const string HarvestEffectTypeNone = "None";
    private const string HarvestEffectTypeAddAttackDamage = "AddAttackDamage";
    private const string HarvestEffectTypeReduceInfectionTier = "ReduceInfectionTier";
    private const string HarvestEffectTypeHealInfectedTiles = "HealInfectedTiles";

    private readonly BattleState _state;
    private readonly BattleFieldService _fieldService;
    private readonly IReadOnlyDictionary<string, PlantData> _plantDatabase;

    public BattlePlantResolver(
        BattleState state,
        BattleFieldService fieldService,
        IReadOnlyDictionary<string, PlantData> plantDatabase)
    {
        _state = state;
        _fieldService = fieldService;
        _plantDatabase = plantDatabase;
    }

    /// <summary>
    /// 尝试在指定地块种下一张种子牌
    /// </summary>
    /// <param name="card">当前打出的种子牌</param>
    /// <param name="tile">目标地块坐标</param>
    public bool TryPlantSeed(CardInstance card, Point tile)
    {
        if (!_plantDatabase.TryGetValue(card.Data.PlantId, out PlantData? plantData))
        {
            return false;
        }

        BattleGridTileState tileState = _state.FarmGrid[tile.X, tile.Y];
        if (tileState.HasPlant || tileState.IsInfected)
        {
            return false;
        }

        tileState.PlantId = plantData.Id;
        tileState.GrowthTurnsRemaining = plantData.GrowthTurnsRequired;
        tileState.IsMature = false;
        tileState.RemainingProtectionCharges = plantData.BaseProtectionCharges;
        tileState.WasPlantedThisTurn = true;
        return true;
    }

    /// <summary>
    /// 尝试收割指定地块上的成熟植物
    /// </summary>
    /// <param name="tile">目标地块坐标</param>
    public bool TryHarvestPlant(Point tile)
    {
        if (_state.IsBattleOver || _state.CurrentPhase != BattleTurnPhase.PlayerAction)
        {
            return false;
        }

        BattleGridTileState tileState = _state.FarmGrid[tile.X, tile.Y];
        if (!tileState.HasPlant || !tileState.IsMature)
        {
            return false;
        }

        if (!_plantDatabase.TryGetValue(tileState.PlantId, out PlantData? plantData))
        {
            return false;
        }

        ApplyHarvestEffect(tile, plantData);
        _fieldService.ClearPlant(tileState);
        return true;
    }

    /// <summary>
    /// 推进所有植物在回合结束时的生长
    /// 本回合刚种下的植物不会立刻生长
    /// </summary>
    public void AdvancePlantGrowth()
    {
        for (int column = 0; column < BattleRules.FieldColumnCount; column++)
        {
            for (int row = 0; row < BattleRules.FieldRowCount; row++)
            {
                BattleGridTileState tileState = _state.FarmGrid[column, row];
                if (!tileState.HasPlant)
                {
                    continue;
                }

                if (tileState.WasPlantedThisTurn)
                {
                    tileState.WasPlantedThisTurn = false;
                    continue;
                }

                if (tileState.IsMature)
                {
                    continue;
                }

                tileState.GrowthTurnsRemaining = Math.Max(0, tileState.GrowthTurnsRemaining - 1);
                tileState.IsMature = tileState.GrowthTurnsRemaining == 0;
            }
        }
    }

    // 收割效果由植物配置驱动，不直接在输入层写死具体规则
    private void ApplyHarvestEffect(Point centerTile, PlantData plantData)
    {
        if (string.Equals(plantData.HarvestEffectType, HarvestEffectTypeNone, StringComparison.Ordinal))
        {
            return;
        }

        if (string.Equals(plantData.HarvestEffectType, HarvestEffectTypeAddAttackDamage, StringComparison.Ordinal))
        {
            _state.BonusAttackDamageThisTurn += Math.Max(0, plantData.HarvestEffectValue);
            return;
        }

        if (string.Equals(plantData.HarvestEffectType, HarvestEffectTypeReduceInfectionTier, StringComparison.Ordinal))
        {
            _state.InfectionTierReductionThisTurn += Math.Max(0, plantData.HarvestEffectValue);
            return;
        }

        if (string.Equals(plantData.HarvestEffectType, HarvestEffectTypeHealInfectedTiles, StringComparison.Ordinal))
        {
            foreach (Point targetTile in _fieldService.GetShapeTargets(centerTile, plantData.HarvestShape))
            {
                _state.FarmGrid[targetTile.X, targetTile.Y].IsInfected = false;
            }

            _fieldService.SyncFarmHealth();
        }
    }
}
