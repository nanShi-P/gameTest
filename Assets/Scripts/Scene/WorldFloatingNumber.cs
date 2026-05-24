using System.Collections;
using UnityEngine;

namespace Game.Scene
{
    /// 场景内 TextMesh 飘字：从 from 飞向 to，时长内淡出。
    public class WorldFloatingNumber : MonoBehaviour
    {
        public static void Spawn(Vector3 from, Vector3 to, string text, Color color, float duration = 0.8f)
        {
            var go = new GameObject("WorldFloat", typeof(TextMesh), typeof(MeshRenderer), typeof(WorldFloatingNumber));
            go.transform.position = from;
            var tm = go.GetComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 32;
            tm.characterSize = 0.06f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = color;
            go.GetComponent<MeshRenderer>().sortingOrder = 30;
            go.GetComponent<WorldFloatingNumber>().Init(from, to, color, duration, tm);
        }

        private void Init(Vector3 from, Vector3 to, Color color, float duration, TextMesh tm)
        {
            StartCoroutine(Run(from, to, color, duration, tm));
        }

        private IEnumerator Run(Vector3 from, Vector3 to, Color color, float duration, TextMesh tm)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                transform.position = Vector3.Lerp(from, to, k);
                var c = color; c.a = 1 - k;
                if (tm != null) tm.color = c;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
