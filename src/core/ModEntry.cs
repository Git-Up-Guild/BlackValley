using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BlackValley;

public sealed class ModEntry : Mod
{
    private ModConfig _config = null!;
    private BattleAssets _battleAssets = null!;

    //新增
    public static Dictionary<string, CardData> CardDatabase = new();
    public static Dictionary<string, PlantData> PlantDatabase = new();

    public override void Entry(IModHelper helper)
    {
        _config = helper.ReadConfig<ModConfig>();
        _battleAssets = new BattleAssets(helper);


        // 尝试从 assets 文件夹读取 JSON 数据。如果没有文件，返回空列表   新增
        var cards = helper.Data.ReadJsonFile<List<CardData>>("assets/CardData.json") ?? new List<CardData>();
        foreach (var card in cards)
        {
            CardDatabase[card.Id] = card;
        }

        var plants = helper.Data.ReadJsonFile<List<PlantData>>("assets/PlantData.json") ?? new List<PlantData>();
        foreach (var plant in plants)
        {
            PlantDatabase[plant.Id] = plant;
        }



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