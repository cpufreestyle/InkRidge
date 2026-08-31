using System;
using UnityEngine;

namespace InkRidge.Meditation
{
    /// <summary>
    /// Controls breathing rhythm phases. Drives visual (BreathCircle shader)
    /// and audio cues. Not a MonoBehaviour — instantiated by MeditationPoint.
    /// </summary>
    public class BreathGuide
    {
        public enum Phase { Inhale, HoldAfterInhale, Exhale, HoldAfterExhale, Idle }

        /// <summary>
        /// Fired whenever the guide moves to a new phase — including the first
        /// Inhale from Start() and the final Idle from Stop().
        ///
        /// This is the single thing audio and haptics subscribe to. The old
        /// design had BreathAudioSync expose a public OnPhaseChanged() that
        /// nothing ever called, so breath audio never played at all. Don't
        /// reintroduce a pull-based API here.
        /// </summary>
        public event Action<Phase> PhaseChanged;

        public enum Pattern
        {
            Balanced444,   // 4-4-4 (inhale-hold-exhale, no hold after exhale)
            Relax478,      // 4-7-8
            Box4444,       // 4-4-4-4
            Free            // variable, guided by slow deep breaths
        }

        public Phase CurrentPhase { get; private set; } = Phase.Idle;
        public float PhaseProgress { get; private set; }
        public int CompletedCycles { get; private set; }
        public Pattern ActivePattern { get; private set; }

        private static readonly float[,] PatternTimings = new float[,]
        {
            { 4f, 4f, 4f, 0f },    // Balanced444
            { 4f, 7f, 8f, 0f },    // Relax478
            { 4f, 4f, 4f, 4f },    // Box4444
            { 6f, 2f, 8f, 0f },    // Free (slow deep)
        };

        private float _phaseTimer;
        private float _totalTimer;
        private bool _running;

        private float _cycleDurationSum;
        private float _cycleDurationSqSum;
        private int _cycleCount;

        public void Start(Pattern pattern)
        {
            ActivePattern = pattern;
            CurrentPhase = Phase.Inhale;
            PhaseProgress = 0f;
            _phaseTimer = 0f;
            _totalTimer = 0f;
            CompletedCycles = 0;
            _cycleDurationSum = 0f;
            _cycleDurationSqSum = 0f;
            _cycleCount = 0;
            _running = true;

            PhaseChanged?.Invoke(CurrentPhase);
        }

        public void Stop()
        {
            _running = false;
            CurrentPhase = Phase.Idle;
            PhaseProgress = 0f;

            PhaseChanged?.Invoke(Phase.Idle);
        }

        /// <summary>Advance the breath cycle. Call every frame with Time.deltaTime.</summary>
        public void Update(float deltaTime)
        {
            if (!_running) return;

            _totalTimer += deltaTime;
            _phaseTimer += deltaTime;

            int patternIndex = (int)ActivePattern;
            float phaseDuration = GetPhaseDuration(patternIndex, CurrentPhase);

            if (phaseDuration <= 0f)
            {
                AdvancePhase();
                return;
            }

            PhaseProgress = Mathf.Clamp01(_phaseTimer / phaseDuration);

            if (_phaseTimer >= phaseDuration)
            {
                AdvancePhase();
            }
        }

        private float GetPhaseDuration(int patternIndex, Phase phase)
        {
            switch (phase)
            {
                case Phase.Inhale: return PatternTimings[patternIndex, 0];
                case Phase.HoldAfterInhale: return PatternTimings[patternIndex, 1];
                case Phase.Exhale: return PatternTimings[patternIndex, 2];
                case Phase.HoldAfterExhale: return PatternTimings[patternIndex, 3];
                default: return 0f;
            }
        }

        private void AdvancePhase()
        {
            int patternIndex = (int)ActivePattern;

            switch (CurrentPhase)
            {
                case Phase.Inhale:
                    CurrentPhase = PatternTimings[patternIndex, 1] > 0
                        ? Phase.HoldAfterInhale : Phase.Exhale;
                    break;
                case Phase.HoldAfterInhale:
                    CurrentPhase = Phase.Exhale;
                    break;
                case Phase.Exhale:
                    CompletedCycles++;
                    RecordCycleDuration();
                    CurrentPhase = PatternTimings[patternIndex, 3] > 0
                        ? Phase.HoldAfterExhale : Phase.Inhale;
                    break;
                case Phase.HoldAfterExhale:
                    CurrentPhase = Phase.Inhale;
                    break;
                default:
                    CurrentPhase = Phase.Inhale;
                    break;
            }

            _phaseTimer = 0f;
            PhaseProgress = 0f;

            PhaseChanged?.Invoke(CurrentPhase);
        }

        private void RecordCycleDuration()
        {
            int patternIndex = (int)ActivePattern;
            float cycleDur = PatternTimings[patternIndex, 0] +
                            PatternTimings[patternIndex, 1] +
                            PatternTimings[patternIndex, 2] +
                            PatternTimings[patternIndex, 3];
            _cycleDurationSum += cycleDur;
            _cycleDurationSqSum += cycleDur * cycleDur;
            _cycleCount++;
        }

        /// <summary>Rhythm stability score 0-1. Always 1 for fixed patterns.</summary>
        public float GetRhythmStability()
        {
            if (_cycleCount == 0) return 1f;
            float mean = _cycleDurationSum / _cycleCount;
            float variance = (_cycleDurationSqSum / _cycleCount) - (mean * mean);
            return Mathf.Clamp01(1f - Mathf.Sqrt(Mathf.Max(0f, variance)) / mean);
        }

        public float GetTotalDuration() => _totalTimer;
    }
}
