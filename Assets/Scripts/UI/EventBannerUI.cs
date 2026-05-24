using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Events;

namespace Game.UI
{
    public class EventBannerUI : MonoBehaviour
    {
        private Text label;
        private MainUIBuilder builder;
        private RectTransform banner;
        private bool shown;
        private Coroutine animCo;

        private void Start()
        {
            builder = FindObjectOfType<MainUIBuilder>();
            if (builder == null || builder.BottomBanner == null) { enabled = false; return; }
            banner = builder.BottomBanner;

            var go = new GameObject("BannerText", typeof(Text));
            go.transform.SetParent(banner, false);
            label = go.GetComponent<Text>();
            label.font = UIFont.Get();
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.93f, 0.78f, 1f);
            var rt = label.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // 初始位置 = 隐藏在屏幕下方
            banner.anchoredPosition = new Vector2(0, -banner.sizeDelta.y);
            banner.gameObject.SetActive(false);
        }

        private void Update()
        {
            var sys = RandomEventSystem.Instance;
            if (sys == null) { if (shown) HideNow(); return; }

            if (sys.IsActive)
            {
                if (!shown) Show();
                label.text = $"★ {sys.CurrentLabel}    {sys.CurrentRemaining:F1}s";
            }
            else if (shown)
            {
                Hide();
            }
        }

        private void Show()
        {
            shown = true;
            banner.gameObject.SetActive(true);
            if (animCo != null) StopCoroutine(animCo);
            animCo = StartCoroutine(SlideTo(0));
        }

        private void Hide()
        {
            shown = false;
            if (animCo != null) StopCoroutine(animCo);
            animCo = StartCoroutine(SlideTo(-banner.sizeDelta.y, deactivate: true));
        }

        private void HideNow()
        {
            shown = false;
            banner.anchoredPosition = new Vector2(0, -banner.sizeDelta.y);
            banner.gameObject.SetActive(false);
        }

        private IEnumerator SlideTo(float targetY, bool deactivate = false)
        {
            float startY = banner.anchoredPosition.y;
            float t = 0; float duration = 0.30f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                // ease-out
                float e = 1 - Mathf.Pow(1 - k, 3);
                banner.anchoredPosition = new Vector2(0, Mathf.Lerp(startY, targetY, e));
                yield return null;
            }
            banner.anchoredPosition = new Vector2(0, targetY);
            if (deactivate) banner.gameObject.SetActive(false);
        }
    }
}
