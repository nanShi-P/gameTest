using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// 程序化生成占位精灵 PNG，供场景调试。真实美术资源同名覆盖即可。
    public static class PlaceholderSpriteGen
    {
        [MenuItem("Alchemist/Art/Generate Placeholders")]
        public static void Generate()
        {
            // 路径放在 Resources 下以便运行时 Resources.Load
            const string root = "Assets/Resources/Art/Sprites";
            Directory.CreateDirectory(root + "/Floor");
            Directory.CreateDirectory(root + "/Cauldron");
            Directory.CreateDirectory(root + "/Characters");

            WriteSolid($"{root}/Floor/floor.png", 32, new Color(0.32f, 0.24f, 0.18f));
            WriteCircle($"{root}/Cauldron/cauldron.png", 64, new Color(0.55f, 0.32f, 0.78f), new Color(1f, 0.85f, 0.55f));

            (string id, Color color, char ch)[] chars =
            {
                ("apprentice",   new Color(0.55f, 0.78f, 0.95f), '学'),
                ("workshop",     new Color(0.95f, 0.78f, 0.55f), '坊'),
                ("spirit",       new Color(0.55f, 0.95f, 0.78f), '精'),
                ("rune_circle",  new Color(0.95f, 0.55f, 0.78f), '阵'),
                ("arcane_mech",  new Color(0.78f, 0.78f, 0.95f), '械'),
                ("dragon_forge", new Color(0.95f, 0.55f, 0.35f), '龙'),
            };
            foreach (var c in chars)
                WriteCharSprite($"{root}/Characters/char_{c.id}.png", 32, c.color, c.ch);

            AssetDatabase.Refresh();
            foreach (var p in Directory.GetFiles(root, "*.png", SearchOption.AllDirectories))
            {
                var ti = AssetImporter.GetAtPath(p.Replace('\\', '/')) as TextureImporter;
                if (ti == null) continue;
                ti.textureType = TextureImporterType.Sprite;
                ti.spritePixelsPerUnit = 32;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.SaveAndReimport();
            }
            Debug.Log($"[PlaceholderSpriteGen] Generated placeholder sprites under {root}");
        }

        private static void WriteSolid(string path, int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = color;
            // 边框深一点
            for (int i = 0; i < size; i++)
            {
                px[i] = px[i + (size - 1) * size] = color * 0.7f;
                px[i * size] = px[i * size + size - 1] = color * 0.7f;
            }
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void WriteCircle(string path, int size, Color bg, Color fg)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float r = size * 0.4f;
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                int i = y * size + x;
                if (d < r * 0.5f) px[i] = fg;
                else if (d < r) px[i] = bg;
                else if (d < r * 1.1f) px[i] = bg * 0.6f;
                else px[i] = new Color(0, 0, 0, 0);
            }
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void WriteCharSprite(string path, int size, Color bg, char ch)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = bg;
            // 简单边框
            for (int i = 0; i < size; i++)
            {
                px[i] = px[i + (size - 1) * size] = Color.black;
                px[i * size] = px[i * size + size - 1] = Color.black;
            }
            // 在中间画一个 12x12 深色块代表"头"，下面 8x12 代表身体
            FillRect(px, size, size / 2 - 6, size - 14, 12, 10, bg * 0.5f);
            FillRect(px, size, size / 2 - 4, 6, 8, 12, bg * 0.7f);
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            // ch 暂未画到位图（Texture2D 没字体），由场景代码再叠 Text 显示
        }

        private static void FillRect(Color[] px, int w, int x, int y, int rw, int rh, Color c)
        {
            for (int j = 0; j < rh; j++)
            for (int i = 0; i < rw; i++)
            {
                int xx = x + i, yy = y + j;
                if (xx < 0 || xx >= w || yy < 0 || yy >= w) continue;
                px[yy * w + xx] = c;
            }
        }
    }
}
