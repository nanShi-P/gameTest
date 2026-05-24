using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    /// 极简 AudioManager：从 Resources/Audio/{SFX,BGM} 自动加载所有 AudioClip。
    /// 缺资源时静默（仅 Debug.Log 警告）。
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField, Range(0, 1)] private float masterVolume = 1f;
        [SerializeField, Range(0, 1)] private float sfxVolume = 1f;
        [SerializeField, Range(0, 1)] private float bgmVolume = 0.4f;
        [SerializeField] private int sfxPoolSize = 4;

        private readonly Dictionary<string, AudioClip> sfxClips = new();
        private readonly Dictionary<string, AudioClip> bgmClips = new();
        private readonly HashSet<string> missingWarned = new();

        private AudioSource[] sfxPool;
        private int sfxIdx;
        private AudioSource bgmSource;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动加载
            foreach (var c in Resources.LoadAll<AudioClip>("Audio/SFX")) sfxClips[c.name] = c;
            foreach (var c in Resources.LoadAll<AudioClip>("Audio/BGM")) bgmClips[c.name] = c;

            sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                sfxPool[i] = gameObject.AddComponent<AudioSource>();
                sfxPool[i].playOnAwake = false;
            }
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            Debug.Log($"[AudioManager] SFX loaded: {sfxClips.Count}, BGM loaded: {bgmClips.Count}");
        }

        private void Start()
        {
            // 自动播 BGM（缺资源时静默）
            PlayBGM("bgm_main");
        }

        public void Play(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!sfxClips.TryGetValue(key, out var clip))
            {
                if (missingWarned.Add(key)) Debug.Log($"[AudioManager] sfx '{key}' not found — silent.");
                return;
            }
            var src = sfxPool[sfxIdx];
            sfxIdx = (sfxIdx + 1) % sfxPool.Length;
            src.volume = masterVolume * sfxVolume;
            src.PlayOneShot(clip);
        }

        public void PlayBGM(string key)
        {
            if (string.IsNullOrEmpty(key)) { bgmSource.Stop(); return; }
            if (!bgmClips.TryGetValue(key, out var clip))
            {
                if (missingWarned.Add("bgm:" + key)) Debug.Log($"[AudioManager] bgm '{key}' not found — silent.");
                return;
            }
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;
            bgmSource.clip = clip;
            bgmSource.volume = masterVolume * bgmVolume;
            bgmSource.Play();
        }

        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); ApplyVolume(); }
        public void SetSfxVolume(float v) { sfxVolume = Mathf.Clamp01(v); ApplyVolume(); }
        public void SetBgmVolume(float v) { bgmVolume = Mathf.Clamp01(v); ApplyVolume(); }

        private void ApplyVolume()
        {
            if (bgmSource != null) bgmSource.volume = masterVolume * bgmVolume;
        }
    }
}
