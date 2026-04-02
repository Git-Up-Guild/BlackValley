using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace BlackValley.World;

/// <summary>
/// 农场世界里的静态遭遇幽灵。
/// 它不会四处移动，但会做轻微漂浮和帧动画，作为战斗入口的视觉锚点。
/// </summary>
internal sealed class FarmEncounterGhost
{
    private const int AnimationFrameCount = 4;
    private const int AnimationFrameDurationTicks = 10;
    private const int VanillaGhostFrameWidth = 16;
    private const int VanillaGhostFrameHeight = 24;
    private const float VanillaGhostScale = 4f;
    private const float FallbackGhostScale = 0.18f;
    private const float ShadowScale = 3.2f;
    private const float BobAmplitude = 5f;
    private const float BobSpeed = 2.4f;

    private readonly string _locationName;
    private readonly Vector2 _anchorPixel;
    private readonly float _phaseOffset;

    private int _animationTick;

    public FarmEncounterGhost(string locationName, Vector2 anchorPixel, int index)
    {
        _locationName = locationName;
        _anchorPixel = anchorPixel;
        _phaseOffset = index * 0.75f;
    }

    public bool IsInLocation(GameLocation location)
    {
        return string.Equals(location.NameOrUniqueName, _locationName, StringComparison.Ordinal);
    }

    public bool IsPlayerInTriggerRange(GameLocation location, Vector2 playerStandingPixel)
    {
        if (!IsInLocation(location))
        {
            return false;
        }

        float maxAxisDistance = Game1.tileSize;
        return MathF.Abs(playerStandingPixel.X - _anchorPixel.X) <= maxAxisDistance
            && MathF.Abs(playerStandingPixel.Y - _anchorPixel.Y) <= maxAxisDistance;
    }

    public bool MatchesLocationAndAnchor(string locationName, Vector2 anchorPixel)
    {
        return string.Equals(_locationName, locationName, StringComparison.Ordinal)
            && Vector2.DistanceSquared(_anchorPixel, anchorPixel) < 1f;
    }

    public Vector2 GetDeathEffectPosition()
    {
        return new Vector2(_anchorPixel.X - 32f, _anchorPixel.Y - 64f);
    }

    public void Update()
    {
        _animationTick = (_animationTick + 1) % (AnimationFrameCount * AnimationFrameDurationTicks);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D texture, bool isUsingFallbackTexture)
    {
        Rectangle? sourceRect = isUsingFallbackTexture
            ? null
            : TryGetAnimatedSourceRect(texture);

        float elapsedSeconds = (float)Game1.currentGameTime.TotalGameTime.TotalSeconds;
        float bobOffset = MathF.Sin(elapsedSeconds * BobSpeed + _phaseOffset) * BobAmplitude;
        float alpha = 0.9f + (MathF.Sin(elapsedSeconds * 1.85f + _phaseOffset) * 0.08f);
        Vector2 worldPosition = new(_anchorPixel.X, _anchorPixel.Y + bobOffset);

        DrawShadow(spriteBatch, worldPosition);

        Rectangle drawSourceRect = sourceRect ?? texture.Bounds;
        Vector2 origin = new(drawSourceRect.Width / 2f, drawSourceRect.Height);
        float scale = isUsingFallbackTexture ? FallbackGhostScale : VanillaGhostScale;
        Vector2 screenPosition = Game1.GlobalToLocal(Game1.viewport, worldPosition);
        float layerDepth = Math.Max(0f, (_anchorPixel.Y + 32f) / 10000f);

        spriteBatch.Draw(
            texture,
            screenPosition,
            sourceRect,
            Color.White * alpha,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            layerDepth);
    }

    private void DrawShadow(SpriteBatch spriteBatch, Vector2 worldPosition)
    {
        Vector2 shadowPosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(worldPosition.X, _anchorPixel.Y + 6f));
        Vector2 shadowOrigin = new(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y);

        spriteBatch.Draw(
            Game1.shadowTexture,
            shadowPosition,
            null,
            Color.White * 0.4f,
            0f,
            shadowOrigin,
            ShadowScale,
            SpriteEffects.None,
            Math.Max(0f, (_anchorPixel.Y - 1f) / 10000f));
    }

    private Rectangle? TryGetAnimatedSourceRect(Texture2D texture)
    {
        if (texture.Width < VanillaGhostFrameWidth * AnimationFrameCount || texture.Height < VanillaGhostFrameHeight)
        {
            return null;
        }

        int frameIndex = (_animationTick / AnimationFrameDurationTicks) % AnimationFrameCount;
        return new Rectangle(frameIndex * VanillaGhostFrameWidth, 0, VanillaGhostFrameWidth, VanillaGhostFrameHeight);
    }
}
