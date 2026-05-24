using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Alchemist/Producer", fileName = "Producer_New", order = 0)]
    public class ProducerDef : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Economy")]
        [Tooltip("初始成本 (MD)")]
        [SerializeField] private double baseCost = 15;
        [Tooltip("基础产出 MD/s")]
        [SerializeField] private double baseProduction = 0.2;
        [Tooltip("每多 1 个的成本乘数")]
        [SerializeField] private double costMultiplier = 1.12;

        [Header("Unlock")]
        [Tooltip("解锁所需的前置生产者 id，留空表示无前置（仅靠 manaDust 累计解锁）")]
        [SerializeField] private string unlockProducerId;
        [Tooltip("前置生产者所需数量")]
        [SerializeField] private int unlockProducerCount = 0;
        [Tooltip("无前置时，需要累计获得的 MD 数量")]
        [SerializeField] private double unlockManaTotal = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public double BaseCost => baseCost;
        public double BaseProduction => baseProduction;
        public double CostMultiplier => costMultiplier;
        public string UnlockProducerId => unlockProducerId;
        public int UnlockProducerCount => unlockProducerCount;
        public double UnlockManaTotal => unlockManaTotal;
    }
}
