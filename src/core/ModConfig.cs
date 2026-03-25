using StardewModdingAPI.Utilities;

namespace BlackValley;

public sealed class ModConfig
{
    // 打开/关闭战斗菜单的热键，默认 K
    public KeybindList ToggleBattleMenuKey { get; set; } = KeybindList.Parse("K");
}