using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InkRidge.Data;
using InkRidge.Meditation;
using InkRidge.Core;

namespace InkRidge.UI
{
    /// <summary>
    /// End-of-journey summary screen. Shows stats from current + all sessions.
    /// </summary>
    public class SummaryScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _walkTimeText;
        [SerializeField] private TextMeshProUGUI _meditationTimeText;
        [SerializeField] private TextMeshProUGUI _breathCyclesText;
        [SerializeField] private TextMeshProUGUI _stabilityText;
        [SerializeField] private TextMeshProUGUI _totalSessionsText;
        [SerializeField] private TextMeshProUGUI _totalWalkTimeText;
        [SerializeField] private TextMeshProUGUI _totalMeditationTimeText;
        [SerializeField] private Button _restartButton;

        void Start()
        {
            var save = SaveManager.Load();
            int sessionCount = save.meditationRecords.Count;

            float currentMeditationTime = 0f;
            int currentBreathCycles = 0;
            float stabilitySum = 0f;
            int recentStart = Mathf.Max(0, save.meditationRecords.Count - 4);

            for (int i = recentStart; i < save.meditationRecords.Count; i++)
            {
                var data = BreathData.FromJson(save.meditationRecords[i]);
                currentMeditationTime += data.totalDurationSec;
                currentBreathCycles += data.completedCycles;
                stabilitySum += data.rhythmStability;
            }
            float avgStability = recentStart < save.meditationRecords.Count
                ? stabilitySum / (save.meditationRecords.Count - recentStart)
                : 0f;

            _walkTimeText?.SetText($"本次步行: {FormatTime(save.totalWalkingTime)}");
            _meditationTimeText?.SetText($"本次冥想: {FormatTime(currentMeditationTime)}");
            _breathCyclesText?.SetText($"呼吸循环: {currentBreathCycles} 次");
            _stabilityText?.SetText($"呼吸稳定度: {avgStability * 100f:F0}%");

            _totalSessionsText?.SetText($"累计完成: {save.totalSessions} 次 · 每日一境 {SaveManager.GetZenDayCount()} 天");
            _totalWalkTimeText?.SetText($"累计步行: {FormatTime(save.totalWalkingTime)}");
            _totalMeditationTimeText?.SetText($"累计冥想: {FormatTime(save.totalMeditationTime)}");

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestart);
        }

        private string FormatTime(float seconds)
        {
            int min = Mathf.FloorToInt(seconds / 60f);
            int sec = Mathf.FloorToInt(seconds % 60f);
            return $"{min}分{sec}秒";
        }

        private void OnRestart()
        {
            SceneTransition.Instance?.LoadScene(0);
        }
    }
}
