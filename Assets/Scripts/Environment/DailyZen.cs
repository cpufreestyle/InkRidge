using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// "每日一境" (Daily Zen): the scene's mood subtly changes every day,
    /// derived from the date so everyone sees the same sky on the same day.
    /// Derived values nudge fog color/density and wind direction — enough to
    /// make a familiar scene feel fresh without breaking the ink-painting look.
    ///
    /// Also marks today in the save file once per day (zen day streak).
    /// Attach to any persistent object in each meditation scene.
    /// </summary>
    public class DailyZen : MonoBehaviour
    {
        [Header("Variation Strength")]
        [SerializeField, Range(0f, 1f)] private float _fogHueShift = 0.04f;
        [SerializeField, Range(0f, 1f)] private float _fogDensitySwing = 0.30f; // ± of base
        [SerializeField] private bool _varyWindDirection = true;

        private string _dateStr;
        private System.Random _rng;

        public string DateStr => _dateStr;

        void Start()
        {
            _dateStr = System.DateTime.Now.ToString("yyyy-MM-dd");
            _rng = new System.Random(GetStableDateSeed());

            ApplyDailyFog();
            ApplyDailyWind();

            if (Data.SaveManager.MarkZenDay(_dateStr))
                Debug.Log($"[DailyZen] 今日之境打卡 ({_dateStr})，累计 {Data.SaveManager.GetZenDayCount()} 天");
        }

        /// <summary>
        /// Stable seed from the date string (no time-of-day drift): FNV-1a.
        /// </summary>
        private int GetStableDateSeed()
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in _dateStr)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return (int)hash;
            }
        }

        private void ApplyDailyFog()
        {
            if (!RenderSettings.fog) return;

            Color fog = RenderSettings.fogColor;
            Color.RGBToHSV(fog, out float h, out float s, out float v);

            // Nudge hue within the ink-wash neighborhood, never garish.
            h = Mathf.Repeat(h + ((float)_rng.NextDouble() - 0.5f) * 2f * _fogHueShift, 1f);
            v = Mathf.Clamp01(v + ((float)_rng.NextDouble() - 0.5f) * 0.08f);

            RenderSettings.fogColor = Color.HSVToRGB(h, s, v);

            // Density drifts ±30% around whatever the SceneBuilder authored.
            RenderSettings.fogDensity *= 1f + ((float)_rng.NextDouble() * 2f - 1f) * _fogDensitySwing;
        }

        private void ApplyDailyWind()
        {
            if (!_varyWindDirection) return;
            var wind = FindObjectOfType<WindSystem>();
            if (wind == null) return;

            // Expose today's direction through the same global the shader reads.
            float angle = (float)_rng.NextDouble() * Mathf.PI * 2f;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            wind.SetDirection(dir);
        }
    }
}
