using StardewModdingAPI.Utilities;
namespace BlackValley;

/// <summary>
/// 模组配置数据
/// 当前用于维护战斗菜单与语言切换热键
/// </summary>
public sealed class ModConfig
{
    // 打开或关闭战斗菜单的热键，默认值为 K
    public KeybindList ToggleBattleMenuKey { get; set; } = KeybindList.Parse("K");

    // 打印当前位置的调试热键，默认值为 J
    public KeybindList PrintPlayerPositionKey { get; set; } = KeybindList.Parse("J");

    // 一键切换中英文显示的热键，默认值为 U
    public KeybindList ToggleLocalizationKey { get; set; } = KeybindList.Parse("U");

    // 是否默认使用中文显示
    public bool UseChineseLocalization { get; set; }
}
