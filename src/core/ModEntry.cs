using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BlackValley;

public sealed class ModEntry : Mod
{
    private ModConfig _config = null!;
    private BattleAssets _battleAssets = null!;

    public override void Entry(IModHelper helper)
    {
        _config = helper.ReadConfig<ModConfig>();
        _battleAssets = new BattleAssets(helper);

        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs eventArgs)
    {
        if (!_config.ToggleBattleMenuKey.JustPressed())
            return;

        Helper.Input.SuppressActiveKeybinds(_config.ToggleBattleMenuKey);

        // 已经打开菜单时，再按一次就关闭
        if (Game1.activeClickableMenu is BattleMenu)
        {
            Game1.exitActiveMenu();
            return;
        }

        Game1.activeClickableMenu = new BattleMenu(_battleAssets);
    }
}