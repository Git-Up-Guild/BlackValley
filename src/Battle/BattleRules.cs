namespace BlackValley.Battle;

/// <summary>
/// 战斗规则常量
/// 统一维护棋盘尺寸、初始费用和手牌上限等逻辑层共享配置
/// </summary>
internal static class BattleRules
{
    public const int FieldRowCount = 4;
    public const int FieldColumnCount = 4;
    public const int PlayerHandSize = 5;
    public const int StartingEnergy = 3;
    public const int StartingFarmHealth = 16;
    public const int FarmDefeatHealthThreshold = 3;
}
