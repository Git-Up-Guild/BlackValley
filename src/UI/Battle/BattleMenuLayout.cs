using BlackValley.Battle;
using Microsoft.Xna.Framework;

namespace BlackValley.UI.Battle;

/// <summary>
/// 统一维护战斗菜单的布局矩形
/// 让输入命中、拖拽预览和绘制逻辑共享同一套坐标来源，避免不同文件各自计算位置
/// </summary>
internal sealed class BattleMenuLayout
{
    // 主面板与战斗网格基础尺寸
    public const int PanelWidth = 1160;
    public const int PanelHeight = 780;
    public const int FieldRowCount = BattleRules.FieldRowCount;
    public const int FieldColumnCount = BattleRules.FieldColumnCount;
    public const int FieldTileSize = 64;
    public const int FieldTileGap = 4;

    // 顶部页头区域
    public const int HeaderTitleTopOffset = 40;
    public const int HeaderTurnLeftOffset = 48;
    public const int HeaderTurnTopOffset = 40;
    public const int FarmHealthTopOffset = 90;
    public const int FarmHealthWidth = 200;
    public const int FarmHealthHeight = 30;
    public const int FarmHealthTextVerticalOffset = 2;

    // 中间农田区域
    public const int FieldTopOffset = 140;

    // 左右角色区域
    public const int CharacterWidth = 128;
    public const int CharacterHeight = 212;
    public const int CharacterTopOffset = 20;
    public const int CharacterFieldGap = 52;
    public const int CharacterNameTopGap = 10;
    public const int CharacterNameHeight = 20;
    public const int PlayerModifierWidth = 150;
    public const int PlayerModifierHeight = 18;
    public const int PlayerModifierLeftGap = 20;
    public const int PlayerModifierTopOffset = 60;
    public const int PlayerModifierLineGap = 22;
    public const int EnemyHealthBarTopGap = 35;
    public const int EnemyHealthBarHeight = 20;

    // 怪物头顶意图区域
    public const int MonsterIntentTopGap = 12;
    public const int MonsterIntentHorizontalPadding = 12;
    public const int MonsterIntentMinWidth = 120;
    public const int MonsterIntentMinHeight = 30;

    // 右上角关闭按钮
    public const int CloseButtonTopOffset = 40;
    public const int CloseButtonRightOffset = 70;
    public const int CloseButtonSize = 40;

    // 底部手牌扇形区域
    public const int HandSlotCount = BattleRules.PlayerHandSize;
    private const int HandCardWidth = 178;
    private const int HandCardHeight = 256;
    private const int HandCardHorizontalStep = 108;
    private const int HandBottomMargin = 78;
    private const float HandRotationStepDegrees = 5f;

    // 底部左右资源区
    private const int SidePanelGap = 60;
    private const int StatusPanelSize = 96;
    private const int StatusPanelTopOffset = 0;
    private const int PilePanelTopOffset = 110;
    private const int EndTurnButtonWidth = 130;
    private const int EndTurnButtonHeight = 72;
    private const int EndTurnButtonTopOffset = 10;

    // 主面板和所有布局结果
    public Rectangle PanelBounds { get; }

    public Rectangle EndTurnButtonBounds { get; }

    public Rectangle CloseButtonBounds { get; }

    public Rectangle FieldAreaBounds { get; }

    public Rectangle FarmerBounds { get; }

    public Rectangle GhostBounds { get; }

    public Rectangle FarmerNameBounds { get; }

    public Rectangle EnemyNameBounds { get; }

    public Rectangle EnemyHealthBarBounds { get; }

    public Rectangle BonusAttackBounds { get; }

    public Rectangle InfectionReductionBounds { get; }

    public Rectangle EnergyBounds { get; }

    public Rectangle DrawPileBounds { get; }

    public Rectangle DiscardPileBounds { get; }

    public Rectangle FarmHealthBounds { get; }

    public Rectangle MonsterIntentBounds { get; }

    public int HeaderTitleY { get; }

    public Point HeaderTurnPosition { get; }

    public Rectangle[] HandSlotBounds { get; }

