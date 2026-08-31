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

        /// <summary>
        /// True when the player ended the session early via hold-to-exit.
        /// Early exits keep their record but do not unlock the next scene,
        /// so history stays honest while the journey stays completable.
        /// </summary>
        public bool completedEarly;

        public BreathData(int sceneIndex, string sceneName, string breathPattern,
                          int completedCycles, float totalDurationSec, float rhythmStability,
                          bool completedEarly = false)
        {
            this.sceneIndex = sceneIndex;
            this.sceneName = sceneName;
            this.breathPattern = breathPattern;
            this.completedCycles = completedCycles;
            this.totalDurationSec = totalDurationSec;
            this.rhythmStability = rhythmStability;
            this.completedEarly = completedEarly;
            this.timeStampISO = DateTime.UtcNow.ToString("o");
        }

        public string ToJson() => JsonUtility.ToJson(this);
        public static BreathData FromJson(string json) => JsonUtility.FromJson<BreathData>(json);
    }
}
