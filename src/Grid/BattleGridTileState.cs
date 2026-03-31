namespace BlackValley.Grid;

/// <summary>
/// 单个战斗地块的运行时状态
/// 与卡牌、植物的静态配置分离，避免 UI 交互直接污染配置数据
/// </summary>
internal sealed class BattleGridTileState
{
    public bool IsInfected { get; set; }

    public string PlantId { get; set; } = string.Empty;

    public int GrowthTurnsRemaining { get; set; }

    public bool IsMature { get; set; }

    public int RemainingProtectionCharges { get; set; }

    public int TemporaryProtectionCharges { get; set; }

    public bool WasPlantedThisTurn { get; set; }

    public bool HasPlant => !string.IsNullOrEmpty(PlantId);
}
