using System.Collections.Generic;
using BlackValley.Grid;
using Microsoft.Xna.Framework;

namespace BlackValley.Battle;

/// <summary>
/// 战斗场地服务
/// 负责处理地块形状换算、农田生命同步和植物清理等公共网格逻辑
/// </summary>
internal sealed class BattleFieldService
{
    private readonly BattleState _state;

    public BattleFieldService(BattleState state)
    {
        _state = state;
    }

    /// <summary>
    /// 获取指定形状在某个中心点下的实际影响地块
    /// 会自动忽略所有越界格子
    /// </summary>
    /// <param name="centerTile">形状中心点</param>
    /// <param name="shape">相对坐标形状</param>
    public List<Point> GetShapeTargets(Point centerTile, List<List<int>> shape)
    {
        List<Point> targetTiles = new();
        if (shape.Count == 0)
        {
            return targetTiles;
        }

        foreach (List<int> offset in shape)
        {
            if (offset.Count < 2)
            {
                continue;
            }

            int targetColumn = centerTile.X + offset[0];
            int targetRow = centerTile.Y + offset[1];

            if (targetColumn < 0 || targetColumn >= BattleRules.FieldColumnCount
                || targetRow < 0 || targetRow >= BattleRules.FieldRowCount)
            {
                continue;
            }

            targetTiles.Add(new Point(targetColumn, targetRow));
        }

        return targetTiles;
    }

    /// <summary>
    /// 根据当前感染地块数量同步农田生命值
    /// </summary>
    public void SyncFarmHealth()
    {
        int infectedCount = 0;

        for (int column = 0; column < BattleRules.FieldColumnCount; column++)
        {
            for (int row = 0; row < BattleRules.FieldRowCount; row++)
            {
                if (_state.FarmGrid[column, row].IsInfected)
                {
                    infectedCount++;
                }
            }
        }

        _state.FarmHealth = _state.MaxFarmHealth - infectedCount;
    }

    /// <summary>
    /// 清空一个地块上的植物运行时状态
    /// 不会直接修改土地本身的感染状态
    /// </summary>
    /// <param name="tileState">目标地块状态</param>
    public void ClearPlant(BattleGridTileState tileState)
    {
        tileState.PlantId = string.Empty;
        tileState.GrowthTurnsRemaining = 0;
        tileState.IsMature = false;
        tileState.RemainingProtectionCharges = 0;
        tileState.WasPlantedThisTurn = false;
    }
}
