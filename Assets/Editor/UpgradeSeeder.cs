using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Data;

namespace Game.EditorTools
{
    public static class UpgradeSeeder
    {
        private const string OutDir = "Assets/Data/Upgrades";

        private struct Seed
        {
            public string id, displayName, targetProducerId;
            public UpgradeType type;
            public double baseCost, costMultiplier, multiplier, unlockManaTotal;
            public int maxLevel, unlockCountThreshold;
        }

        private static readonly string[] ProducerIds =
        {
            "apprentice", "workshop", "spirit", "rune_circle", "arcane_mech", "dragon_forge"
        };

        private static readonly string[] ProducerNames =
        {
            "魔法学徒", "炼金工坊", "元素精灵", "符文阵", "魔导机械", "龙息熔炉"
        };

        // 各生产者加速符文 3 级的基础成本（约等于该生产者 10 个时的累计开销，便于平衡）
        private static readonly double[] BoostBaseCostByProducer =
        {
            500,        // apprentice
            5000,       // workshop
            70000,      // spirit
            1000000,    // rune_circle
            16000000,   // arcane_mech
            260000000,  // dragon_forge
        };

        private static IEnumerable<Seed> BuildSeeds()
        {
            // 魔杖系列：5 级，每级 ×2 点击产出
            yield return new Seed
            {
                id = "wand", displayName = "魔杖强化", type = UpgradeType.WandClick,
                baseCost = 50, costMultiplier = 8, multiplier = 2, maxLevel = 5,
            };

            // 6 个生产者各 3 级加速符文：解锁要求 = 拥有 10/25/50
            for (int i = 0; i < ProducerIds.Length; i++)
            {
                yield return new Seed
                {
                    id = $"boost_{ProducerIds[i]}",
                    displayName = $"加速符文 · {ProducerNames[i]}",
                    type = UpgradeType.ProducerBoost,
                    targetProducerId = ProducerIds[i],
                    baseCost = BoostBaseCostByProducer[i],
                    costMultiplier = 10,
                    multiplier = 2,
                    maxLevel = 3,
                    unlockCountThreshold = 10, // 第 1 级要求拥有 10；运行期判断 2/3 级时再加阶梯
                };
            }

            // 自动点击符
            yield return new Seed
            {
                id = "auto_clicker", displayName = "自动点击符", type = UpgradeType.AutoClicker,
                baseCost = 50000, costMultiplier = 1, multiplier = 1, maxLevel = 1,
                unlockCountThreshold = 500,    // 累计点击 ≥ 500
                unlockManaTotal = 50000,       // 或累计 MD ≥ 50000
            };
        }

        [MenuItem("Alchemist/Seed Upgrades (overwrite)")]
        public static void RunSeed()
        {
            if (!Directory.Exists(OutDir)) Directory.CreateDirectory(OutDir);
            int count = 0;
            foreach (var s in BuildSeeds())
            {
                string path = $"{OutDir}/Upgrade_{s.id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<UpgradeDef>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<UpgradeDef>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                var so = new SerializedObject(asset);
                so.FindProperty("id").stringValue = s.id;
                so.FindProperty("displayName").stringValue = s.displayName;
                so.FindProperty("type").enumValueIndex = (int)s.type;
                so.FindProperty("targetProducerId").stringValue = s.targetProducerId ?? "";
                so.FindProperty("baseCost").doubleValue = s.baseCost;
                so.FindProperty("costMultiplier").doubleValue = s.costMultiplier;
                so.FindProperty("multiplier").doubleValue = s.multiplier;
                so.FindProperty("maxLevel").intValue = s.maxLevel;
                so.FindProperty("unlockCountThreshold").intValue = s.unlockCountThreshold;
                so.FindProperty("unlockManaTotal").doubleValue = s.unlockManaTotal;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                count++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UpgradeSeeder] Seeded {count} upgrades under {OutDir}");
        }
    }
}
