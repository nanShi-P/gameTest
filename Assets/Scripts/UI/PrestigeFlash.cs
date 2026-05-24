using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// 转生时的全屏闪光。由 PrestigeTab 触发。
    public class PrestigeFlash : MonoBehaviour
    {
        public static void Trigger()
        {
            var builder = FindObjectOfType<MainUIBuilder>();
            if (builder == null || builder.OverlayLayer == null) return;

            var go = new GameObject("PrestigeFlash", typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(builder.OverlayLayer, false);
            var img = go.GetComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var runner = go.AddComponent<FlashRunner>();
            runner.Run(img);
        }

        private class FlashRunner : MonoBehaviour
        {
            public void Run(Image img) => StartCoroutine(Anim(img));

            private IEnumerator Anim(Image img)
            {
                // 1) 0.15s 白光淡入到全亮
                yield return Fade(img, Color.white, new Color(1, 1, 1, 1), 0.15f);
                // 2) 0.4s 白→紫
                yield return Fade(img, new Color(1, 1, 1, 1), new Color(0.65f, 0.35f, 0.85f, 0.8f), 0.4f);
                // 3) 0.65s 紫→透明
                yield return Fade(img, new Color(0.65f, 0.35f, 0.85f, 0.8f), new Color(0.65f, 0.35f, 0.85f, 0f), 0.65f);
                Destroy(gameObject);
            }

            private IEnumerator Fade(Image img, Color from, Color to, float duration)
            {
                float t = 0;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / duration);
                    img.color = Color.Lerp(from, to, k);
                    yield return null;
                }
                img.color = to;
            }
        }
    }
}
