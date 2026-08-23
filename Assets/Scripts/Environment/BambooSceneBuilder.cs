using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Builds the bamboo forest trail scene (Scene 1).
    /// </summary>
    public class BambooSceneBuilder : SceneBuilder
    {
        [Header("Bamboo Scene Config")]
        [SerializeField] private int _bambooCount = 40;
        [SerializeField] private float _trailLength = 60f;
        [SerializeField] private float _trailWidth = 4f;

        protected override void Build()
        {
            base.Build();
            BuildGround();
            BuildTrail();
            BuildBambooForest();
            BuildMeditationPoint();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.5f, -0.7f, 0.3f));
            SetupFog(new Color(0.88f, 0.85f, 0.80f), 0.02f);
        }

        private void BuildGround()
        {
            var groundMat = MakeMat(GongbiColors.DeepGreen, 0.008f);
            Plane("Ground", new Vector3(0, 0, 0), new Vector3(20, 1, 10), groundMat);
        }

        private void BuildTrail()
        {
            var stoneMat = MakeMat(GongbiColors.Bluestone, 0.005f);
            for (int i = 0; i < 20; i++)
            {
                float z = i * (_trailLength / 20f);
                float x = Mathf.Sin(i * 0.7f) * 0.5f;
                Cube($"Stone_{i}", new Vector3(x, 0.05f, z),
                    new Vector3(1.2f, 0.1f, 0.8f), stoneMat);
            }
        }

        private void BuildBambooForest()
        {
            var bambooMat = MakeMat(GongbiColors.BambooGreen, 0.01f);
            var leafMat = MakeMat(GongbiColors.EmeraldGreen, 0.005f);
            var rng = new System.Random(42);

            for (int i = 0; i < _bambooCount; i++)
            {
                float x = rng.Next(-15, 15);
                float z = rng.Next(0, (int)_trailLength);
                if (Mathf.Abs(x) < _trailWidth / 2f + 1f) continue;

                float height = 4f + (float)rng.NextDouble() * 3f;
                float radius = 0.15f + (float)rng.NextDouble() * 0.1f;

                Cylinder($"Bamboo_{i}", new Vector3(x, height / 2f, z),
                    new Vector3(radius * 2f, height, radius * 2f), bambooMat);

                Sphere($"Leaves_{i}", new Vector3(x, height + 0.5f, z),
                    new Vector3(1.5f, 1.5f, 1.5f), leafMat);
            }
        }

        private void BuildMeditationPoint()
        {
            var mat = MakeMat(GongbiColors.OchreWall, 0.01f);
            Cube("MeditationPlatform", new Vector3(0, 0.1f, _trailLength),
                new Vector3(3f, 0.2f, 3f), mat);

            Cube("StoneStele", new Vector3(0, 1.2f, _trailLength + 1f),
                new Vector3(0.4f, 2.4f, 0.15f), mat);
        }
    }
}
