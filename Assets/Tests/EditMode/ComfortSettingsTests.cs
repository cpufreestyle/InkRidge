using NUnit.Framework;
using InkRidge.Core;

namespace InkRidge.Tests
{
    public class ComfortSettingsTests
    {
        [Test]
        public void MoveSpeed_Default_Is1_2()
        {
            PlayerPrefs.DeleteKey("Comfort_MoveSpeed");
            Assert.AreEqual(1.2f, ComfortSettings.MoveSpeed);
        }

        [Test]
        public void MoveSpeed_SetValue_Persists()
        {
            ComfortSettings.MoveSpeed = 2.0f;
            Assert.AreEqual(2.0f, ComfortSettings.MoveSpeed);
        }

        [Test]
        public void VignetteEnabled_Default_IsTrue()
        {
            PlayerPrefs.DeleteKey("Comfort_Vignette");
            Assert.IsTrue(ComfortSettings.VignetteEnabled);
        }

        [Test]
        public void SeatedMode_SetFalse_ThenTrue()
        {
            ComfortSettings.SeatedMode = false;
            Assert.IsFalse(ComfortSettings.SeatedMode);
            ComfortSettings.SeatedMode = true;
            Assert.IsTrue(ComfortSettings.SeatedMode);
        }

        [Test]
        public void TurnAngle_SetTo45_Persists()
        {
            ComfortSettings.TurnAngle = 45f;
            Assert.AreEqual(45f, ComfortSettings.TurnAngle);
        }
    }
}
