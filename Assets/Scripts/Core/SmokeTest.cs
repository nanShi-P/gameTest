using UnityEngine;
using Newtonsoft.Json;
using BreakInfinity;
using Game.Util;
using Game.Save;

namespace Game.Core
{
    public class SmokeTest : MonoBehaviour
    {
        private void Start()
        {
            // T001: 依赖可用
            BigDouble a = new BigDouble(1e100);
            BigDouble b = a * 10;
            string json = JsonConvert.SerializeObject(new { value = b.ToString() });
            Debug.Log($"[SmokeTest] BigDouble 1e100 * 10 = {b}");
            Debug.Log($"[SmokeTest] Newtonsoft OK: {json}");

            // T002: NumberFormat
            Check("0", NumberFormat.Format(new BigDouble(0)));
            Check("123.4", NumberFormat.Format(new BigDouble(123.4)));
            Check("1.23K", NumberFormat.Format(new BigDouble(1234)));
            Check("1.50M", NumberFormat.Format(new BigDouble(1.5e6)));
            Check("9.99B", NumberFormat.Format(new BigDouble(9.99e9)));
            Check("1.00T", NumberFormat.Format(new BigDouble(1e12)));
            Check("1.00Qa", NumberFormat.Format(new BigDouble(1e15)));
            Check("1.00aa", NumberFormat.Format(new BigDouble(1e18)));
            Check("1.00ab", NumberFormat.Format(new BigDouble(1e21)));
            Check("-1.23K", NumberFormat.Format(new BigDouble(-1234)));

            // T005: BigDoubleConverter 往返
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new BigDoubleConverter());
            RoundTrip(new BigDouble(0), settings);
            RoundTrip(new BigDouble(1234.5), settings);
            RoundTrip(new BigDouble(1e123), settings);
            RoundTrip(new BigDouble(-1e50), settings);
        }

        private static void RoundTrip(BigDouble v, JsonSerializerSettings s)
        {
            string json = JsonConvert.SerializeObject(v, s);
            BigDouble back = JsonConvert.DeserializeObject<BigDouble>(json, s);
            // 比较：用 ToString 容忍 mantissa 浮点抖动
            if (back.ToString() == v.ToString())
                Debug.Log($"[BigDoubleConverter OK] {v} -> {json}");
            else
                Debug.LogError($"[BigDoubleConverter FAIL] in={v} json={json} out={back}");
        }

        private static void Check(string expected, string actual)
        {
            if (expected == actual)
                Debug.Log($"[NumberFormat OK] {actual}");
            else
                Debug.LogError($"[NumberFormat FAIL] expected={expected} actual={actual}");
        }
    }
}
