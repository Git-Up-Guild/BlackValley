using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BlackValley;

public sealed class BattleMenu : IClickableMenu
{
    private readonly BattleAssets _battleAssets;
    private readonly CardManager _cardManager; // 【新增】卡牌管理器实例

    // --- 新增拖拽状态变量 ---
    private CardInstance? _draggedCard;

    // ==========================================
    // 【阶段五新增】 1. 迷你的网格状态数据结构
    // ==========================================
    private class TileState
    {
        public bool IsInfected = false;
        public string PlantName = ""; // 为空表示没有植物
    }
    private TileState[,] _farmGrid; // 4x4 的真实农田数据
    private int _monsterIntentColumn = 0; // 模拟：怪物打算攻击哪一列
    private Random _rng = new Random();
    // ==========================================

    // 【新增】怪物的血量数据
    private int _ghostMaxHp = 12;
    private int _ghostCurrentHp = 12;


    private readonly Rectangle _panelBounds;
    private readonly Rectangle _endTurnButtonBounds;
    private readonly Rectangle _closeButtonBounds;

    private readonly Rectangle _fieldAreaBounds;
    private readonly Rectangle _farmerBounds;
    private readonly Rectangle _ghostBounds;

    // 新增的UI区域
    private readonly Rectangle _energyBounds;
    private readonly Rectangle _drawPileBounds;
    private readonly Rectangle _discardPileBounds;
    private readonly Rectangle _farmHealthBounds;



    private readonly Rectangle[] _cardSlotBounds;

    private int _currentTurn = 1;

    private int _currentEnergy = 3; // 默认3费  新增
    private int _maxEnergy = 3;//新增
    private int _farmHealth = 16;   // 农田健康度  新增

    private const int FieldRowCount = 4;
    private const int FieldColumnCount = 4;
    private const int FieldTileSize = 64;
    private const int FieldTileGap = 4;

    // 改为 5 张卡牌  新增
    private const int MaxHandSize = 5;

    public BattleMenu(BattleAssets battleAssets)
        : base(
            (Game1.uiViewport.Width - 1100) / 2,
            (Game1.uiViewport.Height - 760) / 2,
            1100,
            760,
            showUpperRightCloseButton: false)
    {
        _battleAssets = battleAssets;

        // 【新增】初始化卡牌系统，生成测试卡组，并抽出第一回合的5张牌
        _cardManager = new CardManager();
        _cardManager.InitializeTestDeck();
        _cardManager.DrawCards(MaxHandSize);

        // 初始化 4x4 空白农田
        _farmGrid = new TileState[FieldColumnCount, FieldRowCount];
        for (int c = 0; c < FieldColumnCount; c++)
            for (int r = 0; r < FieldRowCount; r++)
                _farmGrid[c, r] = new TileState();

        _monsterIntentColumn = _rng.Next(0, FieldColumnCount); // 随机一个初始意图


        _panelBounds = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);

        _closeButtonBounds = new Rectangle(
            _panelBounds.Right - 70,
            _panelBounds.Top + 20,
            40,
            40);

        _endTurnButtonBounds = new Rectangle(
            _panelBounds.Right - 180,//修改过  原230
            _panelBounds.Bottom - 95,
            140, //原170
            60);

        int fieldWidth = FieldColumnCount * FieldTileSize + (FieldColumnCount - 1) * FieldTileGap;
        int fieldHeight = FieldRowCount * FieldTileSize + (FieldRowCount - 1) * FieldTileGap;

        _fieldAreaBounds = new Rectangle(
            _panelBounds.Center.X - fieldWidth / 2,
            _panelBounds.Top + 140,//原170  稍微上移给下方手牌留空间
            fieldWidth,
            fieldHeight);

        _farmerBounds = new Rectangle(
            _fieldAreaBounds.Left - 180,
            _fieldAreaBounds.Top + 20,
            128,
            128);

        _ghostBounds = new Rectangle(
            _fieldAreaBounds.Right + 52,
            _fieldAreaBounds.Top + 20,
            128,
            128);



