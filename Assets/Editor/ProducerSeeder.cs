using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Data;

namespace Game.EditorTools
{
    public static class ProducerSeeder
    {
        private const string OutDir = "Assets/Data/Producers";

        private struct Seed
        {
            public string id, displayName, unlockProducerId;
            public double baseCost, baseProduction, unlockManaTotal;
            public int unlockProducerCount;
        }

        private static readonly Seed[] Seeds =
        {
            new Seed { id = "apprentice",   displayName = "魔法学徒",   baseCost = 15,        baseProduction = 0.2,    unlockProducerId = "",            unlockProducerCount = 0, unlockManaTotal = 10 },
            new Seed { id = "workshop",     displayName = "炼金工坊",   baseCost = 150,       baseProduction = 2,      unlockProducerId = "apprentice",  unlockProducerCount = 1, unlockManaTotal = 0 },
            new Seed { id = "spirit",       displayName = "元素精灵",   baseCost = 2000,      baseProduction = 25,     unlockProducerId = "workshop",    unlockProducerCount = 1, unlockManaTotal = 0 },
            new Seed { id = "rune_circle",  displayName = "符文阵",     baseCost = 30000,     baseProduction = 300,    unlockProducerId = "spirit",      unlockProducerCount = 1, unlockManaTotal = 0 },
            new Seed { id = "arcane_mech",  displayName = "魔导机械",   baseCost = 500000,    baseProduction = 4000,   unlockProducerId = "rune_circle", unlockProducerCount = 1, unlockManaTotal = 0 },
            new Seed { id = "dragon_forge", displayName = "龙息熔炉",   baseCost = 8000000,   baseProduction = 60000,  unlockProducerId = "arcane_mech", unlockProducerCount = 1, unlockManaTotal = 0 },
        };

        [MenuItem("Alchemist/Seed Producers (overwrite)")]
        public static void RunSeed()
        {
            if (!Directory.Exists(OutDir)) Directory.CreateDirectory(OutDir);

            foreach (var s in Seeds)
            {
                string path = $"{OutDir}/Producer_{s.id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<ProducerDef>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<ProducerDef>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                var so = new SerializedObject(asset);
                so.FindProperty("id").stringValue = s.id;
                so.FindProperty("displayName").stringValue = s.displayName;
                so.FindProperty("baseCost").doubleValue = s.baseCost;
                so.FindProperty("baseProduction").doubleValue = s.baseProduction;
                so.FindProperty("costMultiplier").doubleValue = 1.12;
                so.FindProperty("unlockProducerId").stringValue = s.unlockProducerId;
                so.FindProperty("unlockProducerCount").intValue = s.unlockProducerCount;
                so.FindProperty("unlockManaTotal").doubleValue = s.unlockManaTotal;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ProducerSeeder] Seeded {Seeds.Length} producers under {OutDir}");
        }
    }
}
