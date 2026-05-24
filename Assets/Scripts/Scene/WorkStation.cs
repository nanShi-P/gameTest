using System.Collections;
using System.Collections.Generic;
using BreakInfinity;
using UnityEngine;
using Game.Core;
using Game.Util;

namespace Game.Scene
{
    /// 一个生产者的工作站位。在配置位置上根据 GameManager 中的拥有数实时渲染小人（最多 8 个），
    /// 超出在下方显示 "x N" 文本（用 TextMesh）。
    public class WorkStation : MonoBehaviour
    {
        [SerializeField] private string producerId;
        [SerializeField] private Sprite charSprite;
        [SerializeField] private int maxVisible = 8;
        [SerializeField] private Vector2 gridSpacingPx = new Vector2(28, 28);

        private readonly List<GameObject> workers = new();
        private TextMesh countLabel;
        private float lastSync;

        public string ProducerId => producerId;

        public void Init(string id, Sprite sprite)
        {
            producerId = id;
            charSprite = sprite;
            CreateCountLabel();
            StartCoroutine(EmitLoop());
        }

        private IEnumerator EmitLoop()
        {
            // 随机首次延迟避免所有站位同步飘字
            yield return new WaitForSeconds(Random.Range(0.5f, 2.5f));
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2.0f, 3.0f));
                var gm = GameManager.Instance;
                if (gm == null) continue;
                var def = gm.FindProducer(producerId);
                if (def == null) continue;
                int count = gm.State.GetProducerCount(producerId);
                if (count <= 0) continue;
                BigDouble pps = gm.GetProducerProductionPerSecond(def);
                if (pps.CompareTo(BigDouble.Zero) <= 0) continue;
                Vector3 from = transform.position + new Vector3(0, 0.4f, 0);
                Vector3 to = FindCauldronWorldPos();
                WorldFloatingNumber.Spawn(from, to, "+" + NumberFormat.Format(pps),
                    new Color(1f, 0.85f, 0.55f));
            }
        }

        private Vector3 FindCauldronWorldPos()
        {
            var cauldron = GameObject.Find("Cauldron");
            return cauldron != null ? cauldron.transform.position : Vector3.zero;
        }

        private void CreateCountLabel()
        {
            var go = new GameObject("CountLabel", typeof(TextMesh), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, -1.2f, 0);
            countLabel = go.GetComponent<TextMesh>();
            countLabel.fontSize = 32;
            countLabel.characterSize = 0.05f;
            countLabel.alignment = TextAlignment.Center;
            countLabel.anchor = TextAnchor.MiddleCenter;
            countLabel.color = new Color(1f, 0.9f, 0.55f);
            var mr = go.GetComponent<MeshRenderer>();
            mr.sortingOrder = 20;
            countLabel.text = "";
        }

        private void Update()
        {
            if (Time.unscaledTime - lastSync < 0.5f) return;
            lastSync = Time.unscaledTime;
            SyncWorkers();
        }

        private void SyncWorkers()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            int count = gm.State.GetProducerCount(producerId);
            int visible = Mathf.Min(count, maxVisible);

            // 增加
            while (workers.Count < visible)
            {
                var w = SpawnWorker(workers.Count);
                workers.Add(w);
            }
            // 减少
            while (workers.Count > visible)
            {
                int last = workers.Count - 1;
                if (workers[last] != null) Destroy(workers[last]);
                workers.RemoveAt(last);
            }
            // 多余角标
            if (countLabel != null)
            {
                countLabel.text = count > maxVisible ? $"x{count}" : "";
            }
        }

        private GameObject SpawnWorker(int index)
        {
            int col = index % 4;
            int row = index / 4;
            Vector3 local = new Vector3(
                (col - 1.5f) * gridSpacingPx.x / 32f,
                (row - 0.5f) * gridSpacingPx.y / 32f,
                0);
            var go = new GameObject($"Worker_{index}", typeof(SpriteRenderer), typeof(Worker));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = local;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = charSprite;
            sr.sortingOrder = 15;
            if (charSprite == null)
            {
                sr.color = new Color(0.7f, 0.7f, 0.9f);
                go.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            }
            return go;
        }
    }

    /// 单个小人微动协程：随机延迟，做轻微缩放或左右翻转。
    public class Worker : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(Idle());
        }

        private System.Collections.IEnumerator Idle()
        {
            yield return new WaitForSeconds(Random.Range(0f, 1.5f));
            var sr = GetComponent<SpriteRenderer>();
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1f, 2.2f));
                if (Random.value < 0.5f)
                {
                    // 缩放
                    yield return Pulse();
                }
                else if (sr != null)
                {
                    // 翻转
                    sr.flipX = !sr.flipX;
                }
            }
        }

        private System.Collections.IEnumerator Pulse()
        {
            Vector3 baseS = transform.localScale;
            float t = 0; float dur = 0.18f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(baseS, baseS * 1.08f, t / dur);
                yield return null;
            }
            t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(baseS * 1.08f, baseS, t / dur);
                yield return null;
            }
            transform.localScale = baseS;
        }
    }
}
