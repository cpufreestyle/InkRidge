using NUnit.Framework;
using InkRidge.Data;
using InkRidge.Meditation;

namespace InkRidge.Tests
{
    public class SaveManagerTests
    {
        [SetUp]
        public void Setup()
        {
            SaveManager.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            SaveManager.Clear();
        }

        [Test]
        public void Load_Empty_ReturnsDefaults()
        {
            var save = SaveManager.Load();
            Assert.AreEqual(0, save.highestUnlockedScene);
            Assert.AreEqual(0, save.totalSessions);
            Assert.AreEqual(0, save.meditationRecords.Count);
        }

        [Test]
        public void UnlockScene_SetsHigherValue()
        {
            SaveManager.UnlockScene(2);
            Assert.AreEqual(2, SaveManager.GetHighestUnlockedScene());
        }

        [Test]
        public void UnlockScene_DoesNotLowerValue()
        {
            SaveManager.UnlockScene(3);
            SaveManager.UnlockScene(1);
            Assert.AreEqual(3, SaveManager.GetHighestUnlockedScene());
        }

        [Test]
        public void AddMeditationRecord_StoresRecord()
        {
            var record = new BreathData(1, "Bamboo", "Balanced444", 5, 60f, 1f);
            SaveManager.AddMeditationRecord(record);
            var save = SaveManager.Load();
            Assert.AreEqual(1, save.totalSessions);
            Assert.AreEqual(60f, save.totalMeditationTime);
            Assert.AreEqual(1, save.meditationRecords.Count);
        }

        [Test]
        public void AddWalkingTime_Accumulates()
        {
            SaveManager.AddWalkingTime(120f);
            SaveManager.AddWalkingTime(60f);
            var save = SaveManager.Load();
            Assert.AreEqual(180f, save.totalWalkingTime);
        }

        [Test]
        public void SaveLoad_RoundTrip_PreservesData()
        {
            var save = new SaveManager.SaveFile
            {
                highestUnlockedScene = 3,
                totalMeditationTime = 300f,
                totalWalkingTime = 600f,
                totalSessions = 5
            };
            SaveManager.Save(save);
            var loaded = SaveManager.Load();
            Assert.AreEqual(3, loaded.highestUnlockedScene);
            Assert.AreEqual(300f, loaded.totalMeditationTime);
            Assert.AreEqual(600f, loaded.totalWalkingTime);
            Assert.AreEqual(5, loaded.totalSessions);
        }

        [Test]
        public void Clear_RemovesAllData()
        {
            SaveManager.UnlockScene(4);
            SaveManager.Clear();
            Assert.AreEqual(0, SaveManager.GetHighestUnlockedScene());
        }
    }
}
