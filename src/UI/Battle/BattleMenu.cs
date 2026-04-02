using System.Collections.Generic;
using BlackValley;
using BlackValley.Battle;
using BlackValley.Cards;
using BlackValley.Monsters;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace BlackValley.UI.Battle;

/// <summary>
/// 战斗菜单主入口
/// 负责持有当前战斗界面的运行时状态，并将输入、绘制拆分到对应 partial 文件中
/// </summary>
public sealed partial class BattleMenu : IClickableMenu
{
    private const int DefaultEnemyHealth = 12;
    private const string DefaultEnemyId = "Ghost";
    private const float BattleResultAutoCloseDelaySeconds = 2f;

    private const string EnemyTargetType = "Enemy";
    private const string GridTargetPrefix = "Grid";
    private const string NoTargetType = "None";
    private const string DefenseCardType = "Defense";

    private readonly BattleAssets _battleAssets;
    private readonly BattleMenuLayout _layout;
    private readonly BattleController _battleController;
    private readonly Action<bool>? _onBattleResolved;

    private CardInstance? _draggedCard;
    private bool _hasAppliedBattleResultEffects;
    private float _battleResultElapsedSeconds;

    /// <summary>
    /// 创建战斗菜单，并初始化布局和战斗控制器
    /// </summary>
    /// <param name="battleAssets">战斗界面依赖的贴图资源</param>
    /// <param name="onBattleResolved">战斗结果确定后调用的回调，参数表示玩家是否获胜</param>
    public BattleMenu(BattleAssets battleAssets, Action<bool>? onBattleResolved = null)
        : base(
            (Game1.uiViewport.Width - BattleMenuLayout.PanelWidth) / 2,
            (Game1.uiViewport.Height - BattleMenuLayout.PanelHeight) / 2,
            BattleMenuLayout.PanelWidth,
            BattleMenuLayout.PanelHeight,
            showUpperRightCloseButton: false)
    {
        _battleAssets = battleAssets;
        _layout = new BattleMenuLayout(new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height));
        _battleController = new BattleController(ModEntry.CardDatabase, ModEntry.PlantDatabase, ResolveEnemyData());
        _onBattleResolved = onBattleResolved;
    }

    // 当前原型只有史莱姆一个敌人
    // 如果配置缺失，则返回默认值，避免界面初始化失败
    private static EnemyBattleData ResolveEnemyData()
    {
        if (ModEntry.EnemyDatabase.TryGetValue(DefaultEnemyId, out EnemyBattleData? enemyData))
        {
            return enemyData;
        }

        return new EnemyBattleData
        {
            Id = DefaultEnemyId,
            Name = "Ghost",
            MaxHealth = DefaultEnemyHealth,
            BaseInfectionsPerTurn = 1,
            EnrageIntervalTurns = 3,
            ExtraInfectionsPerEnrage = 1
        };
    }

    // 手牌采用中心优先的叠放顺序，保证中间卡默认显示在最上层
    // 如果当前存在悬停卡牌，则额外把它放到最后绘制
    private static List<int> GetHandDrawOrder(int handCount, int? hoveredIndex = null)
    {
        List<int> drawOrder = new(handCount);
        for (int index = 0; index < handCount; index++)
        {
            drawOrder.Add(index);
        }

        float centerIndex = (Math.Max(1, handCount) - 1) / 2f;
        drawOrder.Sort((left, right) =>
        {
            float leftDistance = MathF.Abs(left - centerIndex);
            float rightDistance = MathF.Abs(right - centerIndex);
            int distanceCompare = rightDistance.CompareTo(leftDistance);
            if (distanceCompare != 0)
            {
                return distanceCompare;
            }

            return left.CompareTo(right);
        });

        if (hoveredIndex.HasValue && hoveredIndex.Value >= 0 && hoveredIndex.Value < handCount)
        {
            drawOrder.Remove(hoveredIndex.Value);
            drawOrder.Add(hoveredIndex.Value);
        }

        return drawOrder;
    }

    // 点击和悬停需要按最上层到最下层的顺序命中
    private static List<int> GetHandHitTestOrder(int handCount, int? hoveredIndex = null)
    {
        List<int> hitTestOrder = GetHandDrawOrder(handCount, hoveredIndex);
        hitTestOrder.Reverse();
        return hitTestOrder;
    }

    // 记录上一帧悬停手牌，用来让重叠区域里的高亮卡保持最上层
    private int? GetHoveredHandIndex()
    {
        for (int index = 0; index < _battleController.State.CardManager.Hand.Count; index++)
        {
            if (_battleController.State.CardManager.Hand[index].IsHovered)
            {
                return index;
            }
        }

        return null;
    }
}
