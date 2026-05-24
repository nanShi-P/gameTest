using System.Collections.Generic;
using BreakInfinity;

namespace Game.Core
{
    /// 纯数据类，可直接 JSON 序列化（BigDouble 字段由自定义 Converter 处理）。
    public class GameState
    {
        public int version = 1;

        public BigDouble manaDust = BigDouble.Zero;
        public BigDouble totalManaEarned = BigDouble.Zero;

        public int philosopherStones = 0;
        public int totalClicks = 0;
        public bool autoClickerOwned = false;

        // key = ProducerDef.Id
        public Dictionary<string, int> producerCounts = new Dictionary<string, int>()
        {
            { "apprentice", 1 }, // 初始赠送 1 个魔法学徒
        };
        // key = UpgradeDef.Id
        public Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

        public int GetProducerCount(string id)
        {
            return producerCounts.TryGetValue(id, out var c) ? c : 0;
        }

        public int GetUpgradeLevel(string id)
        {
            return upgradeLevels.TryGetValue(id, out var l) ? l : 0;
        }
    }
}