    /// <summary>
    /// 根据主面板矩形计算战斗菜单各个子区域的位置
    /// </summary>
    /// <param name="panelBounds">战斗菜单主面板区域</param>
    public BattleMenuLayout(Rectangle panelBounds)
    {
        PanelBounds = panelBounds;

        // 先确定顶部信息和中间农田主区域
        int fieldWidth = FieldColumnCount * FieldTileSize + (FieldColumnCount - 1) * FieldTileGap;
        int fieldHeight = FieldRowCount * FieldTileSize + (FieldRowCount - 1) * FieldTileGap;

        FieldAreaBounds = new Rectangle(
            PanelBounds.Center.X - fieldWidth / 2,
            PanelBounds.Top + FieldTopOffset,
            fieldWidth,
            fieldHeight);

        HeaderTitleY = PanelBounds.Top + HeaderTitleTopOffset;
        HeaderTurnPosition = new Point(PanelBounds.Left + HeaderTurnLeftOffset, PanelBounds.Top + HeaderTurnTopOffset);

        // 左右角色区域统一跟随农田位置计算
        FarmerBounds = new Rectangle(
            FieldAreaBounds.Left - CharacterFieldGap - CharacterWidth,
            FieldAreaBounds.Top + CharacterTopOffset,
            CharacterWidth,
            CharacterHeight);

        GhostBounds = new Rectangle(
            FieldAreaBounds.Right + CharacterFieldGap,
            FieldAreaBounds.Top + CharacterTopOffset,
            CharacterWidth,
            CharacterHeight);

        FarmerNameBounds = new Rectangle(
            FarmerBounds.X,
            FarmerBounds.Bottom + CharacterNameTopGap,
            FarmerBounds.Width,
            CharacterNameHeight);

        EnemyNameBounds = new Rectangle(
            GhostBounds.X,
            GhostBounds.Bottom + CharacterNameTopGap,
            GhostBounds.Width,
            CharacterNameHeight);

        EnemyHealthBarBounds = new Rectangle(
            GhostBounds.X,
            GhostBounds.Bottom + EnemyHealthBarTopGap,
            GhostBounds.Width,
            EnemyHealthBarHeight);

        BonusAttackBounds = new Rectangle(
            FarmerBounds.X - PlayerModifierLeftGap - PlayerModifierWidth,
            FarmerBounds.Y + PlayerModifierTopOffset,
            PlayerModifierWidth,
            PlayerModifierHeight);

        InfectionReductionBounds = new Rectangle(
            BonusAttackBounds.X,
            BonusAttackBounds.Y + PlayerModifierLineGap,
            PlayerModifierWidth,
            PlayerModifierHeight);

        // 关闭按钮固定在主面板右上角
        CloseButtonBounds = new Rectangle(
            PanelBounds.Right - CloseButtonRightOffset,
            PanelBounds.Top + CloseButtonTopOffset,
            CloseButtonSize,
            CloseButtonSize);

        // 手牌先确定，再用手牌整体宽度去推导左右资源区
        HandSlotBounds = CreateHandSlotBounds();

        Rectangle firstHandSlotBounds = HandSlotBounds[0];
        Rectangle lastHandSlotBounds = HandSlotBounds[^1];
        int handAreaStartX = firstHandSlotBounds.X;
        int handAreaEndX = lastHandSlotBounds.Right;
        int handAreaY = firstHandSlotBounds.Y;

        EnergyBounds = new Rectangle(
            handAreaStartX - SidePanelGap - StatusPanelSize,
            handAreaY + StatusPanelTopOffset,
            StatusPanelSize,
            StatusPanelSize);

        DrawPileBounds = new Rectangle(
            handAreaStartX - SidePanelGap - StatusPanelSize,
            handAreaY + PilePanelTopOffset,
            StatusPanelSize,
            StatusPanelSize);

        EndTurnButtonBounds = new Rectangle(
            handAreaEndX + SidePanelGap,
            handAreaY + EndTurnButtonTopOffset,
            EndTurnButtonWidth,
            EndTurnButtonHeight);

        DiscardPileBounds = new Rectangle(
            handAreaEndX + SidePanelGap + (EndTurnButtonWidth - StatusPanelSize) / 2,
            handAreaY + PilePanelTopOffset,
            StatusPanelSize,
            StatusPanelSize);

        FarmHealthBounds = new Rectangle(
            PanelBounds.Center.X - FarmHealthWidth / 2,
            PanelBounds.Top + FarmHealthTopOffset,
            FarmHealthWidth,
            FarmHealthHeight);

        // 怪物意图框根据怪物尺寸自动扩展，避免人物放大后头顶框显得太小
        int monsterIntentWidth = Math.Max(MonsterIntentMinWidth, GhostBounds.Width + MonsterIntentHorizontalPadding * 2);
        int monsterIntentHeight = Math.Max(MonsterIntentMinHeight, GhostBounds.Height / 3);
        MonsterIntentBounds = new Rectangle(
            GhostBounds.Center.X - monsterIntentWidth / 2,
            GhostBounds.Top - MonsterIntentTopGap - monsterIntentHeight,
            monsterIntentWidth,
            monsterIntentHeight);
    }

    /// <summary>
    /// 将屏幕坐标转换为战斗网格坐标
    /// </summary>
    /// <param name="x">屏幕空间 X 坐标</param>
    /// <param name="y">屏幕空间 Y 坐标</param>
    public Point? GetGridTileAtPosition(int x, int y)
    {
        if (!FieldAreaBounds.Contains(x, y))
        {
            return null;
        }

        int column = (x - FieldAreaBounds.X) / (FieldTileSize + FieldTileGap);
        int row = (y - FieldAreaBounds.Y) / (FieldTileSize + FieldTileGap);

        if (row < 0 || row >= FieldRowCount || column < 0 || column >= FieldColumnCount)
        {
            return null;
        }

        return new Point(column, row);
    }

