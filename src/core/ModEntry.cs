using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using BlackValley.Cards;
using BlackValley.Monsters;
using BlackValley.Plants;
using BlackValley.UI.Battle;

namespace BlackValley;

/// <summary>
/// 模组主入口
/// 负责初始化配置、加载静态数据，并在按下热键时打开战斗菜单
/// </summary>
public sealed class ModEntry : Mod
{
    private ModConfig _config = null!;
    private BattleAssets _battleAssets = null!;

    public static IMonitor Logger { get; private set; } = null!;

    public static Dictionary<string, CardData> CardDatabase = new();
    public static Dictionary<string, PlantData> PlantDatabase = new();
    public static Dictionary<string, EnemyBattleData> EnemyDatabase = new();

    /// <summary>
    /// 模组加载入口
    /// 负责读取配置、加载资源和注册输入事件
    /// </summary>
    /// <param name="helper">SMAPI 提供的模组辅助接口</param>
    public override void Entry(IModHelper helper)
    {
        Logger = Monitor;
        _config = helper.ReadConfig<ModConfig>();
        ModLocalization.SetUseChinese(_config.UseChineseLocalization);
        ModFontManager.Initialize(helper, Monitor);
        _battleAssets = new BattleAssets(helper);

        CardDatabase.Clear();
        PlantDatabase.Clear();
        EnemyDatabase.Clear();

        // 尝试从当前项目中的 Json 配置目录读取数据
        // 如果文件不存在，则回退为空列表，避免启动阶段直接报错
        var cards = helper.Data.ReadJsonFile<List<CardData>>("src/Json/CardData.json") ?? new List<CardData>();
        foreach (var card in cards)
        {
            CardDatabase[card.Id] = card;
        }

        var plants = helper.Data.ReadJsonFile<List<PlantData>>("src/Json/PlantData.json") ?? new List<PlantData>();
        foreach (var plant in plants)
        {
            PlantDatabase[plant.Id] = plant;
        }

        var enemies = helper.Data.ReadJsonFile<List<EnemyBattleData>>("src/Json/EnemyBattleData.json") ?? new List<EnemyBattleData>();
        foreach (var enemy in enemies)
        {
            EnemyDatabase[enemy.Id] = enemy;
        }

        helper.Events.Input.ButtonsChanged += OnButtonsChanged;

        Monitor.Log(
            $"BlackValley loaded | Hotkey: {_config.ToggleBattleMenuKey} | Position Debug: {_config.PrintPlayerPositionKey} | Language Toggle: {_config.ToggleLocalizationKey} | Language: {ModLocalization.GetLanguageDisplayName()} | Cards: {CardDatabase.Count} | Plants: {PlantDatabase.Count} | Enemies: {EnemyDatabase.Count}",
            LogLevel.Info);
    }

    // 统一在按键变化时处理开关逻辑
    // 这里按 SMAPI 推荐做法使用 ButtonsChanged + JustPressed
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs eventArgs)
    {
        if (_config.PrintPlayerPositionKey.JustPressed())
        {
            HandlePrintPlayerPosition();
            return;
        }

        if (_config.ToggleLocalizationKey.JustPressed())
        {
            HandleLocalizationToggle();
            return;
        }

        if (!_config.ToggleBattleMenuKey.JustPressed())
        {
            return;
        }

        Monitor.Log($"Battle menu hotkey detected: {_config.ToggleBattleMenuKey}", LogLevel.Info);
        Helper.Input.SuppressActiveKeybinds(_config.ToggleBattleMenuKey);

        if (Game1.activeClickableMenu is BattleMenu)
        {
            Monitor.Log("Closing active battle menu", LogLevel.Info);
            Game1.exitActiveMenu();
            return;
        }

        try
        {
            Monitor.Log("Opening battle menu", LogLevel.Info);
            Game1.activeClickableMenu = new BattleMenu(_battleAssets);
        }
        catch (Exception exception)
        {
            Monitor.Log($"Failed to open battle menu: {exception}", LogLevel.Error);
        }
    }

    // 允许在游戏运行时通过热键即时切换语言，并把选择写回配置
    private void HandleLocalizationToggle()
    {
        _config.UseChineseLocalization = !_config.UseChineseLocalization;
        ModLocalization.SetUseChinese(_config.UseChineseLocalization);
        Helper.WriteConfig(_config);
        Helper.Input.SuppressActiveKeybinds(_config.ToggleLocalizationKey);

        Monitor.Log(
            $"Localization toggled | Current language: {ModLocalization.GetLanguageDisplayName()}",
            LogLevel.Info);
    }

    // 输出玩家当前所在地图、地块坐标和像素坐标，方便后续布置遭遇点
    private void HandlePrintPlayerPosition()
    {
        Helper.Input.SuppressActiveKeybinds(_config.PrintPlayerPositionKey);

        if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null)
        {
            Monitor.Log("Position debug requested, but no save is currently loaded.", LogLevel.Warn);
            return;
        }

        var standingPixel = Game1.player.getStandingPosition();
        var rawPosition = Game1.player.Position;
        float tileX = standingPixel.X / (float)Game1.tileSize;
        float tileY = standingPixel.Y / (float)Game1.tileSize;

        string message =
            $"Player position | Location: {Game1.currentLocation.NameOrUniqueName} | Tile: ({tileX:0.##}, {tileY:0.##}) | Standing Pixel: ({standingPixel.X}, {standingPixel.Y}) | Position: ({rawPosition.X:0.##}, {rawPosition.Y:0.##})";

        Monitor.Log(message, LogLevel.Info);
        Game1.addHUDMessage(new HUDMessage($"Tile ({tileX:0.##}, {tileY:0.##})", HUDMessage.newQuest_type));
    }
}
