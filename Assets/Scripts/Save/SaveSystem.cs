using System;
using System.IO;
using BreakInfinity;
using Newtonsoft.Json;
using UnityEngine;
using Game.Core;

namespace Game.Save
{
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;
        public const string FileName = "save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private static JsonSerializerSettings BuildSettings()
        {
            var s = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };
            s.Converters.Add(new BigDoubleConverter());
            return s;
        }

        public static void Save(GameState state)
        {
            try
            {
                state.version = CurrentVersion;
                string json = JsonConvert.SerializeObject(state, BuildSettings());
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e}");
            }
        }

        public static GameState Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;
                string json = File.ReadAllText(FilePath);
                var loaded = JsonConvert.DeserializeObject<GameState>(json, BuildSettings());
                if (loaded == null) return null;

                if (loaded.version != CurrentVersion)
                {
                    Debug.LogWarning($"[SaveSystem] Save version {loaded.version} != current {CurrentVersion}; loaded with best-effort.");
                }
                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed, ignoring save: {e}");
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }

        public static string GetSavePath() => FilePath;
    }
}
