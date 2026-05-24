using UnityEngine;

namespace Game.Data
{
    public enum UpgradeType
    {
        WandClick,      // 提升手动点击产出
        ProducerBoost,  // 指定生产者产出乘区
        AutoClicker,    // 解锁自动点击
    }

    [CreateAssetMenu(menuName = "Alchemist/Upgrade", fileName = "Upgrade_New", order = 1)]
    public class UpgradeDef : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private UpgradeType type;

        [Header("Target (ProducerBoost only)")]
        [SerializeField] private string targetProducerId;

        [Header("Economy")]
        [SerializeField] private double baseCost = 100;
        [Tooltip("多级升级时每级成本乘数")]
        [SerializeField] private double costMultiplier = 5;
        [Tooltip("每级提供的乘区倍数（如 2 表示 ×2）")]
        [SerializeField] private double multiplier = 2;
        [Tooltip("最大可购买级数")]
        [SerializeField] private int maxLevel = 1;

        [Header("Unlock")]
        [Tooltip("ProducerBoost: 拥有该生产者数量达此值才解锁；AutoClicker: 点击次数达此值才解锁")]
        [SerializeField] private int unlockCountThreshold = 0;
        [Tooltip("AutoClicker: 累计 MD 达此值也可解锁（与点击次数 OR 关系）")]
        [SerializeField] private double unlockManaTotal = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public UpgradeType Type => type;
        public string TargetProducerId => targetProducerId;
        public double BaseCost => baseCost;
        public double CostMultiplier => costMultiplier;
        public double Multiplier => multiplier;
        public int MaxLevel => maxLevel;
        public int UnlockCountThreshold => unlockCountThreshold;
        public double UnlockManaTotal => unlockManaTotal;
    }
}
