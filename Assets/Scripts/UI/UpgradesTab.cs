using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;
using Game.Core;
using Game.Data;
using Game.Util;

namespace Game.UI
{
    public class UpgradesTab : MonoBehaviour
    {
        private readonly List<UpgradeRow> rows = new List<UpgradeRow>();
        private float lastRefresh;

        private void Start()
        {
            var tc = TabController.Instance;
            if (tc == null) { enabled = false; return; }

            var panel = new GameObject("UpgradesPanel", typeof(RectTransform));
            tc.RegisterPanel("upgrades", panel);   // 此调用内部会触发 EnsureBuilt 并设置 PanelRoot
            panel.transform.SetParent(tc.PanelRoot, false);
            var pRT = panel.GetComponent<RectTransform>();
            pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
            pRT.offsetMin = pRT.offsetMax = Vector2.zero;

            BuildList(pRT);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged += RefreshAll;
                RefreshAll();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= RefreshAll;
        }

        private void Update()
        {
            if (Time.unscaledTime - lastRefresh >= 0.2f)
            {
                lastRefresh = Time.unscaledTime;
                RefreshAll();
            }
        }

        private void BuildList(RectTransform parent)
        {
            var scrollGo = new GameObject("UpgradeScroll", typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            scrollGo.GetComponent<Image>().color = new Color(0.10f, 0.08f, 0.13f, 1f);
            var sRT = scrollGo.GetComponent<RectTransform>();
            sRT.anchorMin = Vector2.zero; sRT.anchorMax = Vector2.one;
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            var vRT = viewport.GetComponent<RectTransform>();
            vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
            vRT.offsetMin = vRT.offsetMax = Vector2.zero;
            scroll.viewport = vRT;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var cRT = content.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.offsetMin = cRT.offsetMax = Vector2.zero;
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6; vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childForceExpandWidth = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childControlHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = cRT;

            foreach (var u in GameManager.Instance.Upgrades)
            {
                rows.Add(new UpgradeRow(cRT, u));
            }
        }

        private void RefreshAll()
        {
            foreach (var r in rows) r.Refresh();
        }
    }

    public class UpgradeRow
    {
        private readonly UpgradeDef def;
        private readonly GameObject root;
        private readonly Text nameText;
        private readonly Text levelText;
        private readonly Text costText;
        private readonly Button buyBtn;
        private readonly Image buyImg;

        public UpgradeRow(RectTransform parent, UpgradeDef def)
        {
            this.def = def;
            root = new GameObject($"URow_{def.Id}", typeof(Image), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.16f, 0.13f, 0.20f, 1f);
            var le = root.GetComponent<LayoutElement>();
            le.preferredHeight = 72; le.minHeight = 72;

            nameText = MakeText(root.transform, "Name", def.DisplayName, 14, TextAnchor.UpperLeft,
                new Vector2(0.02f, 0.55f), new Vector2(0.60f, 0.95f));

            // 效果描述
            var effectText = MakeText(root.transform, "Effect", BuildEffectDesc(def), 11, TextAnchor.UpperLeft,
                new Vector2(0.02f, 0.30f), new Vector2(0.60f, 0.55f));
            effectText.color = new Color(0.65f, 0.85f, 0.70f);

            levelText = MakeText(root.transform, "Level", "Lv 0/0", 12, TextAnchor.LowerLeft,
                new Vector2(0.02f, 0.05f), new Vector2(0.60f, 0.30f));
            levelText.color = new Color(0.75f, 0.70f, 0.85f);

            var btnGo = new GameObject("BuyButton", typeof(Image), typeof(Button));
            btnGo.transform.SetParent(root.transform, false);
            buyImg = btnGo.GetComponent<Image>();
            buyImg.color = new Color(0.40f, 0.30f, 0.60f, 1f);
            var btnRT = btnGo.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.62f, 0.10f);
            btnRT.anchorMax = new Vector2(0.98f, 0.90f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            buyBtn = btnGo.GetComponent<Button>();
            buyBtn.onClick.AddListener(() =>
            {
                bool ok = GameManager.Instance.TryBuyUpgrade(def.Id);
                if (ok)
                {
                    Game.Util.UITween.ColorFlash(root.GetComponent<UnityEngine.UI.Image>(),
                        new Color(0.45f, 0.30f, 0.20f, 1f), 0.22f);
                    Game.Util.UITween.ScalePunch(costText.transform, 1.15f, 0.18f);
                }
            });

            costText = MakeText(btnRT, "Cost", "购买\n0", 13, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one);
            costText.color = Color.white;
        }

