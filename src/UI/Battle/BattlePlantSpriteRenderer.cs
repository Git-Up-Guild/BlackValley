using System;
using System.Collections.Generic;
using BlackValley.Grid;
using BlackValley.Plants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Crops;

namespace BlackValley.UI.Battle;

/// <summary>
/// 战斗地块中的植物立绘渲染辅助
/// 负责直接复用星露谷原版作物贴图和取帧规则，但不依赖真实场景中的 Crop 对象
/// </summary>
internal static class BattlePlantSpriteRenderer
{
    private const int UnripeDisplayPhase = 2;
    private const int MatureDisplayFrame = 6;

    private static readonly HashSet<string> LoggedMessages = new(StringComparer.Ordinal);

    /// <summary>
    /// 使用星露谷原版 crops 图集绘制当前植物
    /// 未成熟统一显示第三阶段，成熟统一显示成熟阶段
    /// </summary>
    /// <param name="spriteBatch">当前用于绘制菜单的 SpriteBatch</param>
    /// <param name="tileState">目标地块的运行时状态</param>
    /// <param name="tileBounds">地块在屏幕上的绘制区域</param>
    public static bool TryDrawPlant(SpriteBatch spriteBatch, BattleGridTileState tileState, Rectangle tileBounds)
    {
        if (!tileState.HasPlant)
        {
            return false;
        }

        if (!ModEntry.PlantDatabase.TryGetValue(tileState.PlantId, out PlantData? plantData))
        {
            LogOnce($"missing-plant-data:{tileState.PlantId}", $"Plant sprite fallback | Missing PlantData for {tileState.PlantId}");
            return false;
        }

        if (string.IsNullOrWhiteSpace(plantData.VanillaSeedId))
        {
            LogOnce($"missing-seed:{tileState.PlantId}", $"Plant sprite fallback | {plantData.Id} has no VanillaSeedId");
            return false;
        }

        if (!Crop.TryGetData(plantData.VanillaSeedId, out CropData? cropData) || cropData == null)
        {
            LogOnce($"missing-crop-data:{tileState.PlantId}", $"Plant sprite fallback | Crop.TryGetData failed for seed {plantData.VanillaSeedId}");
            return false;
        }

        if (Game1.cropSpriteSheet == null)
        {
            LogOnce($"missing-crop-sheet:{tileState.PlantId}", $"Plant sprite fallback | Game1.cropSpriteSheet is null for {plantData.Id}");
            return false;
        }

        Rectangle sourceRect = GetSourceRect(cropData.SpriteIndex, tileState.IsMature);
        Vector2 drawPosition = new(tileBounds.Center.X, tileBounds.Y + 32f);

        spriteBatch.Draw(
            Game1.cropSpriteSheet,
            drawPosition,
            sourceRect,
            Color.White,
            0f,
            new Vector2(8f, 24f),
            4f,
            SpriteEffects.None,
            0.6f + tileBounds.Bottom / 10000f);

        LogOnce(
            $"draw-success:{plantData.Id}:{tileState.IsMature}",
            $"Plant sprite draw | Plant: {plantData.Id} | Seed: {plantData.VanillaSeedId} | SpriteIndex: {cropData.SpriteIndex} | Mature: {tileState.IsMature} | SourceRect: {sourceRect}");

        return true;
    }

    // 原版作物每两行共用一组横向阶段帧，奇数行需要向右偏移 128 像素
    private static Rectangle GetSourceRect(int spriteIndex, bool isMature)
    {
        int frame = isMature ? MatureDisplayFrame : UnripeDisplayPhase + 1;
        int xOffset = spriteIndex % 2 != 0 ? 128 : 0;
        int x = Math.Min(240, frame * 16 + xOffset);
        int y = spriteIndex / 2 * 32;
        return new Rectangle(x, y, 16, 32);
    }

    // 植物渲染每帧都会走，日志只记录一次，避免把控制台刷满
    private static void LogOnce(string key, string message, StardewModdingAPI.LogLevel level = StardewModdingAPI.LogLevel.Info)
    {
        if (!LoggedMessages.Add(key) || ModEntry.Logger == null)
        {
            return;
        }

        ModEntry.Logger.Log(message, level);
    }
}
