using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// 在 MainUIBuilder.ContentArea 顶部建标签栏，下方留 PanelRoot 给各 Tab 挂自己的内容。
    /// 各 Tab 通过 TabController.Instance.PanelRoot 取容器。
    public class TabController : MonoBehaviour
    {
        public static TabController Instance { get; private set; }

        public RectTransform PanelRoot { get; private set; }

        private readonly List<(string id, GameObject panel, Button btn, Image img)> tabs = new();
        private string activeId;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            EnsureBuilt();
        }

        private bool built;
        private void EnsureBuilt()
        {
            if (built) return;
            var builder = FindObjectOfType<MainUIBuilder>();
            if (builder == null || builder.ContentArea == null)
            {
                Debug.LogError("[TabController] MainUIBuilder/ContentArea missing.");
                enabled = false;
                return;
            }
            for (int i = builder.ContentArea.childCount - 1; i >= 0; i--)
                Destroy(builder.ContentArea.GetChild(i).gameObject);
            BuildBars(builder.ContentArea);
            built = true;
        }

        private void BuildBars(RectTransform parent)
        {
            // TabBar
            var barGo = new GameObject("TabBar", typeof(Image), typeof(HorizontalLayoutGroup));
            barGo.transform.SetParent(parent, false);
            var barImg = barGo.GetComponent<Image>();
            barImg.color = new Color(0.10f, 0.08f, 0.13f, 1f);
            var barRT = barGo.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0.92f);
            barRT.anchorMax = new Vector2(1, 1f);
            barRT.offsetMin = barRT.offsetMax = Vector2.zero;
            var hlg = barGo.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.spacing = 4;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // PanelRoot
            var panelGo = new GameObject("PanelRoot", typeof(RectTransform));
            panelGo.transform.SetParent(parent, false);
            PanelRoot = panelGo.GetComponent<RectTransform>();
            PanelRoot.anchorMin = new Vector2(0, 0);
            PanelRoot.anchorMax = new Vector2(1, 0.92f);
            PanelRoot.offsetMin = PanelRoot.offsetMax = Vector2.zero;

            // 预定义的标签（panel 由各 Tab 脚本自己创建后调用 RegisterPanel）
            CreateTabButton(barGo.transform, "producers", "生产者");
            CreateTabButton(barGo.transform, "upgrades", "升级");
        }

        private void CreateTabButton(Transform parent, string id, string label)
        {
            var go = new GameObject($"Tab_{id}", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.22f, 0.18f, 0.28f, 1f);
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => Activate(id));

            var txt = new GameObject("Label", typeof(Text));
            txt.transform.SetParent(go.transform, false);
            var t = txt.GetComponent<Text>();
            t.text = label;
            t.font = UIFont.Get();
            t.fontSize = 14;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.95f, 0.93f, 0.85f);
            var tRT = t.rectTransform;
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = tRT.offsetMax = Vector2.zero;

            tabs.Add((id, null, btn, img));
        }

        /// 动态注册新 tab（如 PrestigeTab 在 Start 时追加）。
        public void AddTab(string id, string label)
        {
            EnsureBuilt();
            foreach (var t in tabs) if (t.id == id) return; // 已存在
            var bar = transform.Find("MainCanvas/RightPanel/ContentArea/TabBar");
            if (bar == null)
            {
                // 通过 builder 兜底
                var builder = FindObjectOfType<MainUIBuilder>();
                if (builder == null) return;
                bar = builder.ContentArea.Find("TabBar");
                if (bar == null) return;
            }
            CreateTabButton(bar, id, label);
        }

        public void RegisterPanel(string id, GameObject panel)
        {
            EnsureBuilt();
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].id == id)
                {
                    tabs[i] = (id, panel, tabs[i].btn, tabs[i].img);
                    break;
                }
            }
            // 默认显示第一个
            if (string.IsNullOrEmpty(activeId))
            {
                Activate(tabs[0].id);
            }
            else
            {
                panel.SetActive(id == activeId);
            }
        }

        public void Activate(string id)
        {
            activeId = id;
            foreach (var (tid, panel, _, img) in tabs)
            {
                if (panel != null) panel.SetActive(tid == id);
                if (img != null) img.color = tid == id
                    ? new Color(0.42f, 0.32f, 0.55f, 1f)
                    : new Color(0.22f, 0.18f, 0.28f, 1f);
            }
            // 强制刷新刚激活面板的 layout，避免在 inactive 下创建的 VerticalLayoutGroup 不重算
            foreach (var (tid, panel, _, _) in tabs)
            {
                if (tid == id && panel != null)
                {
                    foreach (var rt in panel.GetComponentsInChildren<RectTransform>(true))
                    {
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                    }
                    break;
                }
            }
        }
    }
}