        public void Refresh()
        {
            var gm = GameManager.Instance;
            bool everUnlocked = IsEverUnlocked(gm);
            root.SetActive(everUnlocked);
            if (!everUnlocked) return;

            int lv = gm.State.GetUpgradeLevel(def.Id);
            bool maxed = lv >= def.MaxLevel;
            levelText.text = $"Lv {lv}/{def.MaxLevel}";

            if (maxed)
            {
                costText.text = "已满";
                buyBtn.interactable = false;
                buyImg.color = new Color(0.22f, 0.20f, 0.16f, 1f);
                return;
            }

            // 下一级是否解锁（阶梯）
            string lockHint = NextLevelLockHint(gm, lv);
            if (lockHint != null)
            {
                costText.text = lockHint;
                buyBtn.interactable = false;
                buyImg.color = new Color(0.25f, 0.22f, 0.30f, 1f);
                return;
            }

            BigDouble cost = gm.GetUpgradeCost(def);
            costText.text = "购买\n" + NumberFormat.Format(cost);
            bool can = gm.State.manaDust.CompareTo(cost) >= 0;
            buyBtn.interactable = can;
            buyImg.color = can ? new Color(0.40f, 0.30f, 0.60f, 1f)
                              : new Color(0.25f, 0.22f, 0.30f, 1f);
        }

        /// 是否曾经达到过解锁阈值（决定整行可见性）。一旦可见就一直可见。
        private bool IsEverUnlocked(GameManager gm)
        {
            switch (def.Type)
            {
                case UpgradeType.WandClick:
                    return true;
                case UpgradeType.ProducerBoost:
                    return gm.State.GetProducerCount(def.TargetProducerId) >= 10;
                case UpgradeType.AutoClicker:
                    return gm.State.totalClicks >= def.UnlockCountThreshold
                        || gm.State.totalManaEarned.CompareTo(new BigDouble(def.UnlockManaTotal)) >= 0;
            }
            return true;
        }

        /// 下一级（lv+1 → 1/2/3）所需的阶梯条件。返回 null 表示可购买；返回字符串表示锁定提示。
        private string NextLevelLockHint(GameManager gm, int lv)
        {
            if (def.Type != UpgradeType.ProducerBoost) return null;
            int[] tiers = { 10, 25, 50 };
            int need = tiers[Mathf.Min(lv, tiers.Length - 1)];
            int owned = gm.State.GetProducerCount(def.TargetProducerId);
            if (owned >= need) return null;
            var targetDef = gm.FindProducer(def.TargetProducerId);
            string targetName = targetDef != null ? targetDef.DisplayName : def.TargetProducerId;
            return $"需要\n{targetName} x{need}";
        }

        private static string BuildEffectDesc(UpgradeDef def)
        {
            switch (def.Type)
            {
                case UpgradeType.WandClick:
                    return $"每级使点击产出 ×{def.Multiplier:0.##}";
                case UpgradeType.ProducerBoost:
                {
                    var gm = GameManager.Instance;
                    var target = gm != null ? gm.FindProducer(def.TargetProducerId) : null;
                    string targetName = target != null ? target.DisplayName : def.TargetProducerId;
                    return $"每级使「{targetName}」产出 ×{def.Multiplier:0.##}";
                }
                case UpgradeType.AutoClicker:
                    return "解锁后自动以 2 次/秒触发点击";
            }
            return "";
        }

        private static Text MakeText(Transform parent, string name, string content, int size, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.font = UIFont.Get();
            t.fontSize = size;
            t.alignment = align;
            t.color = new Color(0.95f, 0.93f, 0.85f);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(6, 2); rt.offsetMax = new Vector2(-6, -2);
            return t;
        }
    }
}
