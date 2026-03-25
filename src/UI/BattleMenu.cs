using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BlackValley;

public sealed class BattleMenu : IClickableMenu
{
    private readonly BattleAssets _battleAssets;

    private readonly Rectangle _panelBounds;
    private readonly Rectangle _endTurnButtonBounds;
    private readonly Rectangle _closeButtonBounds;

    private readonly Rectangle _fieldAreaBounds;
    private readonly Rectangle _farmerBounds;
    private readonly Rectangle _ghostBounds;

    private readonly Rectangle[] _cardSlotBounds;

    private int _currentTurn = 1;

    private const int FieldRowCount = 4;
    private const int FieldColumnCount = 4;
    private const int FieldTileSize = 64;
    private const int FieldTileGap = 4;

    public BattleMenu(BattleAssets battleAssets)
        : base(
            (Game1.uiViewport.Width - 1100) / 2,
            (Game1.uiViewport.Height - 760) / 2,
            1100,
            760,
            showUpperRightCloseButton: false)
    {
        _battleAssets = battleAssets;

        _panelBounds = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);

        _closeButtonBounds = new Rectangle(
            _panelBounds.Right - 70,
            _panelBounds.Top + 20,
            40,
            40);

        _endTurnButtonBounds = new Rectangle(
            _panelBounds.Right - 230,
            _panelBounds.Bottom - 95,
            170,
            60);

        int fieldWidth = FieldColumnCount * FieldTileSize + (FieldColumnCount - 1) * FieldTileGap;
        int fieldHeight = FieldRowCount * FieldTileSize + (FieldRowCount - 1) * FieldTileGap;

        _fieldAreaBounds = new Rectangle(
            _panelBounds.Center.X - fieldWidth / 2,
            _panelBounds.Top + 170,
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

        _cardSlotBounds = new Rectangle[3];

        int cardWidth = 150;
        int cardHeight = 210;
        int cardGap = 26;
        int totalCardWidth = cardWidth * 3 + cardGap * 2;
        int cardStartX = _panelBounds.Center.X - totalCardWidth / 2;
        int cardY = _panelBounds.Bottom - 270;

        for (int index = 0; index < 3; index++)
        {
            _cardSlotBounds[index] = new Rectangle(
                cardStartX + index * (cardWidth + cardGap),
                cardY,
                cardWidth,
                cardHeight);
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (_endTurnButtonBounds.Contains(x, y))
        {
            _currentTurn++;
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
        DrawCardSlots(spriteBatch);
        DrawButton(spriteBatch, _endTurnButtonBounds, "End Turn");
        DrawButton(spriteBatch, _closeButtonBounds, "X");

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
                _panelBounds.Top + 28),
            Game1.textColor);

        string turnText = $"Turn: {_currentTurn}";
        spriteBatch.DrawString(
            Game1.smallFont,
            turnText,
            new Vector2(_panelBounds.Left + 48, _panelBounds.Top + 105),
            Game1.textColor);
    }

    private void DrawFieldGrid(SpriteBatch spriteBatch)
    {
        for (int row = 0; row < FieldRowCount; row++)
        {
            for (int column = 0; column < FieldColumnCount; column++)
            {
                int drawX = _fieldAreaBounds.X + column * (FieldTileSize + FieldTileGap);
                int drawY = _fieldAreaBounds.Y + row * (FieldTileSize + FieldTileGap);

                Rectangle tileBounds = new Rectangle(drawX, drawY, FieldTileSize, FieldTileSize);

                spriteBatch.Draw(_battleAssets.FieldTileTexture, tileBounds, Color.White);
            }
        }
    }

    private void DrawCharacters(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_battleAssets.FarmerTexture, _farmerBounds, Color.White);
        spriteBatch.Draw(_battleAssets.GhostTexture, _ghostBounds, Color.White);

        spriteBatch.DrawString(
            Game1.smallFont,
            "Farmer",
            new Vector2(_farmerBounds.X + 24, _farmerBounds.Bottom + 10),
            Game1.textColor);

        spriteBatch.DrawString(
            Game1.smallFont,
            "Ghost",
            new Vector2(_ghostBounds.X + 30, _ghostBounds.Bottom + 10),
            Game1.textColor);
    }

    private void DrawCardSlots(SpriteBatch spriteBatch)
    {
        for (int index = 0; index < _cardSlotBounds.Length; index++)
        {
            Rectangle cardSlotBounds = _cardSlotBounds[index];

            IClickableMenu.drawTextureBox(
                spriteBatch,
                cardSlotBounds.X,
                cardSlotBounds.Y,
                cardSlotBounds.Width,
                cardSlotBounds.Height,
                Color.White);

            string slotText = $"Card {index + 1}";
            Vector2 textSize = Game1.smallFont.MeasureString(slotText);

            spriteBatch.DrawString(
                Game1.smallFont,
                slotText,
                new Vector2(
                    cardSlotBounds.Center.X - textSize.X / 2f,
                    cardSlotBounds.Center.Y - textSize.Y / 2f),
                Game1.textColor);
        }
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