using UnityEngine;
using Game.Core;

namespace Game.Scene
{
    /// 构建 2D 顶下视角的炼金工坊场景。
    /// 使用方法：场景里新建空对象 "WorkshopRoot" 挂此脚本即可。
    /// 注意：本脚本在 Awake 时配置 Camera.main 为正交 + 黑紫色背景，
    /// 不要再在 Inspector 中改 MainCamera 参数。
    public class WorkshopSceneBuilder : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private int floorTilesX = 10;
        [SerializeField] private int floorTilesY = 7;
        [SerializeField] private Vector2 cauldronOffset = new Vector2(0, 80);
        [SerializeField] private float orthoSize = 4f;

        private void Awake()
        {
            ConfigureCamera();
            BuildFloor();
            BuildCauldron();
            BuildStations();
        }

        // §11.2 6 个站位坐标（屏幕像素）
        private static readonly (string id, Vector2 pos)[] StationLayout =
        {
            ("apprentice",   new Vector2(-160, -100)),
            ("workshop",     new Vector2( 160, -100)),
            ("spirit",       new Vector2(-200,   60)),
            ("rune_circle",  new Vector2( 200,   60)),
            ("arcane_mech",  new Vector2(-120,  200)),
            ("dragon_forge", new Vector2( 120,  200)),
        };

        private void BuildStations()
        {
            var stationRoot = new GameObject("Stations").transform;
            stationRoot.SetParent(transform, false);
            foreach (var (id, pos) in StationLayout)
            {
                var sprite = LoadSprite($"Art/Sprites/Characters/char_{id}");
                var go = new GameObject($"Station_{id}", typeof(WorkStation));
                go.transform.SetParent(stationRoot, false);
                go.transform.localPosition = new Vector3(pos.x / 32f, pos.y / 32f, 0);
                go.GetComponent<WorkStation>().Init(id, sprite);
            }
        }

        private void ConfigureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera));
                go.tag = "MainCamera";
                cam = go.GetComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0, 0, -10);
        }

        private void BuildFloor()
        {
            var floorSprite = LoadSprite("Art/Sprites/Floor/floor");
            if (floorSprite == null)
            {
                Debug.LogWarning("[WorkshopSceneBuilder] floor sprite missing, skipping floor.");
                return;
            }
            var root = new GameObject("Floor").transform;
            root.SetParent(transform, false);
            for (int y = 0; y < floorTilesY; y++)
            for (int x = 0; x < floorTilesX; x++)
            {
                var t = new GameObject($"Tile_{x}_{y}", typeof(SpriteRenderer));
                t.transform.SetParent(root, false);
                t.transform.localPosition = new Vector3(
                    (x - floorTilesX * 0.5f + 0.5f) * 1f,
                    (y - floorTilesY * 0.5f + 0.5f) * 1f,
                    0);
                var sr = t.GetComponent<SpriteRenderer>();
                sr.sprite = floorSprite;
                sr.sortingOrder = -10;
                // 棋盘色变化
                if ((x + y) % 2 == 0) sr.color = new Color(0.85f, 0.85f, 0.85f);
            }
        }

        private void BuildCauldron()
        {
            var sprite = LoadSprite("Art/Sprites/Cauldron/cauldron");
            var go = new GameObject("Cauldron", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(CauldronClickHandler));
            go.transform.SetParent(transform, false);
            // 用 UI 坐标转世界单位：1 px ≈ 1/32 unit
            go.transform.localPosition = new Vector3(cauldronOffset.x / 32f, cauldronOffset.y / 32f, 0);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            if (sprite == null)
            {
                Debug.LogWarning("[WorkshopSceneBuilder] cauldron sprite missing — using fallback colored square.");
                sr.color = new Color(0.55f, 0.32f, 0.78f);
                // 没有 sprite 也得有可点区域
                go.transform.localScale = new Vector3(2, 2, 1);
            }
            var col = go.GetComponent<BoxCollider2D>();
            col.size = sprite != null ? sprite.bounds.size : new Vector2(2, 2);
        }

        public static Sprite LoadSprite(string resourcesPath)
        {
            return Resources.Load<Sprite>(resourcesPath);
        }
    }

    public class CauldronClickHandler : MonoBehaviour
    {
        private void OnMouseDown()
        {
            GameManager.Instance?.Click();
            // 小缩放反馈
            StartCoroutine(PunchScale());
        }

        private System.Collections.IEnumerator PunchScale()
        {
            Vector3 baseS = transform.localScale;
            float total = 0.12f;
            float half = total * 0.5f;
            float t = 0;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(baseS, baseS * 0.92f, t / half);
                yield return null;
            }
            t = 0;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(baseS * 0.92f, baseS, t / half);
                yield return null;
            }
            transform.localScale = baseS;
        }
    }
}
