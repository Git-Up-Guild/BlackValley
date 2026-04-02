using StardewModdingAPI.Utilities;
namespace BlackValley;

/// <summary>
/// 模组配置数据
/// 当前用于维护调试热键与语言切换热键
/// </summary>
public sealed class ModConfig
{
    // 旧版战斗调试热键，暂时保留配置兼容，当前正常遭遇改为靠近幽灵触发
    public KeybindList ToggleBattleMenuKey { get; set; } = KeybindList.Parse("K");

    // 打印当前位置的调试热键，默认值为 J
    public KeybindList PrintPlayerPositionKey { get; set; } = KeybindList.Parse("J");

    // 一键切换中英文显示的热键，默认值为 U
    public KeybindList ToggleLocalizationKey { get; set; } = KeybindList.Parse("U");

    // 是否默认使用中文显示
    public bool UseChineseLocalization { get; set; }
}
