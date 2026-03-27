using System.Collections.Generic;

namespace BlackValley;
/// <summary>
/// 卡牌的基础数据模型。用于从 JSON 文件中反序列化读取策划填写的配置表。
/// </summary>
public class CardData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 枚举：Attack(攻击), Skill(技能/防御), Seed(种子)
    public int Cost { get; set; }                    // 打出该牌消耗的能量
    public string TargetType { get; set; } = string.Empty; // 枚举：Enemy(单体敌人), Grid_Empty(空网格), Grid_Any(任意网格)
    public int Damage { get; set; }                  // 攻击牌造成的伤害值
    public int DefenseValue { get; set; }            // 防御牌提供的防护值/抵消感染次数
    public string PlantId { get; set; } = string.Empty;    // 如果是种子牌，对应生成的植物ID (对应 PlantData)

    /// <summary>
    /// 卡牌作用的相对网格形状。
    /// 格式为相对坐标系的二维数组，例如：[[0,0], [0,1], [0,-1]] 表示中心及上下格子。
    /// </summary>
    public List<List<int>> Shape { get; set; } = new();
    public string Description { get; set; } = string.Empty; // 卡牌文本描述
}