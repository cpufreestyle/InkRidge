using System;
using UnityEngine;

namespace InkRidge.Meditation
{
    /// <summary>
    /// Serializable record of a single completed meditation session.
    /// </summary>
    [Serializable]
    public class BreathData
    {
        public int sceneIndex;
        public string sceneName;
        public string breathPattern;
        public int completedCycles;
        public float totalDurationSec;
        public float rhythmStability;
        public string timeStampISO;

        public BreathData(int sceneIndex, string sceneName, string breathPattern,
                          int completedCycles, float totalDurationSec, float rhythmStability)
        {
            this.sceneIndex = sceneIndex;
            this.sceneName = sceneName;
            this.breathPattern = breathPattern;
            this.completedCycles = completedCycles;
            this.totalDurationSec = totalDurationSec;
            this.rhythmStability = rhythmStability;
            this.timeStampISO = DateTime.UtcNow.ToString("o");
        }

        public string ToJson() => JsonUtility.ToJson(this);
        public static BreathData FromJson(string json) => JsonUtility.FromJson<BreathData>(json);
    }
}
