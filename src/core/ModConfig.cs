using StardewModdingAPI.Utilities;
namespace BlackValley;

/// <summary>
/// 模组配置数据
/// 当前用于维护战斗菜单的开关热键
/// </summary>
public sealed class ModConfig
{
    // 打开或关闭战斗菜单的热键，默认值为 K
    public KeybindList ToggleBattleMenuKey { get; set; } = KeybindList.Parse("K");
}
