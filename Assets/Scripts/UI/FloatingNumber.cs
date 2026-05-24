using UnityEngine;
using UnityEngine.UI;
using Game.Util;

namespace Game.UI
{
    /// 飘字：在 OverlayLayer 上生成一个临时 Text，向上飘 60px 并淡出后销毁。
    public class FloatingNumber : MonoBehaviour
    {
        public static void Spawn(RectTransform parent, Vector2 anchoredPos, string text, Color color)
        {
            if (parent == null) return;
            var go = new GameObject("Float", typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = UIFont.Get();
            t.fontSize = 18;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            var rt = t.rectTransform;
            rt.sizeDelta = new Vector2(80, 24);
            rt.anchoredPosition = anchoredPos
                + new Vector2(Random.Range(-20f, 20f), 0);

            UITween.MoveAndFade(rt, new Vector2(0, 60), 0.7f, () =>
            {
                if (go != null) Destroy(go);
            });
        }
    }
}
