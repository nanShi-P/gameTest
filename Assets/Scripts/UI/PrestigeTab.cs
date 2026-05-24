using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;
using Game.Core;
using Game.Util;

namespace Game.UI
{
    /// 转生标签页。在 TabController 注册第三个 tab "prestige"。
    public class PrestigeTab : MonoBehaviour
    {
        private Text infoText;
        private Text stonesText;
        private Button prestigeBtn;
        private Image btnImg;
        private GameObject confirmDialog;
        private float lastRefresh;

        private void Start()
        {
            var tc = TabController.Instance;
            if (tc == null) { enabled = false; return; }

            // 在 TabController 上额外注册一个标签按钮
            tc.AddTab("prestige", "转生");

            var panel = new GameObject("PrestigePanel", typeof(RectTransform));
            tc.RegisterPanel("prestige", panel);
            panel.transform.SetParent(tc.PanelRoot, false);
            var pRT = panel.GetComponent<RectTransform>();
            pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
            pRT.offsetMin = pRT.offsetMax = Vector2.zero;
            BuildContent(pRT);

            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= Refresh;
        }

        private void Update()
        {
            if (Time.unscaledTime - lastRefresh >= 0.2f)
            {
                lastRefresh = Time.unscaledTime;
                Refresh();
            }
        }

        private void BuildContent(RectTransform parent)
        {
            stonesText = MakeText(parent, "Stones", "贤者之石: 0", 22, TextAnchor.MiddleCenter,
                new Vector2(0, 0.78f), new Vector2(1, 0.95f));

            infoText = MakeText(parent, "Info",
                "累计魔力尘达到 1e10 即可转生。\n可获得贤者之石 = floor(sqrt(累计MD / 1e10))\n每枚贤者之石提供 +2% 全局产出。",
                14, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.50f), new Vector2(0.95f, 0.75f));
            infoText.color = new Color(0.85f, 0.80f, 0.70f);

            var btnGo = new GameObject("PrestigeButton", typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.55f, 0.25f, 0.45f, 1f);
            var bRT = btnGo.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.25f, 0.20f);
            bRT.anchorMax = new Vector2(0.75f, 0.42f);
            bRT.offsetMin = bRT.offsetMax = Vector2.zero;
            prestigeBtn = btnGo.GetComponent<Button>();
            prestigeBtn.onClick.AddListener(ShowConfirm);

            var btnLabel = MakeText((RectTransform)btnGo.transform, "Label", "炼成贤者之石", 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one);
            btnLabel.color = Color.white;

            BuildConfirmDialog(parent);
        }

        private void BuildConfirmDialog(RectTransform parent)
        {
            confirmDialog = new GameObject("ConfirmDialog", typeof(Image));
            confirmDialog.transform.SetParent(parent, false);
            confirmDialog.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
            var rt = confirmDialog.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var box = new GameObject("Box", typeof(Image));
            box.transform.SetParent(confirmDialog.transform, false);
            box.GetComponent<Image>().color = new Color(0.16f, 0.12f, 0.20f, 1f);
            var boxRT = box.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.15f, 0.30f);
            boxRT.anchorMax = new Vector2(0.85f, 0.70f);
            boxRT.offsetMin = boxRT.offsetMax = Vector2.zero;

            MakeText((RectTransform)box.transform, "Msg",
                "确认转生？\n所有魔力尘、生产者、升级将清空，\n仅保留贤者之石。",
                15, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.95f));

            var yes = MakeButton(box.transform, "Yes", "确认", new Color(0.50f, 0.25f, 0.45f),
                new Vector2(0.10f, 0.10f), new Vector2(0.45f, 0.40f));
            yes.onClick.AddListener(() =>
            {
                bool ok = GameManager.Instance.TryPrestige();
                confirmDialog.SetActive(false);
                if (ok) PrestigeFlash.Trigger();
            });

            var no = MakeButton(box.transform, "No", "取消", new Color(0.25f, 0.22f, 0.30f),
                new Vector2(0.55f, 0.10f), new Vector2(0.90f, 0.40f));
            no.onClick.AddListener(() => confirmDialog.SetActive(false));

            confirmDialog.SetActive(false);
        }

        private void ShowConfirm()
        {
            if (!GameManager.Instance.CanPrestige()) return;
            confirmDialog.SetActive(true);
        }

        private void Refresh()
        {
            var gm = GameManager.Instance;
            int pending = gm.GetPendingStones();
            int next = pending + 1;
            // 下一颗石所需累计 MD = (next)^2 * threshold
            BigDouble needFor = new BigDouble((double)next * next) * gm.PrestigeThreshold;
            stonesText.text = $"贤者之石: {gm.State.philosopherStones}    (本轮可获得: +{pending})";
            infoText.text =
                $"累计魔力尘 (本轮): {NumberFormat.Format(gm.State.totalManaEarned)}\n" +
                $"下一颗贤者之石需要累计: {NumberFormat.Format(needFor)}\n" +
                $"每枚贤者之石提供 +2% 全局产出（当前 +{gm.State.philosopherStones * 2}%）";
            bool can = pending > 0;
            prestigeBtn.interactable = can;
            btnImg.color = can ? new Color(0.55f, 0.25f, 0.45f, 1f)
                              : new Color(0.25f, 0.22f, 0.30f, 1f);
        }

        // ---- 工具 ----

        private static Text MakeText(RectTransform parent, string name, string content, int size, TextAnchor align,
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
            rt.offsetMin = new Vector2(4, 4); rt.offsetMax = new Vector2(-4, -4);
            return t;
        }

        private static Button MakeButton(Transform parent, string name, string label, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            MakeText(rt, "Label", label, 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one).color = Color.white;
            return go.GetComponent<Button>();
        }
    }
}
