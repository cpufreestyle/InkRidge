using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using InkRidge.Environment;

namespace InkRidge.Tests
{
    [TestFixture]
    public class WindSystemTests
    {
        private GameObject _go;
        private WindSystem _wind;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("WindSystemTest");
            _wind = _go.AddComponent<WindSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            // Reset shader globals to avoid cross-test pollution
            Shader.SetGlobalFloat(Shader.PropertyToID("_WindSpeed"), 0f);
            Shader.SetGlobalFloat(Shader.PropertyToID("_WindMagnitude"), 0f);
            Shader.SetGlobalFloat(Shader.PropertyToID("_WindTurbulence"), 0f);
            Shader.SetGlobalFloat(Shader.PropertyToID("_WindDirectionX"), 0f);
            Shader.SetGlobalFloat(Shader.PropertyToID("_WindDirectionZ"), 0f);
            Shader.SetGlobalFloat(Shader.PropertyToID("_GustDensity"), 0f);
            Shader.SetGlobalFloat(Shader.PropertyToID("_WindIntensity"), 0f);

            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        // ── Component creation ──

        [Test]
        public void AddComponent_Succeeds()
        {
            Assert.IsNotNull(_wind, "WindSystem should be addable as a component");
        }

        // ── SetIntensity clamping ──

        [Test]
        public void SetIntensity_NormalValue_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _wind.SetIntensity(0.5f));
        }

        [Test]
        public void SetIntensity_Zero_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _wind.SetIntensity(0f));
        }

        [Test]
        public void SetIntensity_One_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _wind.SetIntensity(1f));
        }

        [Test]
        public void SetIntensity_AboveOne_ClampsToValidRange()
        {
            _wind.SetIntensity(2.0f);
            InvokeUpdate();

            var intensityId = Shader.PropertyToID("_WindIntensity");
            float shaderIntensity = Shader.GetGlobalFloat(intensityId);
            Assert.LessOrEqual(shaderIntensity, 1.01f,
                "Intensity should be clamped to [0,1]");
        }

        [Test]
        public void SetIntensity_BelowZero_ClampsToValidRange()
        {
            _wind.SetIntensity(-1.0f);
            InvokeUpdate();

            var intensityId = Shader.PropertyToID("_WindIntensity");
            float shaderIntensity = Shader.GetGlobalFloat(intensityId);
            Assert.GreaterOrEqual(shaderIntensity, -0.01f,
                "Intensity should be clamped to [0,1]");
        }

        [Test]
        public void SetIntensity_NegativeThenPositive_BothAccepted()
        {
            Assert.DoesNotThrow(() =>
            {
                _wind.SetIntensity(-0.5f);
                _wind.SetIntensity(0.5f);
                _wind.SetIntensity(1.5f);
            });
        }

        // ── Shader globals ──

        [Test]
        public void Update_SetsWindSpeedShaderGlobal()
        {
            InvokeUpdate();
            var id = Shader.PropertyToID("_WindSpeed");
            // Default _windSpeed = 3.0f
            Assert.AreEqual(3.0f, Shader.GetGlobalFloat(id), 0.01f,
                "WindSpeed shader global should match default value");
        }

        [Test]
        public void Update_SetsWindTurbulenceShaderGlobal()
        {
            InvokeUpdate();
            var id = Shader.PropertyToID("_WindTurbulence");
            // Default _windTurbulence = 2.5f
            Assert.AreEqual(2.5f, Shader.GetGlobalFloat(id), 0.01f,
                "WindTurbulence shader global should match default value");
        }

        [Test]
        public void Update_SetsGustDensityShaderGlobal()
        {
            InvokeUpdate();
            var id = Shader.PropertyToID("_GustDensity");
            // Default _gustDensity = 1.0f
            Assert.AreEqual(1.0f, Shader.GetGlobalFloat(id), 0.01f,
                "GustDensity shader global should match default value");
        }

        [Test]
        public void Update_SetsWindDirectionShaderGlobals()
        {
            InvokeUpdate();
            var dirXId = Shader.PropertyToID("_WindDirectionX");
            var dirZId = Shader.PropertyToID("_WindDirectionZ");

            float dirX = Shader.GetGlobalFloat(dirXId);
            float dirZ = Shader.GetGlobalFloat(dirZId);

            // Direction should be a normalized vector
            float magnitude = Mathf.Sqrt(dirX * dirX + dirZ * dirZ);
            Assert.AreEqual(1.0f, magnitude, 0.05f,
                "Wind direction should be approximately normalized");
        }

        [Test]
        public void Update_SetsWindIntensityShaderGlobal()
        {
            InvokeUpdate();
            var id = Shader.PropertyToID("_WindIntensity");
            float intensity = Shader.GetGlobalFloat(id);
            Assert.GreaterOrEqual(intensity, 0f, "Intensity should be non-negative");
            Assert.LessOrEqual(intensity, 1.01f, "Intensity should not exceed 1");
        }

        // ── Behavior ──

        [Test]
        public void Update_AfterSetIntensityZero_MagnitudeDropsTowardZero()
        {
            // First update at default intensity
            InvokeUpdate();
            var magId = Shader.PropertyToID("_WindMagnitude");
            float initialMag = Shader.GetGlobalFloat(magId);

            // Set intensity to 0 and update
            _wind.SetIntensity(0f);
            InvokeUpdate();
            float reducedMag = Shader.GetGlobalFloat(magId);

            Assert.LessOrEqual(reducedMag, initialMag,
                "Wind magnitude should decrease when intensity drops");
        }

        [Test]
        public void Update_MultipleCalls_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 5; i++)
                    InvokeUpdate();
            }, "Multiple Update calls should be safe");
        }

        // ── Helpers ──

        /// <summary>
        /// Invokes the private Update() method via reflection for edit-mode testing.
        /// </summary>
        private void InvokeUpdate()
        {
            var method = typeof(WindSystem).GetMethod("Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "WindSystem should have a private Update method");
            method.Invoke(_wind, null);
        }
    }
}
