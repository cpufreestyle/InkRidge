using UnityEngine;

namespace InkRidge.Core
{
    /// <summary>
    /// Stores and applies VR comfort settings (persisted via PlayerPrefs).
    /// </summary>
    public static class ComfortSettings
    {
        const string KEY_MOVE_SPEED = "Comfort_MoveSpeed";
        const string KEY_TURN_ANGLE = "Comfort_TurnAngle";
        const string KEY_VIGNETTE = "Comfort_Vignette";
        const string KEY_SEATED = "Comfort_Seated";
        const string KEY_HAPTICS = "Comfort_Haptics";

        const float DEFAULT_MOVE_SPEED = 1.2f;
        const float DEFAULT_TURN_ANGLE = 30f;
        const bool DEFAULT_VIGNETTE = true;
        const bool DEFAULT_SEATED = false;
        const bool DEFAULT_HAPTICS = true;

        // PlayerPrefs are only flushed to disk on a clean quit, and Quest kills
        // backgrounded apps aggressively. Every setter has to force a write or
        // the player's settings silently revert next launch.
        static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static float MoveSpeed
        {
            get => PlayerPrefs.GetFloat(KEY_MOVE_SPEED, DEFAULT_MOVE_SPEED);
            set => SetFloat(KEY_MOVE_SPEED, value);
        }

        public static float TurnAngle
        {
            get => PlayerPrefs.GetFloat(KEY_TURN_ANGLE, DEFAULT_TURN_ANGLE);
            set => SetFloat(KEY_TURN_ANGLE, value);
        }

        public static bool VignetteEnabled
        {
            get => PlayerPrefs.GetInt(KEY_VIGNETTE, DEFAULT_VIGNETTE ? 1 : 0) == 1;
            set => SetBool(KEY_VIGNETTE, value);
        }

        public static bool SeatedMode
        {
            get => PlayerPrefs.GetInt(KEY_SEATED, DEFAULT_SEATED ? 1 : 0) == 1;
            set => SetBool(KEY_SEATED, value);
        }

        /// <summary>Breath-phase controller pulses. See BreathHaptics.</summary>
        public static bool HapticsEnabled
        {
            get => PlayerPrefs.GetInt(KEY_HAPTICS, DEFAULT_HAPTICS ? 1 : 0) == 1;
            set => SetBool(KEY_HAPTICS, value);
        }

        /// <summary>Adjust XR Origin height for seated mode.</summary>
        public static void ApplySeatedMode(Transform xrOrigin)
        {
            if (xrOrigin == null) return;
            float targetY = SeatedMode ? 0.8f : 1.75f;
            xrOrigin.localPosition = new Vector3(
                xrOrigin.localPosition.x,
                targetY,
                xrOrigin.localPosition.z
            );
        }
    }
}
