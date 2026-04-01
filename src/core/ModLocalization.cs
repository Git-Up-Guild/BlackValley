using System;
using BlackValley.Cards;
using BlackValley.Monsters;

namespace BlackValley;

internal static class ModLocalization
{
    public static bool UseChinese { get; private set; }

    public static void SetUseChinese(bool useChinese)
    {
        UseChinese = useChinese;
    }

    public static bool ToggleLanguage()
    {
        UseChinese = !UseChinese;
        return UseChinese;
    }

    public static string Select(string english, string chinese)
    {
        return UseChinese ? chinese : english;
    }

    public static string GetLanguageDisplayName()
    {
        return UseChinese ? "中文" : "English";
    }

    public static string GetBattleTitle()
    {
        return Select("Black Valley Battle Prototype", "黑谷战斗原型");
    }

    public static string GetEndTurnLabel()
    {
        return Select("End Turn", "结束回合");
    }

    public static string GetTurnLabel(int turn)
    {
        return Select($"Turn: {turn}", $"回合：{turn}");
    }

    public static string GetFarmHealthLabel(int currentHealth, int maxHealth)
    {
        return Select($"Farm HP: {currentHealth} / {maxHealth}", $"农田生命：{currentHealth} / {maxHealth}");
    }

    public static string GetPersistentGuardLabel(int charges)
    {
        return Select($"Guard {charges}", $"守护 {charges}");
    }

    public static string GetShieldLabel(int charges)
    {
        return Select($"Shield {charges}", $"护盾 {charges}");
    }

    public static string GetFarmerLabel()
    {
        return Select("Farmer", "农夫");
    }

    public static string GetEnemyName(EnemyBattleData enemyData)
    {
        return enemyData.Id switch
        {
            "slime" => Select(enemyData.Name, "史莱姆"),
            "Ghost" => Select(enemyData.Name, "幽灵"),
            _ when string.Equals(enemyData.Name, "Slime", StringComparison.Ordinal) => Select(enemyData.Name, "史莱姆"),
            _ when string.Equals(enemyData.Name, "Ghost", StringComparison.Ordinal) => Select(enemyData.Name, "幽灵"),
            _ => enemyData.Name
        };
    }

    public static string GetMonsterIntentLabel(int intentCount)
    {
        return intentCount > 0
            ? Select($"Infect x{intentCount}", $"感染 x{intentCount}")
            : Select("Idle", "待机");
    }

    public static string GetDrawPileLabel(int drawPileCount)
    {
        return Select($"Draw\n  {drawPileCount}", $"抽牌堆\n  {drawPileCount}");
    }

    public static string GetDiscardPileLabel(int discardPileCount)
    {
        return Select($"Discard\n   {discardPileCount}", $"弃牌堆\n   {discardPileCount}");
    }

    public static string GetAttackBonusLabel(int bonusDamage)
    {
        return Select($"Attack +{bonusDamage}", $"攻击 +{bonusDamage}");
    }

    public static string GetInfectionReductionLabel(int reduction)
    {
        return Select($"Infect -{reduction}", $"感染 -{reduction}");
    }

    public static string GetRoundIntroLabel(int turn)
    {
        return Select($"Round {turn}", $"第 {turn} 回合");
    }

    public static string GetHarvestLabel()
    {
        return Select("Harvest", "收割");
    }

    public static string GetGrowLabel(int turnsRemaining)
    {
        return Select($"Grow {turnsRemaining}", $"成长 {turnsRemaining}");
    }

    public static string GetBattleResultLabel(string resultText)
    {
        return resultText switch
        {
            "Victory" => Select(resultText, "胜利"),
            "Farm Lost" => Select(resultText, "农场失守"),
            _ => resultText
        };
    }

    public static string GetBattleCloseHint()
    {
        return Select("Press close to exit", "按关闭按钮退出");
    }

    public static string GetCardFallbackTypeLabel(string cardType)
    {
        return cardType switch
        {
            "Attack" => Select(cardType, "攻击"),
            "Seed" => Select(cardType, "种子"),
            "Draw" => Select(cardType, "抽牌"),
            "Defense" => Select(cardType, "防御"),
            "Infection" => Select(cardType, "感染"),
            _ => cardType
        };
    }

