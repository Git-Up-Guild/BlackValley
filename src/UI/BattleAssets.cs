using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace BlackValley;

/// <summary>
/// 战斗界面资源集合
/// 负责集中加载战斗菜单依赖的贴图资源
/// </summary>
public sealed class BattleAssets
{
    private readonly IModHelper _helper;
    private readonly Dictionary<string, Texture2D> _cardIconTextures = new(StringComparer.Ordinal);
    private readonly HashSet<string> _missingCardIconIds = new(StringComparer.Ordinal);

    public Texture2D FieldTileTexture { get; } // 战斗场地格子纹理
    public Texture2D FarmerTexture { get; } // 农夫立绘纹理
    public Texture2D GhostTexture { get; } // 怪物立绘纹理

    /// <summary>
    /// 加载战斗界面使用的贴图资源
    /// </summary>
    /// <param name="helper">SMAPI 提供的模组辅助接口</param>
    public BattleAssets(IModHelper helper)
    {
        _helper = helper;
        FieldTileTexture = helper.ModContent.Load<Texture2D>("assets/ui/field_tile.png");
        FarmerTexture = helper.ModContent.Load<Texture2D>("assets/ui/farmer_idle.png");
        GhostTexture = helper.ModContent.Load<Texture2D>("assets/ui/ghost_idle.png");
    }

    /// <summary>
    /// 根据卡牌图标标识获取对应贴图
    /// 图标文件默认从 assets/ui/cards 目录按 IconId 同名读取
    /// </summary>
    /// <param name="iconId">卡牌配置中的图标标识</param>
    /// <param name="texture">读取成功后的贴图资源</param>
    public bool TryGetCardIconTexture(string iconId, out Texture2D texture)
    {
        if (string.IsNullOrWhiteSpace(iconId))
        {
            texture = null!;
            return false;
        }

        if (_cardIconTextures.TryGetValue(iconId, out texture!))
        {
            return true;
        }

        if (_missingCardIconIds.Contains(iconId))
        {
            texture = null!;
            return false;
        }

        string relativePath = $"assets/ui/cards/{iconId}.png";
        string absolutePath = Path.Combine(_helper.DirectoryPath, "assets", "ui", "cards", $"{iconId}.png");

        if (!File.Exists(absolutePath))
        {
            _missingCardIconIds.Add(iconId);
            texture = null!;
            return false;
        }

        texture = _helper.ModContent.Load<Texture2D>(relativePath);
        _cardIconTextures[iconId] = texture;
        return true;
    }
}
