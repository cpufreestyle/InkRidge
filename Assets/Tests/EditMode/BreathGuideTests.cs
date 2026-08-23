using NUnit.Framework;
using InkRidge.Meditation;

namespace InkRidge.Tests
{
    public class BreathGuideTests
    {
        [Test]
        public void Start_SetsPhaseToInhale()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            Assert.AreEqual(BreathGuide.Phase.Inhale, guide.CurrentPhase);
            Assert.AreEqual(0, guide.CompletedCycles);
        }

        [Test]
        public void Update_Balanced444_CompletesCycleIn12Seconds()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            guide.Update(12f);
            Assert.AreEqual(1, guide.CompletedCycles);
        }

        [Test]
        public void Update_Relax478_CompletesCycleIn19Seconds()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Relax478);
            guide.Update(19f);
            Assert.AreEqual(1, guide.CompletedCycles);
        }

        [Test]
        public void Update_Box4444_CompletesCycleIn16Seconds()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Box4444);
            guide.Update(16f);
            Assert.AreEqual(1, guide.CompletedCycles);
        }

        [Test]
        public void Update_MultipleFrames_SumToFullCycle()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            for (int i = 0; i < 12; i++)
                guide.Update(1f);
            Assert.AreEqual(1, guide.CompletedCycles);
        }

        [Test]
        public void Stop_SetsPhaseToIdle()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            guide.Stop();
            Assert.AreEqual(BreathGuide.Phase.Idle, guide.CurrentPhase);
        }

        [Test]
        public void GetRhythmStability_FixedPattern_Returns1()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            guide.Update(12f);
            Assert.AreEqual(1f, guide.GetRhythmStability(), 0.001f);
        }

        [Test]
        public void PhaseProgress_UpdatesDuringInhale()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            guide.Update(2f);
            Assert.AreEqual(0.5f, guide.PhaseProgress, 0.01f);
            Assert.AreEqual(BreathGuide.Phase.Inhale, guide.CurrentPhase);
        }

        [Test]
        public void Update_After4sInhale_TransitionsToHold()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            guide.Update(4f);
            Assert.AreEqual(BreathGuide.Phase.HoldAfterInhale, guide.CurrentPhase);
        }

        [Test]
        public void Update_Balanced444_NoHoldAfterExhale()
        {
            var guide = new BreathGuide();
            guide.Start(BreathGuide.Pattern.Balanced444);
            guide.Update(8f);
            Assert.AreEqual(BreathGuide.Phase.Exhale, guide.CurrentPhase);
            guide.Update(4f);
            Assert.AreEqual(1, guide.CompletedCycles);
            Assert.AreEqual(BreathGuide.Phase.Inhale, guide.CurrentPhase);
        }
    }
}
