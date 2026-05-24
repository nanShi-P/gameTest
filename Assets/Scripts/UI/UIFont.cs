using UnityEngine;

namespace Game.UI
{
    /// 统一字体获取。优先使用 Windows 系统中文字体（微软雅黑），找不到则降级。
    public static class UIFont
    {
        private static Font cached;

        public static Font Get()
        {
            if (cached != null) return cached;

            // 优先级：微软雅黑 → 苹方 → 思源 → Arial → Unity 内置
            string[] candidates =
            {
                "Microsoft YaHei UI", "Microsoft YaHei", "微软雅黑",
                "PingFang SC", "Source Han Sans CN", "SimHei", "Arial Unicode MS"
            };

            foreach (var name in candidates)
            {
                var f = Font.CreateDynamicFontFromOSFont(name, 16);
                if (f != null) { cached = f; return cached; }
            }

            // 兜底
            cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return cached;
        }
    }
}
