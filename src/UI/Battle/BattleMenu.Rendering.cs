using BlackValley.Cards;
using BlackValley.Grid;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BlackValley.UI.Battle;

public sealed partial class BattleMenu
{
    /// <summary>
    /// 绘制当前帧的完整战斗菜单
    /// </summary>
    /// <param name="spriteBatch">当前用于绘制菜单的 SpriteBatch</param>
    public override void draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.35f);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            _layout.PanelBounds.X,
            _layout.PanelBounds.Y,
            _layout.PanelBounds.Width,
            _layout.PanelBounds.Height,
            Color.White);

        DrawHeader(spriteBatch);
        DrawFieldGrid(spriteBatch);
        DrawCharacters(spriteBatch);
        DrawBattleStatusPanels(spriteBatch);
        DrawHand(spriteBatch);
        DrawButton(spriteBatch, _layout.EndTurnButtonBounds, "End Turn", BattleTextStyles.EndTurnButton);
        DrawButton(spriteBatch, _layout.CloseButtonBounds, "X", BattleTextStyles.CloseButton);
        DrawDragState(spriteBatch);
        DrawRoundIntroOverlay(spriteBatch);

        if (_battleController.State.IsBattleOver)
        {
            DrawBattleResultOverlay(spriteBatch);
        }

        drawMouse(spriteBatch);
    }

    // 页头区域展示标题、回合数和农田生命值，属于整张面板的全局信息
    private void DrawHeader(SpriteBatch spriteBatch)
    {
        string title = "Black Valley Battle Prototype";
        Vector2 titleSize = MeasureText(BattleTextStyles.HeaderTitle, title);

        DrawText(
            spriteBatch,
            BattleTextStyles.HeaderTitle,
            title,
            new Vector2(
                _layout.PanelBounds.Center.X - titleSize.X / 2f,
                _layout.HeaderTitleY),
            Game1.textColor);

        string turnText = $"Turn: {_battleController.State.CurrentTurn}";
        DrawText(
            spriteBatch,
            BattleTextStyles.HeaderTurn,
            turnText,
            new Vector2(_layout.HeaderTurnPosition.X, _layout.HeaderTurnPosition.Y),
            Game1.textColor);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            new Rectangle(384, 396, 15, 15),
            _layout.FarmHealthBounds.X,
            _layout.FarmHealthBounds.Y,
            _layout.FarmHealthBounds.Width,
            _layout.FarmHealthBounds.Height,
            Color.White,
            2f,
            drawShadow: false);

        string healthText = $"Farm HP: {_battleController.State.FarmHealth} / {_battleController.State.MaxFarmHealth}";
        Vector2 healthTextSize = MeasureText(BattleTextStyles.HeaderFarmHealth, healthText);

        DrawText(
            spriteBatch,
            BattleTextStyles.HeaderFarmHealth,
            healthText,
            new Vector2(
                _layout.FarmHealthBounds.Center.X - healthTextSize.X / 2f,
                _layout.FarmHealthBounds.Center.Y - healthTextSize.Y / 2f + BattleMenuLayout.FarmHealthTextVerticalOffset),
            Color.DarkGreen);
    }

    // 网格绘制顺序固定为：地块底图 -> 怪物意图预警 -> 感染覆盖层 -> 临时护盾 -> 植物占位表现
    private void DrawFieldGrid(SpriteBatch spriteBatch)
    {
        for (int row = 0; row < BattleMenuLayout.FieldRowCount; row++)
        {
            for (int column = 0; column < BattleMenuLayout.FieldColumnCount; column++)
            {
                Rectangle tileBounds = _layout.GetGridTileBounds(column, row);
                BattleGridTileState tileState = _battleController.State.FarmGrid[column, row];

                spriteBatch.Draw(_battleAssets.FieldTileTexture, tileBounds, Color.White);

                int intentHitCount = _battleController.GetMonsterIntentHitCount(column, row);
                if (intentHitCount > 0)
                {
                    float alpha = MathF.Min(0.2f + intentHitCount * 0.1f, 0.45f);
                    spriteBatch.Draw(Game1.staminaRect, tileBounds, Color.Red * alpha);
                }

                if (tileState.IsInfected)
                {
                    spriteBatch.Draw(Game1.staminaRect, tileBounds, Color.Purple * 0.6f);
                }

                if (tileState.TemporaryProtectionCharges > 0)
                {
                    spriteBatch.Draw(Game1.staminaRect, tileBounds, Color.CornflowerBlue * 0.45f);
                }

                if (tileState.HasPlant)
                {
                    Rectangle plantBounds = new Rectangle(
                        tileBounds.X + 8,
                        tileBounds.Y + 8,
                        BattleMenuLayout.FieldTileSize - 16,
                        BattleMenuLayout.FieldTileSize - 16);
                    Rectangle plantStateBounds = new Rectangle(
                        plantBounds.X,
                        plantBounds.Y,
                        plantBounds.Width,
                        18);
                    Rectangle plantProtectionBounds = new Rectangle(
                        tileBounds.X + 4,
                        tileBounds.Bottom - 24,
                        tileBounds.Width - 8,
                        16);

                    Color plantColor = tileState.IsMature ? Color.LightBlue * 0.85f : Color.LightGreen * 0.85f;
                    spriteBatch.Draw(Game1.staminaRect, plantBounds, plantColor);
                    DrawCenteredText(
                        spriteBatch,
                        BattleTextStyles.FieldPlantState,
                        GetPlantStateLabel(tileState),
                        plantStateBounds,
                        Color.DarkGreen);

                    if (tileState.RemainingProtectionCharges > 0)
                    {
                        DrawCenteredText(
                            spriteBatch,
                            BattleTextStyles.FieldPlantProtection,
                            $"Block {tileState.RemainingProtectionCharges}",
                            plantProtectionBounds,
                            Color.Navy);
                    }
                }

                if (tileState.TemporaryProtectionCharges > 0)
                {
                    Rectangle temporaryProtectionBounds = new Rectangle(
                        tileBounds.X + 4,
                        tileBounds.Y + 30,
                        tileBounds.Width - 8,
                        16);

                    DrawCenteredText(
                        spriteBatch,
                        BattleTextStyles.FieldTemporaryProtection,
                        $"Shield {tileState.TemporaryProtectionCharges}",
                        temporaryProtectionBounds,
                        Color.White);
                }
            }
        }
    }

    // 角色区域负责绘制双方立绘、怪物血条和当前攻击意图提示
    private void DrawCharacters(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_battleAssets.FarmerTexture, _layout.FarmerBounds, Color.White);
        spriteBatch.Draw(_battleAssets.GhostTexture, _layout.GhostBounds, Color.White);

        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.CharacterName,
            "Farmer",
            _layout.FarmerNameBounds,
            Game1.textColor);

        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.CharacterName,
            _battleController.EnemyData.Name,
            _layout.EnemyNameBounds,
            Game1.textColor);

        float hpPercent = _battleController.State.GhostMaxHp > 0
            ? (float)_battleController.State.GhostCurrentHp / _battleController.State.GhostMaxHp
            : 0f;
        Rectangle hpBarFill = new Rectangle(
            _layout.EnemyHealthBarBounds.X,
            _layout.EnemyHealthBarBounds.Y,
            (int)(_layout.EnemyHealthBarBounds.Width * hpPercent),
            _layout.EnemyHealthBarBounds.Height);

        spriteBatch.Draw(Game1.staminaRect, _layout.EnemyHealthBarBounds, Color.DarkRed);
        spriteBatch.Draw(Game1.staminaRect, hpBarFill, Color.Red);

        string hpText = $"{_battleController.State.GhostCurrentHp}/{_battleController.State.GhostMaxHp}";
        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.EnemyHealth,
            hpText,
            _layout.EnemyHealthBarBounds,
            Color.White);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            _layout.MonsterIntentBounds.X,
            _layout.MonsterIntentBounds.Y,
            _layout.MonsterIntentBounds.Width,
            _layout.MonsterIntentBounds.Height,
            Color.White);

        string intentText = _battleController.State.MonsterIntentCount > 0
            ? $"Infect x{_battleController.State.MonsterIntentCount}"
            : "Idle";
        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.MonsterIntent,
            intentText,
            _layout.MonsterIntentBounds,
            Color.DarkRed);
    }

    // 这里集中绘制能量、抽牌堆和弃牌堆等战斗资源信息，避免散落在各个绘制方法里
    private void DrawBattleStatusPanels(SpriteBatch spriteBatch)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            _layout.EnergyBounds.X,
            _layout.EnergyBounds.Y,
            _layout.EnergyBounds.Width,
            _layout.EnergyBounds.Height,
            Color.Gold);

        string energyText = $"{_battleController.State.CurrentEnergy}/{_battleController.State.MaxEnergy}";
        Vector2 energyTextSize = MeasureText(BattleTextStyles.EnergyValue, energyText);
        DrawText(
            spriteBatch,
            BattleTextStyles.EnergyValue,
            energyText,
            new Vector2(
                _layout.EnergyBounds.Center.X - energyTextSize.X / 2f,
                _layout.EnergyBounds.Center.Y - energyTextSize.Y / 2f),
            Color.Black);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            _layout.DrawPileBounds.X,
            _layout.DrawPileBounds.Y,
            _layout.DrawPileBounds.Width,
            _layout.DrawPileBounds.Height,
            Color.LightCyan);

        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.PileCount,
            $"Draw\n  {_battleController.State.CardManager.DrawPile.Count}",
            _layout.DrawPileBounds,
            Game1.textColor);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            _layout.DiscardPileBounds.X,
            _layout.DiscardPileBounds.Y,
            _layout.DiscardPileBounds.Width,
            _layout.DiscardPileBounds.Height,
            Color.LightGray);

        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.PileCount,
            $"Discard\n   {_battleController.State.CardManager.DiscardPile.Count}",
            _layout.DiscardPileBounds,
            Game1.textColor);

        if (_battleController.State.BonusAttackDamageThisTurn > 0)
        {
            Rectangle bonusAttackBounds = new Rectangle(
                _layout.EnergyBounds.X - 20,
                _layout.EnergyBounds.Bottom + 10,
                _layout.EnergyBounds.Width + 40,
                18);

            DrawCenteredText(
                spriteBatch,
                BattleTextStyles.TurnModifier,
                $"Attack +{_battleController.State.BonusAttackDamageThisTurn}",
                bonusAttackBounds,
                Color.DarkRed);
        }

        if (_battleController.State.InfectionTierReductionThisTurn > 0)
        {
            Rectangle infectionReductionBounds = new Rectangle(
                _layout.EnergyBounds.X - 20,
                _layout.EnergyBounds.Bottom + 32,
                _layout.EnergyBounds.Width + 40,
                18);

            DrawCenteredText(
                spriteBatch,
                BattleTextStyles.TurnModifier,
                $"Infect -{_battleController.State.InfectionTierReductionThisTurn}",
                infectionReductionBounds,
                Color.DarkOliveGreen);
        }
    }

    // 手牌绘制只负责表现，不修改手牌数据本身；拖拽中的牌会交给顶部预览层单独绘制
    private void DrawHand(SpriteBatch spriteBatch)
    {
        int handCount = Math.Min(_battleController.State.CardManager.Hand.Count, BattleMenuLayout.HandSlotCount);
        List<int> drawOrder = GetHandDrawOrder(handCount, GetHoveredHandIndex());

        foreach (int index in drawOrder)
        {
            CardInstance card = _battleController.State.CardManager.Hand[index];
            if (card == _draggedCard)
            {
                continue;
            }

            Rectangle drawBounds = _layout.GetHandCardBounds(handCount, index);
            float rotation = _layout.GetHandCardRotationRadians(handCount, index);
            if (card.IsHovered)
            {
                // 悬停时通过轻微上浮和放大强调当前聚焦的手牌
                drawBounds.Y -= 20;
                drawBounds.Inflate(10, 10);
            }

            card.Bounds = drawBounds;
            DrawTiltedCardFace(spriteBatch, card.Data, drawBounds, card.IsHovered ? 1f : 0.95f, rotation);
        }
    }

    // 拖拽表现独立放在最上层，确保连线、高亮和浮动预览不会被底层 UI 遮挡
    private void DrawDragState(SpriteBatch spriteBatch)
    {
        if (_draggedCard == null)
        {
            return;
        }

        int mouseX = Game1.getMouseX();
        int mouseY = Game1.getMouseY();

        Utility.drawLineWithScreenCoordinates(
            _layout.FarmerBounds.Center.X,
            _layout.FarmerBounds.Center.Y,
            mouseX,
            mouseY,
            spriteBatch,
            Color.White * 0.8f,
            4f);

        if (_draggedCard.Data.TargetType == EnemyTargetType)
        {
            // 攻击牌仅在命中怪物时给予红色高亮反馈
            if (_layout.GhostBounds.Contains(mouseX, mouseY))
            {
                IClickableMenu.drawTextureBox(
                    spriteBatch,
                    _layout.GhostBounds.X - 10,
                    _layout.GhostBounds.Y - 10,
                    _layout.GhostBounds.Width + 20,
                    _layout.GhostBounds.Height + 20,
                    Color.Red * 0.6f);
            }
        }
        else if (_draggedCard.Data.TargetType.StartsWith(GridTargetPrefix, StringComparison.Ordinal))
        {
            Point? centerTile = _layout.GetGridTileAtPosition(mouseX, mouseY);
            if (centerTile.HasValue)
            {
                Color previewColor = _draggedCard.Data.Type.Equals(DefenseCardType, StringComparison.Ordinal)
                    ? Color.CornflowerBlue * 0.5f
                    : Color.LightGreen * 0.5f;

                // 网格牌根据 Shape 预览影响范围，便于玩家确认落点是否合法
                foreach (Point targetTile in _battleController.GetPreviewTargets(_draggedCard.Data, centerTile.Value))
                {
                    Rectangle tileBounds = _layout.GetGridTileBounds(targetTile.X, targetTile.Y);
                    spriteBatch.Draw(Game1.staminaRect, tileBounds, previewColor);
                }
            }
        }

        Rectangle previewBounds = new Rectangle(mouseX + 20, mouseY + 20, 136, 206);
        DrawCardFace(spriteBatch, _draggedCard.Data, previewBounds, 0.92f);
    }

    // 直立卡面也复用和手牌一致的自定义卡面渲染，避免拖拽预览和手牌样式分叉
    private void DrawCardFace(SpriteBatch spriteBatch, CardData cardData, Rectangle drawBounds, float alpha)
    {
        DrawTiltedCardFace(spriteBatch, cardData, drawBounds, alpha, 0f);
    }

    // 手牌采用扇形斜置效果，需要把卡面整体按同一个旋转中心绘制
    private void DrawTiltedCardFace(SpriteBatch spriteBatch, CardData cardData, Rectangle drawBounds, float alpha, float rotation)
    {
        BattleCardFaceLayout cardFaceLayout = _layout.GetCardFaceLayout(drawBounds);

        DrawRotatedPanel(spriteBatch, cardFaceLayout.CardBounds, drawBounds, rotation, BattleCardStyle.PanelFillColor * alpha, BattleCardStyle.PanelBorderColor * alpha, BattleCardStyle.PanelShadowColor * (0.22f * alpha));
        DrawRotatedPanel(spriteBatch, cardFaceLayout.CostBounds, drawBounds, rotation, BattleCardStyle.CostFillColor * alpha, BattleCardStyle.CostBorderColor * alpha, BattleCardStyle.PanelShadowColor * (0.16f * alpha));

        DrawRotatedCenteredText(
            spriteBatch,
            BattleTextStyles.CardCost,
            cardData.Cost.ToString(),
            cardFaceLayout.CostBounds,
            drawBounds,
            rotation,
            Color.Black);

        DrawRotatedWrappedCenteredText(
            spriteBatch,
            BattleTextStyles.CardName,
            cardData.Name,
            cardFaceLayout.NameBounds,
            drawBounds,
            rotation,
            BattleCardStyle.NameWrapWidthFactor,
            Game1.textColor);

        DrawRotatedPanel(spriteBatch, cardFaceLayout.IconBounds, drawBounds, rotation, BattleCardStyle.IconFillColor * alpha, BattleCardStyle.IconBorderColor * alpha, BattleCardStyle.PanelShadowColor * (0.12f * alpha));
        DrawRotatedCardIcon(spriteBatch, cardData, cardFaceLayout.IconBounds, drawBounds, rotation, alpha);
        DrawRotatedCardDescription(spriteBatch, cardData.Description, cardFaceLayout.DescriptionBounds, drawBounds, BattleTextStyles.CardDescription, rotation, BattleCardStyle.DescriptionWrapWidthFactor, alpha);
    }

    // 图标绘制优先读取真实贴图，缺失时退化为文字占位，保证卡面布局始终完整
    private void DrawCardIcon(SpriteBatch spriteBatch, CardData cardData, Rectangle iconBounds, float alpha)
    {
        Rectangle iconDrawBounds = new Rectangle(
            iconBounds.X + 8,
            iconBounds.Y + 8,
            iconBounds.Width - 16,
            iconBounds.Height - 16);

        if (_battleAssets.TryGetCardIconTexture(cardData.IconId, out Texture2D iconTexture))
        {
            spriteBatch.Draw(iconTexture, iconDrawBounds, Color.White * alpha);
            return;
        }

        spriteBatch.Draw(Game1.staminaRect, iconDrawBounds, Color.SlateGray * (0.35f * alpha));
        DrawCenteredText(
            spriteBatch,
            BattleTextStyles.CardFallbackIcon,
            cardData.Type,
            iconDrawBounds,
            Color.DimGray);
    }

    // 斜置卡牌的图标和占位文字需要跟随整张卡一起旋转
    private void DrawRotatedCardIcon(SpriteBatch spriteBatch, CardData cardData, Rectangle iconBounds, Rectangle cardBounds, float rotation, float alpha)
    {
        Rectangle iconDrawBounds = new Rectangle(
            iconBounds.X + 8,
            iconBounds.Y + 8,
            iconBounds.Width - 16,
            iconBounds.Height - 16);

        if (_battleAssets.TryGetCardIconTexture(cardData.IconId, out Texture2D iconTexture))
        {
            DrawRotatedTexture(spriteBatch, iconTexture, iconDrawBounds, cardBounds, rotation, Color.White * alpha);
            return;
        }

        DrawRotatedTexture(spriteBatch, Game1.staminaRect, iconDrawBounds, cardBounds, rotation, Color.SlateGray * (0.35f * alpha));
        DrawRotatedCenteredText(
            spriteBatch,
            BattleTextStyles.CardFallbackIcon,
            cardData.Type,
            iconDrawBounds,
            cardBounds,
            rotation,
            Color.DimGray);
    }

    // 描述区域按宽度自动折行，避免文本超出卡面边界
    private static void DrawCardDescription(SpriteBatch spriteBatch, string description, Rectangle descriptionBounds, BattleTextStyle style, float wrapWidthFactor, float alpha)
    {
        int wrapWidth = Math.Max(1, (int)(descriptionBounds.Width * wrapWidthFactor / style.Scale));
        string wrappedText = Game1.parseText(description, style.Font, wrapWidth);

        DrawText(
            spriteBatch,
            style,
            wrappedText,
            new Vector2(descriptionBounds.X, descriptionBounds.Y),
            Game1.textColor * alpha);
    }

    // 斜置卡牌的描述需要在旋转后保持和卡面同方向
    private static void DrawRotatedCardDescription(
        SpriteBatch spriteBatch,
        string description,
        Rectangle descriptionBounds,
        Rectangle cardBounds,
        BattleTextStyle style,
        float rotation,
        float wrapWidthFactor,
        float alpha)
    {
        int wrapWidth = Math.Max(1, (int)(descriptionBounds.Width * wrapWidthFactor / style.Scale));
        string wrappedText = Game1.parseText(description, style.Font, wrapWidth);

        DrawRotatedText(
            spriteBatch,
            style,
            wrappedText,
            new Vector2(descriptionBounds.X, descriptionBounds.Y),
            cardBounds,
            rotation,
            Game1.textColor * alpha);
    }

    // 通用绘字入口，后续只需要改 BattleTextStyles 就能统一调整大小
    private static void DrawText(SpriteBatch spriteBatch, BattleTextStyle style, string text, Vector2 position, Color color)
    {
        Vector2 snappedPosition = new(MathF.Round(position.X), MathF.Round(position.Y));

        spriteBatch.DrawString(
            style.Font,
            text,
            snappedPosition,
            color,
            0f,
            Vector2.Zero,
            style.Scale,
            SpriteEffects.None,
            1f);
    }

    // 旋转文本时统一绕卡牌底部中心旋转，保证手牌扇形排布时内容一起倾斜
    private static void DrawRotatedText(
        SpriteBatch spriteBatch,
        BattleTextStyle style,
        string text,
        Vector2 position,
        Rectangle cardBounds,
        float rotation,
        Color color)
    {
        Vector2 cardOrigin = new(cardBounds.Width / 2f, cardBounds.Height);
        Vector2 cardTopLeft = new(cardBounds.X, cardBounds.Y);
        Vector2 localPosition = position - cardTopLeft;
        Vector2 snappedLocalPosition = new(MathF.Round(localPosition.X), MathF.Round(localPosition.Y));
        Vector2 desiredOriginInDestination = cardOrigin - snappedLocalPosition;
        Vector2 textOrigin = desiredOriginInDestination / style.Scale;

        spriteBatch.DrawString(
            style.Font,
            text,
            cardTopLeft + cardOrigin,
            color,
            rotation,
            textOrigin,
            style.Scale,
            SpriteEffects.None,
            1f);
    }

    // 统一计算文字测量结果，避免每个模块各自乘缩放
    private static Vector2 MeasureText(BattleTextStyle style, string text)
    {
        return style.Font.MeasureString(text) * style.Scale;
    }

    // 通用居中文本入口，避免卡面上的费用和名称各自重复计算
    private static void DrawCenteredText(SpriteBatch spriteBatch, BattleTextStyle style, string text, Rectangle bounds, Color color)
    {
        Vector2 textSize = MeasureText(style, text);
        float textX = bounds.Center.X - textSize.X / 2f;
        float textY = bounds.Center.Y - textSize.Y / 2f;

        DrawText(spriteBatch, style, text, new Vector2(textX, textY), color);
    }

    // 旋转卡牌中的文本同样需要按区域居中
    private static void DrawRotatedCenteredText(
        SpriteBatch spriteBatch,
        BattleTextStyle style,
        string text,
        Rectangle bounds,
        Rectangle cardBounds,
        float rotation,
        Color color)
    {
        Vector2 textSize = MeasureText(style, text);
        float textX = bounds.Center.X - textSize.X / 2f;
        float textY = bounds.Center.Y - textSize.Y / 2f;

        DrawRotatedText(spriteBatch, style, text, new Vector2(textX, textY), cardBounds, rotation, color);
    }

    // 卡名需要允许两行甚至更多行显示，因此这里先折行后再整体居中
    private static void DrawWrappedCenteredText(SpriteBatch spriteBatch, BattleTextStyle style, string text, Rectangle bounds, float wrapWidthFactor, Color color)
    {
        int wrapWidth = Math.Max(1, (int)(bounds.Width * wrapWidthFactor / style.Scale));
        string wrappedText = Game1.parseText(text, style.Font, wrapWidth);
        Vector2 textSize = MeasureText(style, wrappedText);
        float textX = bounds.Center.X - textSize.X / 2f;
        float textY = bounds.Center.Y - textSize.Y / 2f;

        DrawText(spriteBatch, style, wrappedText, new Vector2(textX, textY), color);
    }

    // 旋转卡牌中的卡名支持自动换行并整体居中
    private static void DrawRotatedWrappedCenteredText(
        SpriteBatch spriteBatch,
        BattleTextStyle style,
        string text,
        Rectangle bounds,
        Rectangle cardBounds,
        float rotation,
        float wrapWidthFactor,
        Color color)
    {
        int wrapWidth = Math.Max(1, (int)(bounds.Width * wrapWidthFactor / style.Scale));
        string wrappedText = Game1.parseText(text, style.Font, wrapWidth);
        Vector2 textSize = MeasureText(style, wrappedText);
        float textX = bounds.Center.X - textSize.X / 2f;
        float textY = bounds.Center.Y - textSize.Y / 2f;

        DrawRotatedText(spriteBatch, style, wrappedText, new Vector2(textX, textY), cardBounds, rotation, color);
    }

    // 斜置卡牌使用简单描边面板，避免手写九宫格旋转过于复杂
    private static void DrawRotatedPanel(
        SpriteBatch spriteBatch,
        Rectangle bounds,
        Rectangle cardBounds,
        float rotation,
        Color fillColor,
        Color borderColor,
        Color shadowColor)
    {
        Rectangle shadowBounds = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width, bounds.Height);
        Rectangle borderBounds = new Rectangle(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);
        DrawRotatedTexture(spriteBatch, Game1.staminaRect, shadowBounds, cardBounds, rotation, shadowColor);
        DrawRotatedTexture(spriteBatch, Game1.staminaRect, borderBounds, cardBounds, rotation, borderColor);
        DrawRotatedTexture(spriteBatch, Game1.staminaRect, bounds, cardBounds, rotation, fillColor);
    }

    // 让矩形和图标资源都能围绕同一个卡牌原点进行旋转绘制
    private static void DrawRotatedTexture(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle targetBounds,
        Rectangle cardBounds,
        float rotation,
        Color color)
    {
        if (targetBounds.Width <= 0 || targetBounds.Height <= 0)
        {
            return;
        }

        Vector2 cardOrigin = new(cardBounds.Width / 2f, cardBounds.Height);
        Vector2 cardTopLeft = new(cardBounds.X, cardBounds.Y);
        Vector2 targetTopLeftLocal = new(targetBounds.X - cardBounds.X, targetBounds.Y - cardBounds.Y);
        Vector2 desiredOriginInDestination = cardOrigin - targetTopLeftLocal;
        Vector2 scale = new(
            targetBounds.Width / (float)texture.Width,
            targetBounds.Height / (float)texture.Height);
        Vector2 sourceOrigin = new(
            desiredOriginInDestination.X / scale.X,
            desiredOriginInDestination.Y / scale.Y);

        spriteBatch.Draw(
            texture,
            cardTopLeft + cardOrigin,
            null,
            color,
            rotation,
            sourceOrigin,
            scale,
            SpriteEffects.None,
            1f);
    }

    // 通用按钮绘制入口，避免关闭按钮和结束回合按钮分别维护同一套样式
    private static void DrawButton(SpriteBatch spriteBatch, Rectangle buttonBounds, string text, BattleTextStyle textStyle)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            buttonBounds.X,
            buttonBounds.Y,
            buttonBounds.Width,
            buttonBounds.Height,
            Color.White);

        DrawCenteredText(spriteBatch, textStyle, text, buttonBounds, Game1.textColor);
    }

    // 成熟植物用文字提醒可以收割，未成熟植物显示剩余成长回合
    private static string GetPlantStateLabel(BattleGridTileState tileState)
    {
        return tileState.IsMature ? "Harvest" : $"Grow {tileState.GrowthTurnsRemaining}";
    }

    // 战斗结束后在界面中央给出明确结果，避免玩家还以为能继续出牌
    private void DrawBattleResultOverlay(SpriteBatch spriteBatch)
    {
        Rectangle overlayBounds = new Rectangle(
            _layout.PanelBounds.Center.X - 180,
            _layout.PanelBounds.Center.Y - 70,
            360,
            140);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            overlayBounds.X,
            overlayBounds.Y,
            overlayBounds.Width,
            overlayBounds.Height,
            Color.White);

        Vector2 resultTextSize = MeasureText(BattleTextStyles.BattleResultTitle, _battleController.State.BattleResultText);
        DrawText(
            spriteBatch,
            BattleTextStyles.BattleResultTitle,
            _battleController.State.BattleResultText,
            new Vector2(
                overlayBounds.Center.X - resultTextSize.X / 2f,
                overlayBounds.Y + 24),
            Color.DarkRed);

        string closeHint = "Press close to exit";
        Vector2 closeHintSize = MeasureText(BattleTextStyles.BattleResultHint, closeHint);
        DrawText(
            spriteBatch,
            BattleTextStyles.BattleResultHint,
            closeHint,
            new Vector2(
                overlayBounds.Center.X - closeHintSize.X / 2f,
                overlayBounds.Bottom - 38),
            Game1.textColor);
    }
}