        _cardSlotBounds = new Rectangle[MaxHandSize];
        int cardWidth = 130;  // 稍微缩小一点点宽度以容纳5张并留出两侧空间 (原140)
        int cardHeight = 200;
        int cardGap = 15;
        int totalCardWidth = cardWidth * MaxHandSize + cardGap * (MaxHandSize - 1);
        int cardStartX = _panelBounds.Center.X - totalCardWidth / 2;
        int cardY = _panelBounds.Bottom - 230;

        for (int index = 0; index < MaxHandSize; index++)//3
        {
            _cardSlotBounds[index] = new Rectangle(
                cardStartX + index * (cardWidth + cardGap),
                cardY,
                cardWidth,
                cardHeight);
        }


        // 新增的资源布局 (左下能量和牌库，右下弃牌堆)
        _energyBounds = new Rectangle(cardStartX - 110, cardY + 10, 80, 80);
        _drawPileBounds = new Rectangle(cardStartX - 110, cardY + 110, 80, 80);
        // 右侧：结束回合按钮在上，弃牌堆在下
        _endTurnButtonBounds = new Rectangle(_panelBounds.Right - 170, cardY + 10, 140, 60);
        _discardPileBounds = new Rectangle(_panelBounds.Right - 140, cardY + 110, 80, 80);
        

