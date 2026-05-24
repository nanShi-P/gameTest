using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;
using Game.Core;
using Game.Util;

namespace Game.UI
{
    /// 重构后的主 UI：
    /// - 顶部 HUD 条（魔力尘 / 产出/秒 / 点击 +N）常驻
    /// - 右侧可折叠侧栏：含 TabController 标签页（生产者/升级/转生）
    /// - 不再有左侧炼金炉按钮（已移到 Scene 中的 Cauldron 对象）
    public class MainUIBuilder : MonoBehaviour
    {
        public RectTransform ContentArea { get; private set; }     // TabController 的容器
        public RectTransform BottomBanner { get; private set; }
        public RectTransform OverlayLayer { get; private set; }
        public Transform Cauldron => null; // 兼容旧引用；场景里 Cauldron 取代

        // 兼容旧字段（其他脚本可能引用）
        public RectTransform LeftPanel { get; private set; }
        public RectTransform RightPanel { get; private set; }

        private Text hudManaText;
        private Text hudPpsText;
        private Text hudClickText;
        private RectTransform sidebar;
        private bool sidebarOpen;

        private float lastUITick;
        private bool dirty = true;
        private double displayedMana, displayedPps;

        private void Awake() { BuildCanvas(); }

        private void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.StateChanged += MarkDirty;
        }
        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.StateChanged -= MarkDirty;
        }
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= MarkDirty;
                GameManager.Instance.StateChanged += MarkDirty;
                displayedMana = GameManager.Instance.State.manaDust.ToDouble();
                displayedPps = GameManager.Instance.GetProductionPerSecond().ToDouble();
            }
            MarkDirty();
        }

        private void MarkDirty() => dirty = true;

        private void Update()
        {
            if (Time.unscaledTime - lastUITick >= 0.1f) { lastUITick = Time.unscaledTime; dirty = true; }
            if (dirty) { dirty = false; RefreshHUD(); }
        }

        private void RefreshHUD()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            double tm = gm.State.manaDust.ToDouble();
            double tp = gm.GetProductionPerSecond().ToDouble();
            double tc = gm.ClickProduction().ToDouble();
            UITween.LerpNumber(hudManaText, displayedMana, tm, 0.15f, v =>
            {
                displayedMana = v;
                if (hudManaText != null) hudManaText.text = "魔力尘  " + NumberFormat.Format(new BigDouble(v));
            });
            UITween.LerpNumber(hudPpsText, displayedPps, tp, 0.15f, v =>
            {
                displayedPps = v;
                if (hudPpsText != null) hudPpsText.text = "产出/秒  " + NumberFormat.Format(new BigDouble(v));
            });
            if (hudClickText != null) hudClickText.text = "点击  +" + NumberFormat.Format(new BigDouble(tc));
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }

            var canvasRT = canvasGo.GetComponent<RectTransform>();

            BuildHUD(canvasRT);
            BuildSidebar(canvasRT);

            BottomBanner = CreatePanel(canvasRT, "BottomBanner", new Color(0.18f, 0.10f, 0.06f, 0.85f));
            BottomBanner.anchorMin = new Vector2(0, 0);
            BottomBanner.anchorMax = new Vector2(1, 0);
            BottomBanner.pivot = new Vector2(0.5f, 0);
            BottomBanner.sizeDelta = new Vector2(0, 40);
            BottomBanner.anchoredPosition = Vector2.zero;
            BottomBanner.gameObject.SetActive(false);

            var overlayGo = new GameObject("OverlayLayer", typeof(RectTransform), typeof(CanvasGroup));
            overlayGo.transform.SetParent(canvasRT, false);
            overlayGo.GetComponent<CanvasGroup>().blocksRaycasts = false;
            OverlayLayer = overlayGo.GetComponent<RectTransform>();
            Stretch(OverlayLayer);
            OverlayLayer.SetAsLastSibling();
        }

        private void BuildHUD(RectTransform canvasRT)
        {
            var hud = CreatePanel(canvasRT, "HUD", new Color(0.08f, 0.06f, 0.10f, 0.85f));
            hud.anchorMin = new Vector2(0, 1);
            hud.anchorMax = new Vector2(1, 1);
            hud.pivot = new Vector2(0.5f, 1);
            hud.sizeDelta = new Vector2(0, 48);
            hud.anchoredPosition = Vector2.zero;

            hudManaText = MakeText(hud, "ManaText", "魔力尘  0", 18, TextAnchor.MiddleLeft,
                new Vector2(0, 0), new Vector2(0.33f, 1));
            hudPpsText = MakeText(hud, "PpsText", "产出/秒  0", 16, TextAnchor.MiddleCenter,
                new Vector2(0.33f, 0), new Vector2(0.66f, 1));
            hudClickText = MakeText(hud, "ClickText", "点击  +1", 14, TextAnchor.MiddleRight,
                new Vector2(0.66f, 0), new Vector2(1, 1));
        }

        private void BuildSidebar(RectTransform canvasRT)
        {
            sidebar = CreatePanel(canvasRT, "Sidebar", new Color(0.10f, 0.08f, 0.14f, 0.95f));
            // 占右侧 40%，从屏幕外滑入
            sidebar.anchorMin = new Vector2(1, 0);
            sidebar.anchorMax = new Vector2(1, 1);
            sidebar.pivot = new Vector2(1, 0.5f);
            sidebar.sizeDelta = new Vector2(512, -48); // 高度 = 全高 - HUD 高
            sidebar.anchoredPosition = new Vector2(sidebar.sizeDelta.x, -24); // 初始隐藏到屏幕外

            ContentArea = CreatePanel(sidebar, "ContentArea", new Color(0.14f, 0.11f, 0.18f, 1f));
            ContentArea.anchorMin = new Vector2(0, 0);
            ContentArea.anchorMax = new Vector2(1, 1);
            ContentArea.offsetMin = new Vector2(8, 8);
            ContentArea.offsetMax = new Vector2(-8, -8);

            // 兼容旧字段
            RightPanel = sidebar;
            LeftPanel = null;

            BuildToggleButton(canvasRT);
        }

        private void BuildToggleButton(RectTransform canvasRT)
        {
            var btnGo = new GameObject("SidebarToggle", typeof(Image), typeof(Button));
            btnGo.transform.SetParent(canvasRT, false);
            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.42f, 0.32f, 0.55f, 0.95f);
            var rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(56, 40);
            rt.anchoredPosition = new Vector2(-8, -56);

            var lbl = MakeText(rt, "Label", "☰", 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            lbl.color = Color.white;

            btnGo.GetComponent<Button>().onClick.AddListener(ToggleSidebar);
        }

        private void ToggleSidebar()
        {
            sidebarOpen = !sidebarOpen;
            StopAllCoroutines();
            StartCoroutine(SlideSidebar(sidebarOpen ? 0 : sidebar.sizeDelta.x));
        }

        private System.Collections.IEnumerator SlideSidebar(float targetX)
        {
            float startX = sidebar.anchoredPosition.x;
            float t = 0; float dur = 0.25f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = 1 - Mathf.Pow(1 - k, 3);
                sidebar.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, e), sidebar.anchoredPosition.y);
                yield return null;
            }
            sidebar.anchoredPosition = new Vector2(targetX, sidebar.anchoredPosition.y);
        }

        // ------- 工具 -------

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }
        private static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            return CreateImage(parent, name, color).rectTransform;
        }
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
            rt.offsetMin = new Vector2(12, 0); rt.offsetMax = new Vector2(-12, 0);
            return t;
        }
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }
}
