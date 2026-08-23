using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using InkRidge.Meditation;

namespace InkRidge.Data
{
    /// <summary>
    /// Manages local JSON save/load for meditation records and cumulative stats.
    /// File location: Application.persistentDataPath / "inkridge_save.json"
    /// </summary>
    public class SaveManager
    {
        [Serializable]
        public class SaveFile
        {
            public int highestUnlockedScene;
            public float totalMeditationTime;
            public float totalWalkingTime;
            public int totalSessions;
            public List<string> meditationRecords = new List<string>();
        }

        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "inkridge_save.json");

        public static SaveFile Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    return JsonUtility.FromJson<SaveFile>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager.Load failed: {e.Message}");
            }
            return new SaveFile();
        }

        public static void Save(SaveFile data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager.Save failed: {e.Message}");
            }
        }

        public static void AddMeditationRecord(BreathData record)
        {
            var save = Load();
            save.meditationRecords.Add(record.ToJson());
            save.totalMeditationTime += record.totalDurationSec;
            save.totalSessions++;
            Save(save);
        }

        public static void UnlockScene(int sceneIndex)
        {
            var save = Load();
            if (sceneIndex > save.highestUnlockedScene)
            {
                save.highestUnlockedScene = sceneIndex;
                Save(save);
            }
        }

        public static int GetHighestUnlockedScene()
        {
            return Load().highestUnlockedScene;
        }

        public static void AddWalkingTime(float seconds)
        {
            var save = Load();
            save.totalWalkingTime += seconds;
            Save(save);
        }

        /// <summary>Clear all save data. For testing.</summary>
        public static void Clear()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
