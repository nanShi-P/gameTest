using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;
using Game.Core;
using Game.Data;
using Game.Util;

namespace Game.UI
{
    /// 单个生产者一行 UI。非 MonoBehaviour，由 ProducersTab 管理。
    public class ProducerRow
    {
        private readonly ProducerDef def;
        private readonly GameObject root;
        private readonly Text nameText;
        private readonly Text countText;
        private readonly Text ppsText;
        private readonly Text costText;
        private readonly Button buyButton;
        private readonly Image buyImage;

        private bool unlocked;

        public ProducerRow(RectTransform parent, ProducerDef def)
        {
            this.def = def;

            root = new GameObject($"Row_{def.Id}", typeof(Image), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            var bg = root.GetComponent<Image>();
            bg.color = new Color(0.16f, 0.13f, 0.20f, 1f);
            var le = root.GetComponent<LayoutElement>();
            le.preferredHeight = 78;
            le.minHeight = 78;

            // 名称（左上）
            nameText = MakeText(root.transform, "Name", def.DisplayName, 16, TextAnchor.UpperLeft,
                new Vector2(0.02f, 0.55f), new Vector2(0.55f, 0.95f));

            // 单个产出描述（名称右侧小字）
            var descText = MakeText(root.transform, "Desc",
                $"每个 +{NumberFormat.Format(new BigDouble(def.BaseProduction))}/s",
                11, TextAnchor.UpperLeft,
                new Vector2(0.02f, 0.32f), new Vector2(0.60f, 0.55f));
            descText.color = new Color(0.65f, 0.85f, 0.70f);

            // 数量 & 产出（左下）
            countText = MakeText(root.transform, "Count", "x0", 13, TextAnchor.LowerLeft,
                new Vector2(0.02f, 0.05f), new Vector2(0.20f, 0.30f));
            countText.color = new Color(0.75f, 0.70f, 0.85f);

            ppsText = MakeText(root.transform, "Pps", "0 /s", 13, TextAnchor.LowerLeft,
                new Vector2(0.20f, 0.05f), new Vector2(0.60f, 0.30f));
            ppsText.color = new Color(0.95f, 0.85f, 0.55f);

            // 购买按钮（右）
            var btnGo = new GameObject("BuyButton", typeof(Image), typeof(Button));
            btnGo.transform.SetParent(root.transform, false);
            buyImage = btnGo.GetComponent<Image>();
            buyImage.color = new Color(0.40f, 0.30f, 0.60f, 1f);
            var btnRT = btnGo.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.60f, 0.10f);
            btnRT.anchorMax = new Vector2(0.98f, 0.90f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            buyButton = btnGo.GetComponent<Button>();
            buyButton.onClick.AddListener(OnBuy);

            costText = MakeText(btnRT, "Cost", "购买\n0", 14, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one);
            costText.color = Color.white;
        }

        private void OnBuy()
        {
            bool ok = GameManager.Instance.TryBuyProducer(def.Id);
            if (ok)
            {
                Game.Util.UITween.ColorFlash(root.GetComponent<UnityEngine.UI.Image>(),
                    new Color(0.45f, 0.30f, 0.20f, 1f), 0.22f);
                Game.Util.UITween.ScalePunch(costText.transform, 1.15f, 0.18f);
            }
        }

        public void Refresh()
        {
            var gm = GameManager.Instance;
            unlocked = IsUnlocked(gm);
            root.SetActive(unlocked);
            if (!unlocked) return;

            int count = gm.State.GetProducerCount(def.Id);
            BigDouble cost = gm.GetProducerCost(def);
            BigDouble pps = gm.GetProducerProductionPerSecond(def);

            countText.text = $"x{count}";
            ppsText.text = NumberFormat.Format(pps) + " /s";
            costText.text = "购买\n" + NumberFormat.Format(cost);

            bool canAfford = gm.State.manaDust.CompareTo(cost) >= 0;
            buyButton.interactable = canAfford;
            buyImage.color = canAfford
                ? new Color(0.40f, 0.30f, 0.60f, 1f)
                : new Color(0.25f, 0.22f, 0.30f, 1f);
        }

        private bool IsUnlocked(GameManager gm)
        {
            if (!string.IsNullOrEmpty(def.UnlockProducerId))
            {
                return gm.State.GetProducerCount(def.UnlockProducerId) >= def.UnlockProducerCount;
            }
            if (def.UnlockManaTotal > 0)
            {
                return gm.State.totalManaEarned.CompareTo(new BigDouble(def.UnlockManaTotal)) >= 0;
            }
            return true;
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
            t.color = new Color(0.95f, 0.93f, 0.85f, 1f);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = new Vector2(4, 2);
            rt.offsetMax = new Vector2(-4, -2);
            return t;
        }
    }
}
