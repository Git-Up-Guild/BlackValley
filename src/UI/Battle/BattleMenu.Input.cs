using BlackValley.Battle;
using BlackValley.Cards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace BlackValley.UI.Battle;

public sealed partial class BattleMenu
{
    /// <summary>
    /// 根据鼠标位置更新手牌悬停反馈
    /// </summary>
    /// <param name="x">屏幕空间 X 坐标</param>
    /// <param name="y">屏幕空间 Y 坐标</param>
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

        if (!CanReceiveBattleInput() || _draggedCard != null)
        {
            ClearHandHoverState();
            return;
        }

        int? hoveredIndex = GetHoveredHandIndex();
        ClearHandHoverState();

        int handCount = _battleController.State.CardManager.Hand.Count;
        foreach (int index in GetHandHitTestOrder(handCount, hoveredIndex))
        {
            CardInstance card = _battleController.State.CardManager.Hand[index];
            Rectangle hitBounds = card.Bounds.Width > 0
                ? card.Bounds
                : _layout.GetHandCardBounds(handCount, index);

            if (!hitBounds.Contains(x, y))
            {
                continue;
            }

            card.IsHovered = true;
            break;
        }
    }

    /// <summary>
    /// 处理鼠标左键点击
    /// 优先尝试抓取手牌，其次处理按钮点击，剩余输入交给基类菜单
    /// </summary>
    /// <param name="x">屏幕空间 X 坐标</param>
    /// <param name="y">屏幕空间 Y 坐标</param>
    /// <param name="playSound">是否允许基类菜单播放默认点击音效</param>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (TryStartDraggingCard(x, y) || TryHandleButtonClick(x, y))
        {
            return;
        }

        if (_battleController.State.IsBattleOver || TryHarvestPlant(x, y))
        {
            return;
        }

        base.receiveLeftClick(x, y, playSound);
    }

    /// <summary>
    /// 在鼠标松开时结算当前拖拽的卡牌
    /// </summary>
    /// <param name="x">屏幕空间 X 坐标</param>
    /// <param name="y">屏幕空间 Y 坐标</param>
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);

        if (_draggedCard == null || _battleController.State.IsBattleOver)
        {
            return;
        }

        TryPlayCard(_draggedCard, x, y);
        _draggedCard = null;
    }

    /// <summary>
    /// 在按下菜单键或 Escape 时关闭战斗菜单
    /// </summary>
    /// <param name="key">当前按下的键位</param>
    public override void receiveKeyPress(Keys key)
    {
        if (Game1.options.menuButton.Contains(new InputButton(key)) || key == Keys.Escape)
        {
            CloseMenu();
            return;
        }

        base.receiveKeyPress(key);
    }

    // 只允许从当前手牌槽发起拖拽，避免和其他 UI 区域产生误触
    private bool TryStartDraggingCard(int x, int y)
    {
        if (!CanReceiveBattleInput())
        {
            return false;
        }

        int handCount = _battleController.State.CardManager.Hand.Count;
        foreach (int index in GetHandHitTestOrder(handCount, GetHoveredHandIndex()))
        {
            CardInstance card = _battleController.State.CardManager.Hand[index];
            Rectangle hitBounds = card.Bounds.Width > 0
                ? card.Bounds
                : _layout.GetHandCardBounds(handCount, index);

            if (!hitBounds.Contains(x, y))
            {
                continue;
            }

            if (_battleController.State.CurrentEnergy < card.Data.Cost)
            {
                Game1.playSound("cancel");
                return true;
            }

            _draggedCard = card;
            ClearHandHoverState();
            Game1.playSound("dwop");
            return true;
        }

        return false;
    }

    // 按钮命中统一在这里分发，避免输入入口继续膨胀
    private bool TryHandleButtonClick(int x, int y)
    {
        if (_layout.CloseButtonBounds.Contains(x, y))
        {
            CloseMenu();
            return true;
        }

        if (_battleController.State.IsBattleOver)
        {
            return false;
        }

        if (_layout.EndTurnButtonBounds.Contains(x, y))
        {
            HandleEndTurn();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 执行回合结束结算
    /// 输入层只负责触发控制器结算，并在成功时播放反馈音效
    /// </summary>
    private void HandleEndTurn()
    {
        if (!CanReceiveBattleInput())
        {
            return;
        }

        _battleController.EndTurn();
        Game1.playSound("smallSelect");
    }

    /// <summary>
    /// 尝试打出一张拖拽中的卡牌
    /// 先校验费用，再根据目标类型分发到对应的结算逻辑
    /// </summary>
    /// <param name="card">当前尝试打出的卡牌</param>
    /// <param name="x">屏幕空间 X 坐标</param>
    /// <param name="y">屏幕空间 Y 坐标</param>
    private void TryPlayCard(CardInstance card, int x, int y)
    {
        if (!CanReceiveBattleInput())
        {
            return;
        }

        bool isValidTarget = false;
        if (card.Data.TargetType == EnemyTargetType)
        {
            isValidTarget = TryPlayEnemyCard(card, x, y);
        }
        else if (card.Data.TargetType.StartsWith(GridTargetPrefix, System.StringComparison.Ordinal))
        {
            isValidTarget = TryPlayGridCard(card, x, y);
        }

        if (!isValidTarget)
        {
            Game1.playSound("cancel");
            return;
        }

        Game1.playSound("throwDownITem");
    }

    // 攻击牌的命中校验由 UI 处理，真正的扣血和胜负判定交给控制器
    private bool TryPlayEnemyCard(CardInstance card, int x, int y)
    {
        if (!_layout.GhostBounds.Contains(x, y))
        {
            return false;
        }

        return _battleController.TryPlayCardOnEnemy(card);
    }

    // 网格牌的落点判定由 UI 负责，实际种植和护盾结算交给控制器
    private bool TryPlayGridCard(CardInstance card, int x, int y)
    {
        Point? tile = _layout.GetGridTileAtPosition(x, y);
        if (!tile.HasValue)
        {
            return false;
        }

        return _battleController.TryPlayCardOnGrid(card, tile.Value);
    }

    // 收割点击仍由 UI 捕获，再转交控制器执行植物效果和地块清理
    private bool TryHarvestPlant(int x, int y)
    {
        if (!CanReceiveBattleInput())
        {
            return false;
        }

        Point? tile = _layout.GetGridTileAtPosition(x, y);
        if (!tile.HasValue)
        {
            return false;
        }

        bool isHarvested = _battleController.TryHarvestPlant(tile.Value);
        if (isHarvested)
        {
            Game1.playSound("harvest");
        }

        return isHarvested;
    }

    // 只有玩家行动阶段才响应拖拽 收割和结束回合等战斗输入
    private bool CanReceiveBattleInput()
    {
        return !_battleController.State.IsBattleOver
            && _battleController.State.CurrentPhase == BattleTurnPhase.PlayerAction;
    }

    // 拖拽和关闭菜单前统一清理悬停状态，避免卡牌残留放大效果
    private void ClearHandHoverState()
    {
        foreach (CardInstance card in _battleController.State.CardManager.Hand)
        {
            card.IsHovered = false;
        }
    }

    // 菜单关闭收口放在同一个方法里，保证音效、拖拽状态和 UI 关闭流程一致
    private void CloseMenu()
    {
        ClearHandHoverState();
        _draggedCard = null;
        Game1.playSound("bigDeSelect");
        Game1.exitActiveMenu();
    }
}
