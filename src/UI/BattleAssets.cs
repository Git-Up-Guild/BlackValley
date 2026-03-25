using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace BlackValley;

public sealed class BattleAssets
{
    public Texture2D FieldTileTexture { get; } // 战斗场地的格子纹理
    public Texture2D FarmerTexture { get; } // 农民的纹理
    public Texture2D GhostTexture { get; } // 史莱姆的纹理

    public BattleAssets(IModHelper helper)
    {
        FieldTileTexture = helper.ModContent.Load<Texture2D>("assets/ui/field_tile.png");
        FarmerTexture = helper.ModContent.Load<Texture2D>("assets/ui/farmer_idle.png");
        GhostTexture = helper.ModContent.Load<Texture2D>("assets/ui/ghost_idle.png");
    }
}