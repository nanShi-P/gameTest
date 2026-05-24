using System;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;
using Game.Data;
using Game.Save;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Data")]
        [SerializeField] private List<ProducerDef> producers = new List<ProducerDef>();
        [SerializeField] private List<UpgradeDef> upgrades = new List<UpgradeDef>();

        [Header("Debug")]
        [SerializeField] private bool logInitialState = true;
        [Tooltip("Play 时强制给某生产者一些初始数量，用于验证 Tick。留空 id 表示不生效。")]
        [SerializeField] private string debugSeedProducerId = "";
        [SerializeField] private int debugSeedCount = 0;
        [Tooltip("调试用：Play 时直接给初始魔力尘。0 = 不生效。会绕过 Load。")]
        [SerializeField] private double debugStartMana = 0;
        [Tooltip("调试用：全局产出速度倍数。1 = 正常，100 = 100 倍速。")]
        [SerializeField] private float debugSpeedMultiplier = 1f;

        [Header("Tick")]
        [Tooltip("Tick 间隔（秒）。默认 0.1 = 10Hz")]
        [SerializeField] private float tickInterval = 0.1f;

        [Header("Save")]
        [Tooltip("自动保存间隔（秒）。0 表示禁用自动保存。")]
        [SerializeField] private float autoSaveInterval = 10f;
        [Tooltip("启动时若存档存在则加载；关闭便于调试。")]
        [SerializeField] private bool loadOnStart = true;

        // 临时加成（由 T012 随机事件系统设置）
        public double TempGlobalMult { get; set; } = 1.0;
        public double TempClickMult { get; set; } = 1.0;

        public GameState State { get; private set; }
        public IReadOnlyList<ProducerDef> Producers => producers;
        public IReadOnlyList<UpgradeDef> Upgrades => upgrades;

        public event Action StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            State = new GameState();

            if (loadOnStart)
            {
                var loaded = SaveSystem.Load();
                if (loaded != null)
                {
                    State = loaded;
                    Debug.Log($"[GameManager] Save loaded from {SaveSystem.GetSavePath()}");
                }
            }

            if (!string.IsNullOrEmpty(debugSeedProducerId) && debugSeedCount > 0
                && State.GetProducerCount(debugSeedProducerId) == 0)
            {
                State.producerCounts[debugSeedProducerId] = debugSeedCount;
            }

            if (debugStartMana > 0 && State.manaDust.CompareTo(BigDouble.Zero) == 0)
            {
                State.manaDust = new BigDouble(debugStartMana);
                State.totalManaEarned = new BigDouble(debugStartMana);
            }

            if (logInitialState)
            {
                Debug.Log($"[GameManager] Initialized. Producers={producers.Count} Upgrades={upgrades.Count} manaDust={State.manaDust}");
            }
        }

        private void Start()
        {
            InvokeRepeating(nameof(Tick), tickInterval, tickInterval);
            if (autoSaveInterval > 0f)
            {
                InvokeRepeating(nameof(AutoSave), autoSaveInterval, autoSaveInterval);
            }
            StartCoroutine(AutoClickerLoop());
        }

        private System.Collections.IEnumerator AutoClickerLoop()
        {
            var wait = new WaitForSeconds(0.5f); // 2 次/秒
            while (true)
            {
                yield return wait;
                if (State != null && State.autoClickerOwned)
                {
                    Click();
                }
            }
        }

        private void AutoSave()
        {
            SaveSystem.Save(State);
        }

        private void OnApplicationQuit()
        {
            SaveSystem.Save(State);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveSystem.Save(State);
        }

        private void Tick()
        {
            BigDouble pps = GetProductionPerSecond();
            if (pps.CompareTo(BigDouble.Zero) > 0)
            {
                BigDouble delta = pps * tickInterval * debugSpeedMultiplier;
                State.manaDust += delta;
                State.totalManaEarned += delta;
                StateChanged?.Invoke();
            }
        }

        /// 全局产出乘区 = (1 + 0.02 * PS) * 临时事件加成
        public double GetGlobalMultiplier()
        {
            return (1.0 + 0.02 * State.philosopherStones) * TempGlobalMult;
        }

        /// 单个生产者的当前每秒产出（已含全局与该生产者加速符文倍率）
        public BigDouble GetProducerProductionPerSecond(ProducerDef def)
        {
            int count = State.GetProducerCount(def.Id);
            if (count <= 0) return BigDouble.Zero;
            double localMult = GetProducerBoostMultiplier(def.Id);
            double globalMult = GetGlobalMultiplier();
            return new BigDouble(def.BaseProduction) * count * localMult * globalMult;
        }

        /// 全部生产者合计每秒产出
        public BigDouble GetProductionPerSecond()
        {
            BigDouble sum = BigDouble.Zero;
            foreach (var p in producers)
            {
                sum += GetProducerProductionPerSecond(p);
            }
            return sum;
        }

        /// 指定生产者的加速符文乘区（多级叠乘）
        public double GetProducerBoostMultiplier(string producerId)
        {
            string upgradeId = $"boost_{producerId}";
            var def = FindUpgrade(upgradeId);
            if (def == null) return 1.0;
            int lv = State.GetUpgradeLevel(upgradeId);
            return Math.Pow(def.Multiplier, lv);
        }

        public ProducerDef FindProducer(string id) => producers.Find(p => p.Id == id);
        public UpgradeDef FindUpgrade(string id) => upgrades.Find(u => u.Id == id);

        // ------ 公共操作 ------

        /// 手动点击：+1 MD（后续 T011 会乘魔杖加成）。
        public void Click()
        {
            BigDouble gain = ClickProduction();
            State.manaDust += gain;
            State.totalManaEarned += gain;
            State.totalClicks++;
            Game.Audio.AudioManager.Instance?.Play("click");
            StateChanged?.Invoke();
        }

        public BigDouble ClickProduction()
        {
            int wandLv = State.GetUpgradeLevel("wand");
            double mult = Math.Pow(2, wandLv) * TempClickMult;
            return new BigDouble(mult);
        }

        public BigDouble GetProducerCost(ProducerDef def)
        {
            int owned = State.GetProducerCount(def.Id);
            return new BigDouble(def.BaseCost) * BigDouble.Pow(def.CostMultiplier, owned);
        }

        public bool TryBuyProducer(string id)
        {
            var def = FindProducer(id);
            if (def == null) return false;
            BigDouble cost = GetProducerCost(def);
            if (State.manaDust.CompareTo(cost) < 0) return false;
            State.manaDust -= cost;
            int prev = State.GetProducerCount(id);
            State.producerCounts[id] = prev + 1;
            Game.Audio.AudioManager.Instance?.Play("buy");
            StateChanged?.Invoke();
            return true;
        }

        public BigDouble GetUpgradeCost(UpgradeDef def)
        {
            int lv = State.GetUpgradeLevel(def.Id);
            return new BigDouble(def.BaseCost) * BigDouble.Pow(def.CostMultiplier, lv);
        }

        public bool TryBuyUpgrade(string id)
        {
            var def = FindUpgrade(id);
            if (def == null) return false;
            int lv = State.GetUpgradeLevel(id);
            if (lv >= def.MaxLevel) return false;
            BigDouble cost = GetUpgradeCost(def);
            if (State.manaDust.CompareTo(cost) < 0) return false;
            State.manaDust -= cost;
            State.upgradeLevels[id] = lv + 1;
            if (def.Type == UpgradeType.AutoClicker) State.autoClickerOwned = true;
            Game.Audio.AudioManager.Instance?.Play("buy");
            StateChanged?.Invoke();
            return true;
        }

        [Header("Prestige")]
        [Tooltip("解锁转生所需的累计 MD 阈值")]
        [SerializeField] private double prestigeThreshold = 1e9;
        [Tooltip("贤者之石 = floor(sqrt(totalManaEarned / threshold))")]
        public double PrestigeThreshold => prestigeThreshold;

        /// 计算当前可获得的贤者之石数量：floor(sqrt(totalManaEarned / threshold))
        public int GetPendingStones()
        {
            BigDouble threshold = new BigDouble(prestigeThreshold);
            if (State.totalManaEarned.CompareTo(threshold) < 0) return 0;
            double log = BreakInfinity.BigDouble.Log10(State.totalManaEarned)
                       - System.Math.Log10(prestigeThreshold);
            double rooted = System.Math.Pow(10, log / 2.0);
            if (double.IsNaN(rooted) || double.IsInfinity(rooted)) return int.MaxValue;
            return (int)System.Math.Floor(rooted);
        }

        public bool CanPrestige() => GetPendingStones() > 0;

        public bool TryPrestige()
        {
            int gain = GetPendingStones();
            if (gain <= 0) return false;

            // 清掉正在进行的随机事件
            var evt = Game.Events.RandomEventSystem.Instance;
            if (evt != null) evt.ClearActiveEvent();

            State.philosopherStones += gain;
            State.manaDust = BigDouble.Zero;
            State.totalManaEarned = BigDouble.Zero;
            State.totalClicks = 0;
            State.autoClickerOwned = false;
            State.producerCounts.Clear();
            State.producerCounts["apprentice"] = 1; // 转生后保留 1 个初始学徒
            State.upgradeLevels.Clear();
            TempGlobalMult = 1.0;
            TempClickMult = 1.0;

            Game.Audio.AudioManager.Instance?.Play("prestige");
            StateChanged?.Invoke();
            SaveSystem.Save(State);
            return true;
        }
    }
}
