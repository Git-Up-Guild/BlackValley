using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace BlackValley;

internal static class ModFontManager
{
    private const string ChineseDialogueFontAsset = "Fonts/SpriteFont1.zh-CN";
    private const string ChineseSmallFontAsset = "Fonts/SmallFont.zh-CN";
    private const string ChineseRoundDialogueFontAsset = "Fonts/Chinese_round/SpriteFont1.zh-CN";
    private const string ChineseRoundSmallFontAsset = "Fonts/Chinese_round/SmallFont.zh-CN";

    private static IModHelper? _helper;
    private static IMonitor? _monitor;
    private static SpriteFont? _chineseDialogueFont;
    private static SpriteFont? _chineseSmallFont;

    public static void Initialize(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor = monitor;
    }

    public static SpriteFont GetDialogueFont()
    {
        if (!ModLocalization.UseChinese)
        {
            return Game1.dialogueFont;
        }

        return _chineseDialogueFont ??= LoadChineseFont(
            ChineseDialogueFontAsset,
            ChineseRoundDialogueFontAsset,
            Game1.dialogueFont);
    }

    public static SpriteFont GetSmallFont()
    {
        if (!ModLocalization.UseChinese)
        {
            return Game1.smallFont;
        }

        return _chineseSmallFont ??= LoadChineseFont(
            ChineseSmallFontAsset,
            ChineseRoundSmallFontAsset,
            Game1.smallFont);
    }

    private static SpriteFont LoadChineseFont(string primaryAssetName, string fallbackAssetName, SpriteFont defaultFont)
    {
        if (_helper == null)
        {
            return defaultFont;
        }

        try
        {
            return _helper.GameContent.Load<SpriteFont>(primaryAssetName);
        }
        catch
        {
            try
            {
                return _helper.GameContent.Load<SpriteFont>(fallbackAssetName);
            }
            catch
            {
                _monitor?.Log(
                    $"Failed to load Chinese font assets '{primaryAssetName}' and '{fallbackAssetName}', falling back to default font.",
                    LogLevel.Warn);
                return defaultFont;
            }
        }
    }
}
