using System.Collections.Generic;

namespace BlackValley.Cards;
/// <summary>
/// 卡牌的基础数据模型
/// 用于从 Json 文件中反序列化读取策划填写的配置表
/// </summary>
public class CardData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty; // 枚举：Player、Enemy
    public string Type { get; set; } = string.Empty; // 枚举：Attack、Defense、Seed、Draw、Infection
    public int Cost { get; set; }                    // 打出该牌消耗的能量
    public int CopyCount { get; set; } = 1;         // 该卡在对应牌组中的张数
    public string TargetType { get; set; } = string.Empty; // 枚举：Enemy、Grid_Empty、Grid_Any、Grid_UninfectedCenter、None
    public string ShapeId { get; set; } = string.Empty; // 形状标识，便于运行时判断和降档
    public string IconId { get; set; } = string.Empty; // 卡面图标标识
    public int Damage { get; set; }                  // 攻击牌造成的伤害值
    public int DrawCount { get; set; }               // 功能牌抽牌数量
    public int ProtectionChargesPerTile { get; set; } // 防御牌覆盖格子的单格抵挡次数
    public int InfectionTier { get; set; }          // 感染档次，0 表示不参与感染降档逻辑
    public int DefenseValue { get; set; }            // 防御牌提供的防护值或抵消感染次数
    public string PlantId { get; set; } = string.Empty;    // 种子牌对应生成的植物 ID
    /// <summary>
    /// 卡牌作用的相对网格形状
    /// 格式为相对坐标系的二维数组，例如：[[0,0], [0,1], [0,-1]] 表示中心及上下格子
    /// </summary>
    public List<List<int>> Shape { get; set; } = new();
    public List<string> DowngradeCardIds { get; set; } = new(); // 当前感染牌被削弱后可降为哪些卡
    public string Description { get; set; } = string.Empty; // 卡牌描述文本
}
