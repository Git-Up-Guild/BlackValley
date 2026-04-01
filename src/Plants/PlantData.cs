using System.Collections.Generic;

namespace BlackValley.Plants;
/// <summary>
/// 植物的基础数据模型
/// 用于从 Json 文件中反序列化，并由网格逻辑在运行时引用
/// </summary>
public class PlantData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IconId { get; set; } = string.Empty;
    public string VanillaSeedId { get; set; } = string.Empty;

    /// <summary>
    /// 生长所需回合数，例如防风草为 1，甜瓜为 2
    /// </summary>
    public int GrowthTurnsRequired { get; set; }

    public int BaseProtectionCharges { get; set; } = 1;

    /// <summary>
    /// 收割时触发的效果类型
    /// 例如 None、AddAttackDamage、ReduceInfectionTier、HealInfectedTiles
    /// </summary>
    public string HarvestEffectType { get; set; } = string.Empty;

    /// <summary>
    /// 收割效果的数值
    /// 例如加伤数值、感染降档层数
    /// </summary>
    public int HarvestEffectValue { get; set; }

    public string HarvestShapeId { get; set; } = string.Empty;

    /// <summary>
    /// 收割效果的相对形状
    /// 例如蓝莓收割后治疗十字范围感染格
    /// </summary>
    public List<List<int>> HarvestShape { get; set; } = new();

    public string Description { get; set; } = string.Empty;
}
