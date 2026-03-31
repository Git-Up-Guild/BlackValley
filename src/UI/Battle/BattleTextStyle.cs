using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace BlackValley.UI.Battle;

/// <summary>
/// 战斗界面中的一类文字样式
/// 统一描述字体资源和缩放倍率
/// </summary>
internal readonly struct BattleTextStyle
{
    public SpriteFont Font { get; }

    public float Scale { get; }

    public BattleTextStyle(SpriteFont font, float scale)
    {
        Font = font;
        Scale = scale;
    }
}

/// <summary>
/// 战斗界面文字样式配置
/// 统一维护不同模块显示文字的字体和大小
/// </summary>
internal static class BattleTextStyles
{
    // 这套字体资源里 tinyFont 视觉上不一定比 smallFont 更小
    // 需要更紧凑的文字时，优先用带像素对齐的 smallFont 保守缩放
    private static BattleTextStyle Dialogue(float scale = 1f) => new(Game1.dialogueFont, scale);
    private static BattleTextStyle Small(float scale = 1f) => new(Game1.smallFont, scale);
    private static BattleTextStyle Compact(float scale = 0.85f) => new(Game1.smallFont, scale);

    public static BattleTextStyle HeaderTitle => Dialogue();
    public static BattleTextStyle HeaderTurn => Small();
    public static BattleTextStyle HeaderFarmHealth => Compact(0.82f);

    public static BattleTextStyle FieldPlantState => Compact(0.82f);
    public static BattleTextStyle FieldPlantProtection => Compact(0.82f);
    public static BattleTextStyle FieldTemporaryProtection => Compact(0.82f);

    public static BattleTextStyle CharacterName => Small();
    public static BattleTextStyle EnemyHealth => Compact(0.84f);
    public static BattleTextStyle MonsterIntent => Compact(0.9f);

    public static BattleTextStyle EnergyValue => Dialogue();
    public static BattleTextStyle PileCount => Compact(0.78f);
    public static BattleTextStyle TurnModifier => Compact(0.84f);

    public static BattleTextStyle CardCost => Small();
    public static BattleTextStyle CardName => Compact(0.78f);
    public static BattleTextStyle CardDescription => Compact(0.78f);
    public static BattleTextStyle CardFallbackIcon => Compact(0.84f);

    public static BattleTextStyle EndTurnButton => Compact(0.9f);
    public static BattleTextStyle CloseButton => Small();

    public static BattleTextStyle BattleResultTitle => Dialogue();
    public static BattleTextStyle BattleResultHint => Small();

    public static BattleTextStyle RoundIntro => Dialogue(1.5f);
}
