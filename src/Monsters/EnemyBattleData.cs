namespace BlackValley.Monsters;

/// <summary>
/// 敌人的战斗配置数据
/// 用于描述敌人基础生命、每回合感染次数和强化规则
/// </summary>
public class EnemyBattleData
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string IconId { get; set; } = string.Empty;

    public int MaxHealth { get; set; }

    public int BaseInfectionsPerTurn { get; set; } = 1;

    public int EnrageIntervalTurns { get; set; } = 3;

    public int ExtraInfectionsPerEnrage { get; set; } = 1;

    public string Description { get; set; } = string.Empty;
}
