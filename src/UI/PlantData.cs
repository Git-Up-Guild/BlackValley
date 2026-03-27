namespace BlackValley;
/// <summary>
/// 植物的实体数据模型。用于从 JSON 文件中反序列化。
/// 独立于卡牌存在，由网格管理器根据 PlantId 读取并维护其生命周期。
/// </summary>
public class PlantData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 生长所需回合数（如：防风草为1，甜瓜为2）
    /// </summary>
    public int GrowthTurnsRequired { get; set; }

    /// <summary>
    /// 效果触发时机（枚举：OnMonsterAttack 被攻击时, OnGrowthComplete 成长完成时, Passive 持续被动）
    /// </summary>
    public string EffectTrigger { get; set; } = string.Empty;

    /// <summary>
    /// 触发的具体效果类型（由逻辑程序解析，如：BlockInfection 抵挡感染, AddDamage 增加伤害）
    /// </summary>
    public string EffectType { get; set; } = string.Empty;
}