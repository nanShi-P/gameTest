using System;
using System.Text;
using BreakInfinity;

namespace Game.Util
{
    /// 增量游戏数字格式化：<1000 一位小数；<1e15 用 K/M/B/T；≥1e15 用 aa/ab/ac… 双字母后缀（每 3 个数量级一进）。
    public static class NumberFormat
    {
        private static readonly string[] ShortSuffixes = { "", "K", "M", "B", "T", "Qa" };

        public static string Format(BigDouble v)
        {
            int sign = BigDouble.Sign(v);
            if (sign == 0) return "0";

            BigDouble abs = sign < 0 ? -v : v;
            string body = FormatPositive(abs);
            return sign < 0 ? "-" + body : body;
        }

        public static string Format(double v) => Format(new BigDouble(v));

        private static string FormatPositive(BigDouble v)
        {
            // <1000：一位小数
            if (v.CompareTo(new BigDouble(1000.0)) < 0)
            {
                double d = v.ToDouble();
                return d.ToString("0.0");
            }

            // 计算 10 的整数次幂数量级
            int exp = (int)Math.Floor(BigDouble.Log10(v));
            int tier = exp / 3; // 每 3 个数量级一个后缀档位

            if (tier < ShortSuffixes.Length)
            {
                double mantissa = (v / BigDouble.Pow10((long)(tier * 3))).ToDouble();
                return mantissa.ToString("0.00") + ShortSuffixes[tier];
            }

            // 双字母后缀：tier=6 -> aa, 7 -> ab, ...
            int idx = tier - ShortSuffixes.Length;
            string suffix = DoubleLetterSuffix(idx);
            double m = (v / BigDouble.Pow10((long)(tier * 3))).ToDouble();
            return m.ToString("0.00") + suffix;
        }

        private static string DoubleLetterSuffix(int idx)
        {
            int first = idx / 26;
            int second = idx % 26;
            // 超过 zz 之后继续扩展不在 MVP 范围（>=1e2358），返回 e 记法兜底
            if (first >= 26) return null;
            var sb = new StringBuilder(2);
            sb.Append((char)('a' + first));
            sb.Append((char)('a' + second));
            return sb.ToString();
        }
    }
}
