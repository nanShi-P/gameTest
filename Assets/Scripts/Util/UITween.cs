using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Util
{
    /// 最小可用的 UI 缓动集合。不引入 DOTween。
    /// 通过 key（默认用目标实例）去重，确保同对象同种动画不会叠加。
    public static class UITween
    {
        private static readonly Dictionary<object, Coroutine> running = new();
        private static UITweenRunner runner;

        private static UITweenRunner Runner
        {
            get
            {
                if (runner == null)
                {
                    var go = new GameObject("__UITweenRunner");
                    Object.DontDestroyOnLoad(go);
                    runner = go.AddComponent<UITweenRunner>();
                }
                return runner;
            }
        }

        private static void Replace(object key, IEnumerator co)
        {
            if (running.TryGetValue(key, out var old) && old != null) Runner.StopCoroutine(old);
            running[key] = Runner.StartCoroutine(Wrap(key, co));
        }

        private static IEnumerator Wrap(object key, IEnumerator inner)
        {
            yield return inner;
            running.Remove(key);
        }

        /// 缩放回弹：scale 1 → low → 1
        public static void ScalePunch(Transform t, float low = 0.92f, float total = 0.12f)
        {
            if (t == null) return;
            Replace(("punch", t), Punch(t, low, total));
        }

        private static IEnumerator Punch(Transform t, float low, float total)
        {
            float half = total * 0.5f;
            yield return LerpScalar(0, 1, half, x => SetScale(t, Mathf.Lerp(1f, low, x)));
            yield return LerpScalar(0, 1, half, x => SetScale(t, Mathf.Lerp(low, 1f, x)));
            SetScale(t, 1f);
        }

        private static void SetScale(Transform t, float s)
        {
            if (t != null) t.localScale = new Vector3(s, s, 1);
        }

        /// 平移 + 淡出，结束销毁。常用于飘字。
        public static void MoveAndFade(RectTransform rt, Vector2 deltaPx, float duration, System.Action onDone = null)
        {
            if (rt == null) return;
            var g = rt.GetComponent<Graphic>();
            Replace(("mf", rt), MoveFade(rt, g, deltaPx, duration, onDone));
        }

        private static IEnumerator MoveFade(RectTransform rt, Graphic g, Vector2 deltaPx, float duration, System.Action onDone)
        {
            Vector2 start = rt.anchoredPosition;
            Vector2 end = start + deltaPx;
            Color startC = g != null ? g.color : Color.white;
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (rt != null) rt.anchoredPosition = Vector2.Lerp(start, end, k);
                if (g != null)
                {
                    var c = startC; c.a = startC.a * (1 - k); g.color = c;
                }
                yield return null;
            }
            onDone?.Invoke();
        }

        /// 颜色短闪：g.color → flash → g.color。
        public static void ColorFlash(Graphic g, Color flash, float duration = 0.2f)
        {
            if (g == null) return;
            Replace(("flash", g), Flash(g, flash, duration));
        }

        private static IEnumerator Flash(Graphic g, Color flash, float duration)
        {
            Color baseC = g.color;
            float half = duration * 0.5f;
            yield return LerpScalar(0, 1, half, k => g.color = Color.Lerp(baseC, flash, k));
            yield return LerpScalar(0, 1, half, k => g.color = Color.Lerp(flash, baseC, k));
            g.color = baseC;
        }

        /// 数值滚动：从 from 平滑变到 to，每帧回调 setter。
        public static void LerpNumber(object owner, double from, double to, float duration, System.Action<double> setter)
        {
            if (setter == null) return;
            Replace(("num", owner), Num(from, to, duration, setter));
        }

        private static IEnumerator Num(double from, double to, float duration, System.Action<double> setter)
        {
            if (duration <= 0) { setter(to); yield break; }
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                setter(from + (to - from) * k);
                yield return null;
            }
            setter(to);
        }

        private static IEnumerator LerpScalar(float a, float b, float duration, System.Action<float> setter)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                setter(Mathf.Lerp(a, b, k));
                yield return null;
            }
            setter(b);
        }

        private class UITweenRunner : MonoBehaviour { }
    }
}
