using NUnit.Framework;
using UnityEngine;
using InkRidge.Environment;

namespace InkRidge.Tests
{
    /// <summary>
    /// Exposes protected SceneBuilder members for contract testing.
    /// </summary>
    internal class TestableSceneBuilder : SceneBuilder
    {
        public void TestBuild() => Build();

        public Material TestMakeMat(Color color, float outlineWidth = 0.012f) =>
            MakeMat(color, outlineWidth);

        public Material TestMakeWindMat(Color color, float sway = 0.5f, float outlineWidth = 0.01f) =>
            MakeWindMat(color, sway, outlineWidth);

        public GameObject TestCube(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null) =>
            Cube(name, pos, scale, mat, parent);

        public GameObject TestCylinder(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null) =>
            Cylinder(name, pos, scale, mat, parent);

        public GameObject TestSphere(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null) =>
            Sphere(name, pos, scale, mat, parent);

        public GameObject TestPlane(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null) =>
            Plane(name, pos, scale, mat, parent);

        public void TestSetupFog(Color color, float density) => SetupFog(color, density);

        public void TestSetupLighting(Color color, Vector3 dir, float intensity = 1.1f) =>
            SetupLighting(color, dir, intensity);

        public GameObject GetRoot() => _root;
    }

    [TestFixture]
    public class SceneBuilderContractTests
    {
        private GameObject _holder;
        private TestableSceneBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("TestHolder");
            _builder = _holder.AddComponent<TestableSceneBuilder>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_builder != null && _builder.GetRoot() != null)
                Object.DestroyImmediate(_builder.GetRoot());
            if (_holder != null)
                Object.DestroyImmediate(_holder);
        }

        // ── Build() contract ──

        [Test]
        public void Build_CreatesRootObjectWithSceneName()
        {
            _builder.TestBuild();
            var root = _builder.GetRoot();

            Assert.IsNotNull(root, "Build() should create a root GameObject");
            Assert.That(root.name, Does.StartWith("__Scene_"));
        }

        [Test]
        public void Build_CreatesNewRootEachCall()
        {
            _builder.TestBuild();
            var first = _builder.GetRoot();
            _builder.TestBuild();
            var second = _builder.GetRoot();

            Assert.AreNotSame(first, second, "Each Build() call should create a new root");
            Object.DestroyImmediate(first);
        }

        // ── MakeMat contract ──

        [Test]
        public void MakeMat_ReturnsNonNullMaterial()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.red);
            Assert.IsNotNull(mat);
        }

        [Test]
        public void MakeMat_SetsMainColor()
        {
            _builder.TestBuild();
            var color = new Color(0.5f, 0.3f, 0.1f);
            var mat = _builder.TestMakeMat(color);
            Assert.AreEqual(color, mat.GetColor("_MainColor"));
        }

        [Test]
        public void MakeMat_SetsOutlineWidth()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.white, 0.02f);
            Assert.AreEqual(0.02f, mat.GetFloat("_OutlineWidth"), 0.001f);
        }

        [Test]
        public void MakeMat_SetsWindSwayToZero()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.white);
            Assert.AreEqual(0f, mat.GetFloat("_WindSway"), 0.001f,
                "Standard material should not sway in wind");
        }

        [Test]
        public void MakeMat_SetsOutlineColor()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.white);
            Assert.AreEqual(GongbiColors.InkOutline, mat.GetColor("_OutlineColor"));
        }

        // ── MakeWindMat contract ──

        [Test]
        public void MakeWindMat_SetsWindSway()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeWindMat(Color.green, 0.8f);
            Assert.AreEqual(0.8f, mat.GetFloat("_WindSway"), 0.001f);
        }

        [Test]
        public void MakeWindMat_StillSetsMainColor()
        {
            _builder.TestBuild();
            var color = new Color(0.2f, 0.6f, 0.1f);
            var mat = _builder.TestMakeWindMat(color, 0.5f);
            Assert.AreEqual(color, mat.GetColor("_MainColor"));
        }

        // ── Primitive factory methods ──

        [Test]
        public void Cube_CreatesWithCorrectNamePositionAndScale()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.red);
            var go = _builder.TestCube("TestCube", new Vector3(1, 2, 3), new Vector3(4, 5, 6), mat);

            Assert.AreEqual("TestCube", go.name);
            Assert.AreEqual(new Vector3(1, 2, 3), go.transform.position);
            Assert.AreEqual(new Vector3(4, 5, 6), go.transform.localScale);
            Assert.IsNotNull(go.GetComponent<MeshRenderer>());
            Assert.AreEqual(mat, go.GetComponent<MeshRenderer>().material);
        }

        [Test]
        public void Cube_ParentsToRootByDefault()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.red);
            var go = _builder.TestCube("TestCube", Vector3.zero, Vector3.one, mat);

            Assert.AreEqual(_builder.GetRoot().transform, go.transform.parent);
        }

        [Test]
        public void Cube_ParentsToCustomTransformWhenProvided()
        {
            _builder.TestBuild();
            var customParent = new GameObject("CustomParent").transform;
            var mat = _builder.TestMakeMat(Color.red);
            var go = _builder.TestCube("TestCube", Vector3.zero, Vector3.one, mat, customParent);

            Assert.AreEqual(customParent, go.transform.parent);
            Object.DestroyImmediate(customParent.gameObject);
        }

        [Test]
        public void Cylinder_CreatesWithRendererAndMaterial()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.blue);
            var go = _builder.TestCylinder("TestCyl", new Vector3(0, 5, 0), new Vector3(1, 3, 1), mat);

            Assert.AreEqual("TestCyl", go.name);
            Assert.AreEqual(new Vector3(0, 5, 0), go.transform.position);
            var renderer = go.GetComponent<MeshRenderer>();
            Assert.IsNotNull(renderer);
            Assert.AreEqual(mat, renderer.material);
        }

        [Test]
        public void Sphere_CreatesWithCorrectScale()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.green);
            var scale = new Vector3(2f, 1.5f, 2f);
            var go = _builder.TestSphere("TestSphere", Vector3.zero, scale, mat);

            Assert.AreEqual("TestSphere", go.name);
            Assert.AreEqual(scale, go.transform.localScale);
        }

        [Test]
        public void Plane_CreatesAndParentsToRoot()
        {
            _builder.TestBuild();
            var mat = _builder.TestMakeMat(Color.gray);
            var go = _builder.TestPlane("TestPlane", Vector3.zero, new Vector3(5, 1, 5), mat);

            Assert.AreEqual("TestPlane", go.name);
            Assert.AreEqual(_builder.GetRoot().transform, go.transform.parent);
            Assert.IsNotNull(go.GetComponent<MeshRenderer>());
        }

        // ── SetupFog ──

        [Test]
        public void SetupFog_ConfiguresExponentialFog()
        {
            _builder.TestBuild();
            var fogColor = new Color(0.9f, 0.8f, 0.7f);
            _builder.TestSetupFog(fogColor, 0.03f);

            Assert.IsTrue(RenderSettings.fog);
            Assert.AreEqual(FogMode.Exponential, RenderSettings.fogMode);
            Assert.AreEqual(fogColor, RenderSettings.fogColor);
            Assert.AreEqual(0.03f, RenderSettings.fogDensity, 0.001f);
        }

        // ── SetupLighting ──

        [Test]
        public void SetupLighting_CreatesDirectionalLightUnderRoot()
        {
            _builder.TestBuild();
            _builder.TestSetupLighting(GongbiColors.WarmLight, new Vector3(0, -1, 0), 1.5f);

            var lightObj = _builder.GetRoot().transform.Find("DirectionalLight");
            Assert.IsNotNull(lightObj, "SetupLighting should create a 'DirectionalLight' child");

            var light = lightObj.GetComponent<Light>();
            Assert.IsNotNull(light);
            Assert.AreEqual(LightType.Directional, light.type);
            Assert.AreEqual(1.5f, light.intensity, 0.01f);
            Assert.AreEqual(LightShadows.Soft, light.shadows);
        }
    }
}
