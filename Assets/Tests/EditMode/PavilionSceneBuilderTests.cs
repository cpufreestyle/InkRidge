using NUnit.Framework;
using UnityEngine;
using InkRidge.Environment;

namespace InkRidge.Tests
{
    /// <summary>
    /// Exposes Build() for PavilionSceneBuilder integration testing.
    /// </summary>
    internal class TestablePavilionBuilder : PavilionSceneBuilder
    {
        public void TestBuild() => Build();
        public GameObject GetRoot() => _root;
    }

    [TestFixture]
    public class PavilionSceneBuilderTests
    {
        private GameObject _holder;
        private TestablePavilionBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("TestHolder");
            _builder = _holder.AddComponent<TestablePavilionBuilder>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_builder != null && _builder.GetRoot() != null)
                Object.DestroyImmediate(_builder.GetRoot());
            if (_holder != null)
                Object.DestroyImmediate(_holder);
        }

        private void BuildScene() => _builder.TestBuild();
        private Transform Root => _builder.GetRoot().transform;

        // ── Root ──

        [Test]
        public void Build_CreatesRootObject()
        {
            BuildScene();
            Assert.IsNotNull(_builder.GetRoot(), "Build should create a root GameObject");
            Assert.That(_builder.GetRoot().name, Does.StartWith("__Scene_"));
        }

        // ── Ground ──

        [Test]
        public void Build_CreatesGroundAtOrigin()
        {
            BuildScene();
            var ground = Root.Find("Ground");
            Assert.IsNotNull(ground, "Ground plane should exist");
            Assert.AreEqual(Vector3.zero, ground.position);
        }

        [Test]
        public void Build_CreatesFallenLeaves()
        {
            BuildScene();
            Assert.IsNotNull(Root.Find("Leaf_0"), "At least one fallen leaf should exist");
            Assert.IsNotNull(Root.Find("Leaf_19"), "Last fallen leaf should exist");
        }

        // ── Pavilion structure ──

        [Test]
        public void Build_CreatesFourPillars()
        {
            BuildScene();
            for (int i = 0; i < 4; i++)
                Assert.IsNotNull(Root.Find($"Pillar_{i}"), $"Pillar_{i} should exist");
        }

        [Test]
        public void Build_CreatesPillarBasesAndCaps()
        {
            BuildScene();
            for (int i = 0; i < 4; i++)
            {
                Assert.IsNotNull(Root.Find($"PillarBase_{i}"), $"PillarBase_{i} should exist");
                Assert.IsNotNull(Root.Find($"PillarCap_{i}"), $"PillarCap_{i} should exist");
            }
        }

        [Test]
        public void Build_CreatesPavilionBaseTiers()
        {
            BuildScene();
            Assert.IsNotNull(Root.Find("PavilionBase"), "First base tier should exist");
            Assert.IsNotNull(Root.Find("PavilionBase2"), "Second base tier should exist");
        }

        [Test]
        public void Build_CreatesRoofLayers()
        {
            BuildScene();
            Assert.IsNotNull(Root.Find("RoofEave"), "Roof eave layer should exist");
            Assert.IsNotNull(Root.Find("RoofMain"), "Main roof body should exist");
            Assert.IsNotNull(Root.Find("RoofTop"), "Roof top layer should exist");
        }

        [Test]
        public void Build_CreatesRoofOrnaments()
        {
            BuildScene();
            Assert.IsNotNull(Root.Find("RoofOrnament"), "Roof ornament sphere should exist");
            Assert.IsNotNull(Root.Find("RoofSpire"), "Roof spire should exist");
        }

        [Test]
        public void Build_CreatesCrossBeamsAndBrackets()
        {
            BuildScene();
            for (int i = 0; i < 4; i++)
            {
                Assert.IsNotNull(Root.Find($"Beam_{i}"), $"Beam_{i} should exist");
                Assert.IsNotNull(Root.Find($"Bracket_{i}"), $"Bracket_{i} should exist");
            }
        }

        [Test]
        public void Build_CreatesUpturnedEaveCorners()
        {
            BuildScene();
            for (int i = 0; i < 4; i++)
                Assert.IsNotNull(Root.Find($"EaveCorner_{i}"), $"EaveCorner_{i} should exist");
        }

        [Test]
        public void Build_CreatesSteps()
        {
            BuildScene();
            Assert.IsNotNull(Root.Find("Step1"), "Step 1 should exist");
            Assert.IsNotNull(Root.Find("Step2"), "Step 2 should exist");
            Assert.IsNotNull(Root.Find("Step3"), "Step 3 should exist");
        }

        [Test]
        public void Build_CreatesPavilionFloor()
        {
            BuildScene();
            var floor = Root.Find("PavilionFloor");
            Assert.IsNotNull(floor, "PavilionFloor should exist");
            Assert.AreEqual(new Vector3(0, 0.25f, 0), floor.position);
        }

        // ── Surrounding trees ──

        [Test]
        public void Build_CreatesExpectedTreeCount()
        {
            BuildScene();
            // Default _treeCount = 20, each tree = trunk + 2 leaf spheres
            for (int i = 0; i < 20; i++)
                Assert.IsNotNull(Root.Find($"TreeTrunk_{i}"), $"TreeTrunk_{i} should exist");
            for (int i = 0; i < 20; i++)
                Assert.IsNotNull(Root.Find($"TreeLeaves_{i}"), $"TreeLeaves_{i} should exist");
        }

        // ── Bamboo background ──

        [Test]
        public void Build_CreatesExpectedBambooCount()
        {
            BuildScene();
            // Default _bambooCount = 25
            for (int i = 0; i < 25; i++)
                Assert.IsNotNull(Root.Find($"Bamboo_{i}"), $"Bamboo_{i} should exist");
        }

        // ── Stone path ──

        [Test]
        public void Build_CreatesStonePath()
        {
            BuildScene();
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(Root.Find($"PathStone_{i}"), $"PathStone_{i} should exist");
        }

        // ── Lanterns ──

        [Test]
        public void Build_CreatesLanternsOnBothSides()
        {
            BuildScene();
            foreach (int side in new[] { -1, 1 })
                for (int i = 0; i < 2; i++)
                {
                    Assert.IsNotNull(Root.Find($"LanternBase_{side}_{i}"),
                        $"LanternBase_{side}_{i} should exist");
                    Assert.IsNotNull(Root.Find($"LanternGlow_{side}_{i}"),
                        $"LanternGlow_{side}_{i} should exist");
                    Assert.IsNotNull(Root.Find($"LanternRoof_{side}_{i}"),
                        $"LanternRoof_{side}_{i} should exist");
                }
        }

        // ── Meditation point ──

        [Test]
        public void Build_CreatesMeditationCushion()
        {
            BuildScene();
            var cushion = Root.Find("MeditationCushion");
            Assert.IsNotNull(cushion, "MeditationCushion should exist");
            Assert.AreEqual(new Vector3(0, 0.3f, 0), cushion.position);
        }

        [Test]
        public void Build_CreatesIncenseBurner()
        {
            BuildScene();
            Assert.IsNotNull(Root.Find("IncenseBase"), "IncenseBase should exist");
            Assert.IsNotNull(Root.Find("IncenseBody"), "IncenseBody should exist");
        }

        // ── Boundary walls ──

        [Test]
        public void Build_CreatesBoundaryWalls()
        {
            BuildScene();
            foreach (int side in new[] { -1, 1 })
                for (int dir = 0; dir < 2; dir++)
                    Assert.IsNotNull(Root.Find($"BW_{side}_{dir}"),
                        $"BW_{side}_{dir} should exist");
        }

        [Test]
        public void Build_BoundaryWallRenderersAreDisabled()
        {
            BuildScene();
            foreach (int side in new[] { -1, 1 })
                for (int dir = 0; dir < 2; dir++)
                {
                    var wall = Root.Find($"BW_{side}_{dir}");
                    Assert.IsNotNull(wall);
                    Assert.IsFalse(wall.GetComponent<Renderer>().enabled,
                        $"Boundary wall BW_{side}_{dir} renderer should be disabled");
                }
        }

        // ── Components ──

        [Test]
        public void Build_AttachesGradientFogComponent()
        {
            BuildScene();
            var fogObj = Root.Find("GradientFog");
            Assert.IsNotNull(fogObj, "GradientFog child should exist");
            Assert.IsNotNull(fogObj.GetComponent<GradientFog>(),
                "GradientFog component should be attached");
        }

        [Test]
        public void Build_AttachesWindSystemComponent()
        {
            BuildScene();
            var windObj = Root.Find("WindSystem");
            Assert.IsNotNull(windObj, "WindSystem child should exist");
            Assert.IsNotNull(windObj.GetComponent<WindSystem>(),
                "WindSystem component should be attached");
        }

        // ── Lighting ──

        [Test]
        public void Build_CreatesDirectionalLight()
        {
            BuildScene();
            var lightObj = Root.Find("DirectionalLight");
            Assert.IsNotNull(lightObj, "DirectionalLight should exist");
            var light = lightObj.GetComponent<Light>();
            Assert.IsNotNull(light);
            Assert.AreEqual(LightType.Directional, light.type);
        }

        // ── Overall output ──

        [Test]
        public void Build_ProducesSubstantialHierarchy()
        {
            BuildScene();
            Assert.Greater(Root.childCount, 100,
                "Pavilion scene should produce a rich object hierarchy");
        }
    }
}
