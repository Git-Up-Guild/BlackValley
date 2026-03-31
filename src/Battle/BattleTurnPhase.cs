namespace BlackValley.Battle;

/// <summary>
/// 战斗回合阶段
/// 用于限制当前可执行的输入和结算流程
/// </summary>
internal enum BattleTurnPhase
{
    RoundIntro,
    PlayerAction,
    ResolvingEndTurn,
    BattleEnded
}