    public static string GetCardName(CardData cardData)
    {
        if (cardData.Id.StartsWith("test_strike_", StringComparison.Ordinal))
        {
            return Select(cardData.Name, "基础斩击");
        }

        if (cardData.Id.StartsWith("test_seed_", StringComparison.Ordinal))
        {
            return Select(cardData.Name, "防风草种子");
        }

        if (cardData.Id.StartsWith("test_draw_", StringComparison.Ordinal))
        {
            return Select(cardData.Name, "快速抽牌");
        }

        if (cardData.Id.StartsWith("test_cross_", StringComparison.Ordinal))
        {
            return Select(cardData.Name, "十字护盾");
        }

        return cardData.Id switch
        {
            "player_attack_basic" => Select(cardData.Name, "基础斩击"),
            "player_draw_quick" => Select(cardData.Name, "快速抽牌"),
            "player_seed_parsnip" => Select(cardData.Name, "防风草种子"),
            "player_seed_strawberry" => Select(cardData.Name, "草莓种子"),
            "player_seed_starfruit" => Select(cardData.Name, "杨桃种子"),
            "player_seed_blueberry" => Select(cardData.Name, "蓝莓种子"),
            "player_defense_line3_horizontal" => Select(cardData.Name, "横向守护"),
            "player_defense_line3_vertical" => Select(cardData.Name, "纵向守护"),
            "player_defense_cross" => Select(cardData.Name, "十字守护"),
            "enemy_infection_single" => Select(cardData.Name, "单格感染"),
            "enemy_infection_line3_horizontal" => Select(cardData.Name, "横向三格感染"),
            "enemy_infection_line3_vertical" => Select(cardData.Name, "纵向三格感染"),
            "enemy_infection_cross" => Select(cardData.Name, "十字感染"),
            "fallback_enemy_infection_single" => Select(cardData.Name, "单格感染"),
            _ => cardData.Name
        };
    }

    public static string GetCardDescription(CardData cardData)
    {
        if (cardData.Id.StartsWith("test_strike_", StringComparison.Ordinal))
        {
            return Select("Deal 6 damage to the monster", "对怪物造成 6 点伤害");
        }

        if (cardData.Id.StartsWith("test_seed_", StringComparison.Ordinal))
        {
            return Select("Plant a seed on an empty tile", "在空地块上种下一颗种子");
        }

        if (cardData.Id.StartsWith("test_draw_", StringComparison.Ordinal))
        {
            return Select("Draw 1 card", "抽 1 张牌");
        }

        if (cardData.Id.StartsWith("test_cross_", StringComparison.Ordinal))
        {
            return Select("This turn\n5 cross tiles block 1 infection each", "本回合\n十字 5 格各抵挡 1 次感染");
        }

        return cardData.Id switch
        {
            "player_attack_basic" => Select(cardData.Description, "对怪物造成 1 点伤害"),
            "player_draw_quick" => Select(cardData.Description, "抽 1 张牌"),
            "player_seed_parsnip" => Select(cardData.Description, "生长 1 回合\n收割：在此地留下 1 点格挡"),
            "player_seed_strawberry" => Select(cardData.Description, "生长 1 回合\n收割：本回合攻击 +1"),
            "player_seed_starfruit" => Select(cardData.Description, "生长 1 回合\n收割：感染等级 -1"),
            "player_seed_blueberry" => Select(cardData.Description, "生长 2 回合\n收割：治疗一个十字区域"),
            "player_defense_line3_horizontal" => Select(cardData.Description, "本回合\n横向 3 格各抵挡 1 次感染"),
            "player_defense_line3_vertical" => Select(cardData.Description, "本回合\n纵向 3 格各抵挡 1 次感染"),
            "player_defense_cross" => Select(cardData.Description, "本回合\n十字 5 格各抵挡 1 次感染"),
            "enemy_infection_single" => Select(cardData.Description, "以一个未感染地块为中心感染单格"),
            "enemy_infection_line3_horizontal" => Select(cardData.Description, "以一个未感染地块为中心感染横向 3 格"),
            "enemy_infection_line3_vertical" => Select(cardData.Description, "以一个未感染地块为中心感染纵向 3 格"),
            "enemy_infection_cross" => Select(cardData.Description, "以一个未感染地块为中心感染十字 5 格"),
            "fallback_enemy_infection_single" => Select("Infect a single tile", "感染单个地块"),
            _ => cardData.Description
        };
    }
}
