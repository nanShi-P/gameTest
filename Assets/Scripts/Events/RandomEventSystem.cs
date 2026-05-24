using System;
using System.Collections;
using BreakInfinity;
using UnityEngine;
using Game.Core;

namespace Game.Events
{
    public enum EventKind { ManaSurge, ApprenticeInsight, MeteorShower }

    public class RandomEventSystem : MonoBehaviour
    {
        public static RandomEventSystem Instance { get; private set; }

        [Header("Timing")]
        [Tooltip("两次事件之间的最小间隔（秒）")]
        [SerializeField] private float minInterval = 55f;
        [Tooltip("两次事件之间的最大间隔（秒）")]
        [SerializeField] private float maxInterval = 125f;
        [Tooltip("游戏开始后多久才出现第一次事件")]
        [SerializeField] private float firstDelay = 30f;

        public event Action<EventKind, string, float> EventStarted; // kind, label, duration
        public event Action EventEnded;

        public bool IsActive { get; private set; }
        public string CurrentLabel { get; private set; }
        public float CurrentRemaining { get; private set; }

        private Coroutine activeCo;

        public void ClearActiveEvent()
        {
            if (activeCo != null) { StopCoroutine(activeCo); activeCo = null; }
            var gm = GameManager.Instance;
            if (gm != null) { gm.TempGlobalMult = 1.0; gm.TempClickMult = 1.0; }
            IsActive = false; CurrentLabel = null; CurrentRemaining = 0;
            EventEnded?.Invoke();
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(Loop());
        }

        private IEnumerator Loop()
        {
            yield return new WaitForSeconds(firstDelay);
            while (true)
            {
                while (IsActive) yield return null; // 当前事件未结束，等
                float wait = UnityEngine.Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(wait);
                TriggerRandom();
            }
        }

        public void TriggerRandom()
        {
            // 权重：暴走 40 / 灵感 40 / 流星 20
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < 40) Trigger(EventKind.ManaSurge);
            else if (roll < 80) Trigger(EventKind.ApprenticeInsight);
            else Trigger(EventKind.MeteorShower);
        }

        public void Trigger(EventKind kind)
        {
            if (activeCo != null) StopCoroutine(activeCo);
            activeCo = StartCoroutine(RunEvent(kind));
        }

        private IEnumerator RunEvent(EventKind kind)
        {
            var gm = GameManager.Instance;
            string label;
            float duration;

            // 音效
            switch (kind)
            {
                case EventKind.ManaSurge: Game.Audio.AudioManager.Instance?.Play("event_surge"); break;
                case EventKind.ApprenticeInsight: Game.Audio.AudioManager.Instance?.Play("event_insight"); break;
                case EventKind.MeteorShower: Game.Audio.AudioManager.Instance?.Play("event_meteor"); break;
            }

            switch (kind)
            {
                case EventKind.ManaSurge:
                {
                    duration = 30f; label = "魔法暴走 ×7";
                    IsActive = true; CurrentLabel = label; CurrentRemaining = duration;
                    EventStarted?.Invoke(kind, label, duration);
                    gm.TempGlobalMult = 7.0;
                    yield return Countdown(duration);
                    gm.TempGlobalMult = 1.0;
                    break;
                }
                case EventKind.MeteorShower:
                {
                    duration = 60f; label = "流星雨 点击 ×20";
                    IsActive = true; CurrentLabel = label; CurrentRemaining = duration;
                    EventStarted?.Invoke(kind, label, duration);
                    gm.TempClickMult = 20.0;
                    yield return Countdown(duration);
                    gm.TempClickMult = 1.0;
                    break;
                }
                case EventKind.ApprenticeInsight:
                default:
                {
                    duration = 4f; label = "学徒灵感 +60s 产量";
                    IsActive = true; CurrentLabel = label; CurrentRemaining = duration;
                    EventStarted?.Invoke(kind, label, duration);
                    BigDouble bonus = gm.GetProductionPerSecond() * 60;
                    gm.State.manaDust += bonus;
                    gm.State.totalManaEarned += bonus;
                    yield return Countdown(duration);
                    break;
                }
            }

            IsActive = false; CurrentLabel = null; CurrentRemaining = 0;
            EventEnded?.Invoke();
        }

        private IEnumerator Countdown(float duration)
        {
            float t = duration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                CurrentRemaining = Mathf.Max(0, t);
                yield return null;
            }
        }
    }
}
