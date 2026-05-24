using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    /// 在 MainUIBuilder.ContentArea 内构建生产者列表（垂直滚动）。
    public class ProducersTab : MonoBehaviour
    {
        private readonly List<ProducerRow> rows = new List<ProducerRow>();
        private RectTransform listContent;
        private float lastRefresh;

        private void Start()
        {
            var tc = TabController.Instance;
            if (tc == null || tc.PanelRoot == null)
            {
                if (tc == null) { Debug.LogError("[ProducersTab] TabController missing."); enabled = false; return; }
            }
            var panel = new GameObject("ProducersPanel", typeof(RectTransform));
            tc.RegisterPanel("producers", panel);   // 先触发 PanelRoot 构建
            panel.transform.SetParent(tc.PanelRoot, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            BuildList(rt);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged += RefreshAll;
                RefreshAll();
            }
        }

        private static RectTransform CreatePanel(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= RefreshAll;
        }

        private void Update()
        {
            // 兜底每 0.2s 刷一次（购买按钮的"可购买"状态随 pps 持续变化）
            if (Time.unscaledTime - lastRefresh >= 0.2f)
            {
                lastRefresh = Time.unscaledTime;
                RefreshAll();
            }
        }

        private void BuildList(RectTransform parent)
        {
            // ScrollRect
            var scrollGo = new GameObject("ProducerScroll", typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            var scrollImg = scrollGo.GetComponent<Image>();
            scrollImg.color = new Color(0.10f, 0.08f, 0.13f, 1f);
            var scrollRT = scrollGo.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            // Viewport
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var vImg = viewportGo.GetComponent<Image>();
            vImg.color = new Color(1, 1, 1, 0.01f);
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;
            var vRT = viewportGo.GetComponent<RectTransform>();
            vRT.anchorMin = Vector2.zero;
            vRT.anchorMax = Vector2.one;
            vRT.offsetMin = vRT.offsetMax = Vector2.zero;
            scroll.viewport = vRT;

            // Content
            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var cRT = contentGo.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.offsetMin = cRT.offsetMax = Vector2.zero;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = cRT;
            listContent = cRT;

            // 行
            foreach (var p in GameManager.Instance.Producers)
            {
                var row = new ProducerRow(listContent, p);
                rows.Add(row);
            }
        }

        private void RefreshAll()
        {
            foreach (var r in rows) r.Refresh();
        }
    }
}
