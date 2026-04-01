using System;
using BlackValley.Battle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace BlackValley.UI.Battle;

public sealed partial class BattleMenu
{
    private const float RoundIntroDurationSeconds = 0.9f;
    private const float RoundIntroStartScale = 2.5f;
    private const float RoundIntroEndScale = 1f;

    private int _roundIntroTurn = -1;
    private float _roundIntroElapsedSeconds;

    /// <summary>
    /// 更新回合开场动画
    /// 动画结束后再解锁玩家操作
    /// </summary>
    /// <param name="time">当前帧的时间信息</param>
    public override void update(GameTime time)
    {
        base.update(time);
        UpdateRoundIntro(time);
    }

    // 回合开始时播放中间的 Round 文案缩放动画
    private void UpdateRoundIntro(GameTime time)
    {
        if (_battleController.State.CurrentPhase != BattleTurnPhase.RoundIntro || _battleController.State.IsBattleOver)
        {
            return;
        }

        if (_roundIntroTurn != _battleController.State.CurrentTurn)
        {
            _roundIntroTurn = _battleController.State.CurrentTurn;
            _roundIntroElapsedSeconds = 0f;
        }

        _roundIntroElapsedSeconds += (float)time.ElapsedGameTime.TotalSeconds;
        if (_roundIntroElapsedSeconds < RoundIntroDurationSeconds)
        {
            return;
        }

        _roundIntroElapsedSeconds = RoundIntroDurationSeconds;
        _battleController.CompleteRoundIntro();
    }

    // 动画期间在屏幕中央绘制从大变小的回合文字
    private void DrawRoundIntroOverlay(SpriteBatch spriteBatch)
    {
        if (_battleController.State.CurrentPhase != BattleTurnPhase.RoundIntro || _battleController.State.IsBattleOver)
        {
            return;
        }

        float progress = Math.Clamp(_roundIntroElapsedSeconds / RoundIntroDurationSeconds, 0f, 1f);
        float easedProgress = 1f - MathF.Pow(1f - progress, 3f);
        BattleTextStyle textStyle = BattleTextStyles.RoundIntro;
        float scale = textStyle.Scale * MathHelper.Lerp(RoundIntroStartScale, RoundIntroEndScale, easedProgress);
        float alpha = MathHelper.Lerp(1f, 0.78f, easedProgress);
        string roundText = ModLocalization.GetRoundIntroLabel(_battleController.State.CurrentTurn);

        Vector2 textSize = textStyle.Font.MeasureString(roundText) * scale;
        Vector2 textPosition = new(
            _layout.PanelBounds.Center.X - textSize.X / 2f,
            _layout.PanelBounds.Center.Y - textSize.Y / 2f - 30f);

        spriteBatch.DrawString(
            textStyle.Font,
            roundText,
            textPosition + new Vector2(4f, 4f),
            Color.Black * (0.45f * alpha),
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            1f);

        spriteBatch.DrawString(
            textStyle.Font,
            roundText,
            textPosition,
            Color.Gold * alpha,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            1f);
    }
}