        // 顶部农田健康度  新增
        _farmHealthBounds = new Rectangle(_panelBounds.Center.X - 100, _panelBounds.Top + 70, 200, 30);
    
    }

    // --- 核心工具：通过屏幕XY坐标，算出当前鼠标指在 4x4 网格的第几行第几列 ---
    private Point? GetGridTileAtPosition(int x, int y)
    {
        if (!_fieldAreaBounds.Contains(x, y)) return null;

        int col = (x - _fieldAreaBounds.X) / (FieldTileSize + FieldTileGap);
        int row = (y - _fieldAreaBounds.Y) / (FieldTileSize + FieldTileGap);

        if (row >= 0 && row < FieldRowCount && col >= 0 && col < FieldColumnCount)
        {
            return new Point(col, row);
        }
        return null;
    }

    // 【新增】处理鼠标悬停动画的核心逻辑！
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        // 如果正在拖拽，就不触发手牌的上浮效果
        if (_draggedCard != null) return;

        for (int i = 0; i < _cardManager.Hand.Count; i++)
        {
            _cardManager.Hand[i].IsHovered = _cardSlotBounds[i].Contains(x, y);
        }
    }
    /// <summary>
    /// 鼠标左键点击事件回调，用于处理抓取卡牌和点击按钮。
    /// </summary>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        // 1. 优先检测是否点中了某张手牌，如果是，将其设置为“抓取状态”
        for (int i = 0; i < _cardManager.Hand.Count; i++)
        {
            if (_cardSlotBounds[i].Contains(x, y))
            {
                _draggedCard = _cardManager.Hand[i];
                Game1.playSound("dwop"); // 抓起卡牌的音效
                return; // 截断点击，不再触发后续按钮
            }
        }

        // 2. 检测按钮点击
        if (_endTurnButtonBounds.Contains(x, y))
        {
            // --- 回合结束阶段 ---
            _currentTurn++;
            _currentEnergy = _maxEnergy;
            _cardManager.DiscardHand();
            _cardManager.DrawCards(MaxHandSize);

            // TODO: [未实现功能] 怪物攻击结算与防风草抵挡判定。
            // 逻辑流程应为：根据怪物当前的意图 (_monsterIntentColumn)，对对应网格触发感染判定 -> 检查网格是否有防御牌Buff或防风草 -> 感染成功则改变格子颜色 / 感染失败则消耗植物。

            // TODO: [未实现功能] 失败判定：检查16个格子是否全部被感染。

            // TODO: [未实现功能] 植物生长推进：遍历网格，所有已种下植物的倒计时 -1。如果变为0则成熟触发效果（如甜瓜在此刻触发十字治愈）。

            // 改变怪物下回合意图
            _monsterIntentColumn = _rng.Next(0, FieldColumnCount);
            Game1.playSound("smallSelect");
            return;
        }

        if (_closeButtonBounds.Contains(x, y))
        {
            Game1.playSound("bigDeSelect");
            Game1.exitActiveMenu();
            return;
        }

        base.receiveLeftClick(x, y, playSound);
    }

    // --- 鼠标松开：结算打牌逻辑 ---
    /// <summary>
    /// 鼠标松开时的事件回调，用于处理卡牌拖拽结束后的打出判定。
    /// </summary>
    /// <param name="x">鼠标松开时的X屏幕坐标</param>
    /// <param name="y">鼠标松开时的Y屏幕坐标</param>
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);

        if (_draggedCard != null)
        {
            TryPlayCard(_draggedCard, x, y);
            _draggedCard = null; // 无论成功与否，松手就解除抓取状态
        }
    }

    /// <summary>
    /// 核心逻辑：尝试结算一张被拖拽的卡牌。判定其目标是否合法，并执行扣费、扣血、种地等效果。
    /// </summary>
    /// <param name="card">正在尝试打出的卡牌实体</param>
    /// <param name="x">鼠标目标X坐标</param>
    /// <param name="y">鼠标目标Y坐标</param>
    private void TryPlayCard(CardInstance card, int x, int y)
    {
        if (_currentEnergy < card.Data.Cost) { Game1.playSound("cancel"); return; }

        bool isValidTarget = false;

        // 1. 如果是攻击牌，检查是否拖到了怪物身上
        if (card.Data.TargetType == "Enemy" && _ghostBounds.Contains(x, y))
        {
            isValidTarget = true;
            // TODO: [未实现功能] 草莓加攻判定：在此处计算伤害时，需检查农田里是否有成熟的草莓提供额外伤害Buff。
            _ghostCurrentHp -= card.Data.Damage;
            if (_ghostCurrentHp < 0) _ghostCurrentHp = 0;

            // TODO:[未实现功能] 胜负判定：在此处检查 _ghostCurrentHp 是否为0，若是则触发胜利逻辑。
        }
        // 2. 如果是网格目标卡牌（种子/防御技能）
        else if (card.Data.TargetType.StartsWith("Grid"))
        {
            Point? tile = GetGridTileAtPosition(x, y);
            if (tile.HasValue)
            {
                if (card.Data.Type == "Seed")
                {
                    // TODO: [未实现功能] 植物完整生命周期：目前仅用字符串 PlantName 占位。需要实例化真正的植物类，并设定其成长倒计时 (GrowthTurnsLeft)。
                    if (string.IsNullOrEmpty(_farmGrid[tile.Value.X, tile.Value.Y].PlantName) && !_farmGrid[tile.Value.X, tile.Value.Y].IsInfected)
                    {
                        _farmGrid[tile.Value.X, tile.Value.Y].PlantName = card.Data.Name;
                        isValidTarget = true;
                    }
                }
                else
                {
                    // TODO: [未实现功能] 防御牌护盾生效：目前技能牌只是扣费丢弃。需要根据 card.Data.Shape 在 _farmGrid 中对应格子上添加一回合的护盾 Buff。
                    isValidTarget = true;
                }
            }
        }

        // 结算流程
        if (isValidTarget)
        {
            _currentEnergy -= card.Data.Cost;
            _cardManager.Hand.Remove(card);
            _cardManager.DiscardPile.Add(card.Data);
            Game1.playSound("throwDownITem");
        }
        else
        {
            Game1.playSound("cancel");
        }
    }


    public override void receiveKeyPress(Microsoft.Xna.Framework.Input.Keys key)
    {
        if (Game1.options.menuButton.Contains(new InputButton(key))
            || key == Microsoft.Xna.Framework.Input.Keys.Escape)
        {
            Game1.playSound("bigDeSelect");
            Game1.exitActiveMenu();
            return;
        }

        base.receiveKeyPress(key);
    }

    public override void draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.35f);

        IClickableMenu.drawTextureBox(
            spriteBatch,
            _panelBounds.X,
            _panelBounds.Y,
            _panelBounds.Width,
            _panelBounds.Height,
            Color.White);

        DrawHeader(spriteBatch);
        DrawFieldGrid(spriteBatch);
        DrawCharacters(spriteBatch);

        DrawStSUIElements(spriteBatch); // 绘制杀戮尖塔风格元素  新增


        // 【修改】现在调用新的绘制手牌方法
        DrawHand(spriteBatch);

        DrawButton(spriteBatch, _endTurnButtonBounds, "End Turn");
        DrawButton(spriteBatch, _closeButtonBounds, "X");
        // --- 最后绘制拖拽特效（保证画在最上层） ---
        DrawDragState(spriteBatch);

        drawMouse(spriteBatch);
    }

    private void DrawHeader(SpriteBatch spriteBatch)
    {
        string title = "Black Valley Battle Prototype";
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);

        spriteBatch.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(
                _panelBounds.Center.X - titleSize.X / 2f,
                _panelBounds.Top + 20),//28
            Game1.textColor);

        string turnText = $"Turn: {_currentTurn}";
        spriteBatch.DrawString(
            Game1.smallFont,
            turnText,
            new Vector2(_panelBounds.Left + 48, _panelBounds.Top + 20), //20G105
            Game1.textColor);

        // 绘制农田健康度 (模拟血条)   新增
        IClickableMenu.drawTextureBox(spriteBatch, Game1.mouseCursors, new Rectangle(384, 396, 15, 15),
            _farmHealthBounds.X, _farmHealthBounds.Y, _farmHealthBounds.Width, _farmHealthBounds.Height, Color.White, 2f, false);
        string hpText = $"Farm HP: {_farmHealth} / 16";
        Vector2 hpSize = Game1.smallFont.MeasureString(hpText);
        spriteBatch.DrawString(Game1.smallFont, hpText,
            new Vector2(_farmHealthBounds.Center.X - hpSize.X / 2f, _farmHealthBounds.Center.Y - hpSize.Y / 2f + 2),
            Color.DarkGreen);
    }



    // ==========================================
    // 【阶段五核心修改】 渲染带有植物和预警的网格
    // ==========================================
    private void DrawFieldGrid(SpriteBatch spriteBatch)
    {
        for (int row = 0; row < FieldRowCount; row++)
        {
            for (int column = 0; column < FieldColumnCount; column++)
            {
                int drawX = _fieldAreaBounds.X + column * (FieldTileSize + FieldTileGap);
                int drawY = _fieldAreaBounds.Y + row * (FieldTileSize + FieldTileGap);
                Rectangle tileBounds = new Rectangle(drawX, drawY, FieldTileSize, FieldTileSize);

                // 1. 画基础泥土格子
                spriteBatch.Draw(_battleAssets.FieldTileTexture, tileBounds, Color.White);

                // 2. 如果怪物本回合打算攻击这一列，在格子上盖一层红色预警半透明层！
                if (column == _monsterIntentColumn)
                {
                    spriteBatch.Draw(Game1.staminaRect, tileBounds, Color.Red * 0.3f);
                }

                // 3. 画格子里种下的植物 (如果没有贴图，先画一个绿框和文字占位)
                TileState state = _farmGrid[column, row];

                // 如果被污染了，画紫色
                if (state.IsInfected)
                {
                    spriteBatch.Draw(Game1.staminaRect, tileBounds, Color.Purple * 0.6f);
                }

                if (!string.IsNullOrEmpty(state.PlantName))
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(drawX + 8, drawY + 8, FieldTileSize - 16, FieldTileSize - 16), Color.LightGreen * 0.8f);
                    spriteBatch.DrawString(Game1.smallFont, "Seed", new Vector2(drawX + 12, drawY + 20), Color.DarkGreen);
                }
            }
        }
    }

    // ==========================================
    // 【阶段五核心修改】 渲染怪物的意图UI (Intent)
    // ==========================================
    private void DrawCharacters(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_battleAssets.FarmerTexture, _farmerBounds, Color.White);
        spriteBatch.Draw(_battleAssets.GhostTexture, _ghostBounds, Color.White);
        spriteBatch.DrawString(Game1.smallFont, "Farmer", new Vector2(_farmerBounds.X + 24, _farmerBounds.Bottom + 10), Game1.textColor);
        spriteBatch.DrawString(Game1.smallFont, "Ghost", new Vector2(_ghostBounds.X + 30, _ghostBounds.Bottom + 10), Game1.textColor);

        // 【新增】画怪物血条
        Rectangle hpBarBg = new Rectangle(_ghostBounds.X, _ghostBounds.Bottom + 35, _ghostBounds.Width, 20);
        float hpPercent = (float)_ghostCurrentHp / _ghostMaxHp;
        Rectangle hpBarFill = new Rectangle(hpBarBg.X, hpBarBg.Y, (int)(hpBarBg.Width * hpPercent), hpBarBg.Height);

        spriteBatch.Draw(Game1.staminaRect, hpBarBg, Color.DarkRed); // 血条底色
        spriteBatch.Draw(Game1.staminaRect, hpBarFill, Color.Red);     // 当前血量

        string hpText = $"{_ghostCurrentHp}/{_ghostMaxHp}";
        Vector2 hpTextSize = Game1.smallFont.MeasureString(hpText);
        spriteBatch.DrawString(Game1.smallFont, hpText, new Vector2(hpBarBg.Center.X - hpTextSize.X / 2f, hpBarBg.Center.Y - hpTextSize.Y / 2f), Color.White);

        // 在幽灵头顶画一个“攻击意图”的气泡框
        Rectangle intentBox = new Rectangle(_ghostBounds.Center.X - 50, _ghostBounds.Top - 50, 100, 40);
        IClickableMenu.drawTextureBox(spriteBatch, intentBox.X, intentBox.Y, intentBox.Width, intentBox.Height, Color.White);
        spriteBatch.DrawString(Game1.smallFont, "Attack", new Vector2(intentBox.X + 15, intentBox.Y + 5), Color.DarkRed);
    }

    //新增函数
    private void DrawStSUIElements(SpriteBatch spriteBatch)
    {
        // 1. 绘制能量球 (用星露谷内置的圆角框暂代)
        IClickableMenu.drawTextureBox(spriteBatch, _energyBounds.X, _energyBounds.Y, _energyBounds.Width, _energyBounds.Height, Color.Gold);
        string energyText = $"{_currentEnergy}/{_maxEnergy}";
        Vector2 eSize = Game1.dialogueFont.MeasureString(energyText);
        spriteBatch.DrawString(Game1.dialogueFont, energyText,
            new Vector2(_energyBounds.Center.X - eSize.X / 2f, _energyBounds.Center.Y - eSize.Y / 2f), Color.Black);

        // 2. 绘制抽牌堆
        IClickableMenu.drawTextureBox(spriteBatch, _drawPileBounds.X, _drawPileBounds.Y, _drawPileBounds.Width, _drawPileBounds.Height, Color.LightCyan);
        // 【修改】动态读取真实的弃牌堆数量
        spriteBatch.DrawString(Game1.smallFont, $"Draw\n  {_cardManager.DrawPile.Count}",
            new Vector2(_drawPileBounds.X + 10, _drawPileBounds.Y + 15), Game1.textColor);

        // 3. 绘制弃牌堆
        IClickableMenu.drawTextureBox(spriteBatch, _discardPileBounds.X, _discardPileBounds.Y, _discardPileBounds.Width, _discardPileBounds.Height, Color.LightGray);
        // 【修改】动态读取真实的弃牌堆数量
        spriteBatch.DrawString(Game1.smallFont, $"Discard\n   {_cardManager.DiscardPile.Count}",
            new Vector2(_discardPileBounds.X + 5, _discardPileBounds.Y + 15), Game1.textColor);
    }



    // 【全新方法】渲染实际的手牌，并加入悬停反馈
    private void DrawHand(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _cardManager.Hand.Count; i++)
        {
            var card = _cardManager.Hand[i];

            // 【关键修改】如果这张牌正在被拖拽，就不要在下面槽位里画它了！
            if (card == _draggedCard) continue;

            Rectangle drawBounds = _cardSlotBounds[i]; // 取出基础坐标

            // 如果鼠标正悬停在这张牌上，改变它的绘制坐标和大小
            if (card.IsHovered)
            {
                drawBounds.Y -= 20;       // 向上浮动20像素
                drawBounds.Inflate(10, 10); // 向四周放大10像素
            }

            // 1. 画卡牌底框
            IClickableMenu.drawTextureBox(
                spriteBatch, drawBounds.X, drawBounds.Y, drawBounds.Width, drawBounds.Height, Color.White);

            // 2. 画卡牌费用 (左上角的小黄框)
            Rectangle costBox = new Rectangle(drawBounds.X - 10, drawBounds.Y - 10, 40, 40);
            IClickableMenu.drawTextureBox(spriteBatch, costBox.X, costBox.Y, costBox.Width, costBox.Height, Color.Gold);
            Vector2 costSize = Game1.smallFont.MeasureString(card.Data.Cost.ToString());
            spriteBatch.DrawString(Game1.smallFont, card.Data.Cost.ToString(),
                new Vector2(costBox.Center.X - costSize.X / 2f, costBox.Center.Y - costSize.Y / 2f), Color.Black);

            // 3. 画卡牌名称
            Vector2 nameSize = Game1.smallFont.MeasureString(card.Data.Name);
            spriteBatch.DrawString(
                Game1.smallFont, card.Data.Name,
                new Vector2(drawBounds.Center.X - nameSize.X / 2f, drawBounds.Top + 20),
                Game1.textColor);
        }
    }


    // --- 炫酷的拖拽交互渲染 ---
    private void DrawDragState(SpriteBatch spriteBatch)
    {
        if (_draggedCard == null) return;

        // 获取鼠标实时位置
        int mouseX = Game1.getMouseX();
        int mouseY = Game1.getMouseY();

        // 1. 画一条从农夫到鼠标的连线（用星露谷内置函数）
        Utility.drawLineWithScreenCoordinates(_farmerBounds.Center.X, _farmerBounds.Center.Y, mouseX, mouseY, spriteBatch, Color.White * 0.8f, 4f);

        // 2. 根据卡牌类型，画高亮预览
        if (_draggedCard.Data.TargetType == "Enemy")
        {
            // 如果鼠标在怪物身上，把怪物高亮变红
            if (_ghostBounds.Contains(mouseX, mouseY))
            {
                IClickableMenu.drawTextureBox(spriteBatch, _ghostBounds.X - 10, _ghostBounds.Y - 10, _ghostBounds.Width + 20, _ghostBounds.Height + 20, Color.Red * 0.6f);
            }
        }
        else if (_draggedCard.Data.TargetType.StartsWith("Grid"))
        {
            // 如果指着网格，计算影响的形状
            Point? centerTile = GetGridTileAtPosition(mouseX, mouseY);
            if (centerTile.HasValue)
            {
                foreach (var offset in _draggedCard.Data.Shape)
                {
                    if (offset.Count >= 2)
                    {
                        int targetCol = centerTile.Value.X + offset[0];
                        int targetRow = centerTile.Value.Y + offset[1];

                        // 确保形状蔓延的格子没有超出 4x4 边界
                        if (targetCol >= 0 && targetCol < FieldColumnCount && targetRow >= 0 && targetRow < FieldRowCount)
                        {
                            int drawX = _fieldAreaBounds.X + targetCol * (FieldTileSize + FieldTileGap);
                            int drawY = _fieldAreaBounds.Y + targetRow * (FieldTileSize + FieldTileGap);

                            // 用星露谷内置的白色色块，染成半透明绿色画在影子上
                            spriteBatch.Draw(Game1.staminaRect, new Rectangle(drawX, drawY, FieldTileSize, FieldTileSize), Color.LightGreen * 0.5f);
                        }
                    }
                }
            }
        }

        // 3. 把被拖拽的卡牌以缩小半透明的形式挂在鼠标旁边
        Rectangle floatBounds = new Rectangle(mouseX + 20, mouseY + 20, 90, 140);
        IClickableMenu.drawTextureBox(spriteBatch, floatBounds.X, floatBounds.Y, floatBounds.Width, floatBounds.Height, Color.White * 0.8f);
        spriteBatch.DrawString(Game1.smallFont, _draggedCard.Data.Name, new Vector2(floatBounds.X + 10, floatBounds.Top + 20), Game1.textColor);
    }

    private void DrawButton(SpriteBatch spriteBatch, Rectangle buttonBounds, string text)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            buttonBounds.X,
            buttonBounds.Y,
            buttonBounds.Width,
            buttonBounds.Height,
            Color.White);

        SpriteFont font = buttonBounds.Width <= 64 ? Game1.smallFont : Game1.dialogueFont;
        Vector2 textSize = font.MeasureString(text);

        spriteBatch.DrawString(
            font,
            text,
            new Vector2(
                buttonBounds.Center.X - textSize.X / 2f,
                buttonBounds.Center.Y - textSize.Y / 2f),
            Game1.textColor);
    }
}