    /// <summary>
    /// 获取指定地块的绘制区域
    /// </summary>
    /// <param name="column">目标列</param>
    /// <param name="row">目标行</param>
    public Rectangle GetGridTileBounds(int column, int row)
    {
        int drawX = FieldAreaBounds.X + column * (FieldTileSize + FieldTileGap);
        int drawY = FieldAreaBounds.Y + row * (FieldTileSize + FieldTileGap);

        return new Rectangle(drawX, drawY, FieldTileSize, FieldTileSize);
    }

    /// <summary>
    /// 根据卡牌外框计算费用 名称 图标和描述的内部版式
    /// </summary>
    /// <param name="cardBounds">卡牌整体绘制区域</param>
    public BattleCardFaceLayout GetCardFaceLayout(Rectangle cardBounds)
    {
        int horizontalPadding = Math.Max(12, cardBounds.Width / 10);
        int topPadding = Math.Max(12, cardBounds.Height / 20);
        int sectionGap = Math.Max(8, cardBounds.Height / 30);
        int costSize = Math.Clamp(cardBounds.Width / 4, 28, 36);
        int nameHeight = Math.Clamp(BattleCardStyle.NameHeight, 28, cardBounds.Height / 3);
        int nameHorizontalOffset = Math.Max(0, BattleCardStyle.NameHorizontalOffset);
        int iconWidth = cardBounds.Width - horizontalPadding * 2;
        int iconHeight = Math.Clamp(cardBounds.Height / 3, 52, 72);

        Rectangle costBounds = new Rectangle(
            cardBounds.X + horizontalPadding - 6,
            cardBounds.Y + topPadding - 6,
            costSize,
            costSize);

        Rectangle nameBounds = new Rectangle(
            cardBounds.X + horizontalPadding + nameHorizontalOffset,
            cardBounds.Y + topPadding + BattleCardStyle.NameTopOffset,
            Math.Max(24, cardBounds.Width - horizontalPadding * 2 - nameHorizontalOffset),
            nameHeight);

        Rectangle iconBounds = new Rectangle(
            cardBounds.X + horizontalPadding,
            nameBounds.Bottom + sectionGap,
            iconWidth,
            iconHeight);

        Rectangle descriptionBounds = new Rectangle(
            cardBounds.X + horizontalPadding,
            iconBounds.Bottom + sectionGap,
            cardBounds.Width - horizontalPadding * 2,
            Math.Max(40, cardBounds.Bottom - (iconBounds.Bottom + sectionGap) - topPadding));

        return new BattleCardFaceLayout(cardBounds, costBounds, nameBounds, iconBounds, descriptionBounds);
    }

    // 手牌区域由布局统一生成，避免输入命中和绘制使用不同的卡槽坐标
    private Rectangle[] CreateHandSlotBounds()
    {
        Rectangle[] handSlotBounds = new Rectangle[HandSlotCount];

        for (int index = 0; index < HandSlotCount; index++)
        {
            handSlotBounds[index] = GetHandCardBounds(HandSlotCount, index);
        }

        return handSlotBounds;
    }

    /// <summary>
    /// 获取当前手牌在扇形布局中的外框区域
    /// 会根据手牌数量重新居中，而不是固定占满五个卡槽
    /// </summary>
    /// <param name="cardCount">当前手牌数量</param>
    /// <param name="index">当前手牌索引</param>
    public Rectangle GetHandCardBounds(int cardCount, int index)
    {
        int safeCardCount = Math.Max(1, cardCount);
        int totalCardWidth = HandCardWidth + HandCardHorizontalStep * (safeCardCount - 1);
        int cardStartX = PanelBounds.Center.X - totalCardWidth / 2;
        int baseCardY = PanelBounds.Bottom - HandCardHeight - HandBottomMargin;
        float centerIndex = (safeCardCount - 1) / 2f;
        float distanceFromCenter = MathF.Abs(index - centerIndex);
        int yOffset = (int)MathF.Round(distanceFromCenter * 12f);

        return new Rectangle(
            cardStartX + index * HandCardHorizontalStep,
            baseCardY + yOffset,
            HandCardWidth,
            HandCardHeight);
    }

    /// <summary>
    /// 获取当前手牌在扇形布局中的旋转角度
    /// 左右两侧的卡会从中间向两边展开，中间卡保持竖直
    /// </summary>
    /// <param name="cardCount">当前手牌数量</param>
    /// <param name="index">当前手牌索引</param>
    public float GetHandCardRotationRadians(int cardCount, int index)
    {
        float centerIndex = (Math.Max(1, cardCount) - 1) / 2f;
        float rotationDegrees = (index - centerIndex) * HandRotationStepDegrees;
        return MathHelper.ToRadians(rotationDegrees);
    }
}
