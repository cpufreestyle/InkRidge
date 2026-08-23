using UnityEngine;

namespace InkRidge.Environment
{
    public class WaterfallSceneBuilder : SceneBuilder
    {
        [Header("Waterfall Config")]
        [SerializeField] private float _cliffHeight = 20f;
        [SerializeField] private float _cliffWidth = 15f;

        protected override void Build()
        {
            base.Build();
            BuildCliff();
            BuildWaterfall();
            BuildClimbHandholds();
            BuildMeditationPlatform();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.3f, -0.8f, 0.2f));
            SetupFog(new Color(0.85f, 0.88f, 0.90f), 0.025f);
        }

        private void BuildCliff()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.015f);
            var darkRockMat = MakeMat(GongbiColors.DarkEarth, 0.015f);

            Cube("CliffMain", new Vector3(0, _cliffHeight / 2f, 0),
                new Vector3(_cliffWidth, _cliffHeight, 2f), rockMat);

            for (int i = 0; i < 8; i++)
            {
                float y = 2f + i * 2.5f;
                float x = Random.Range(-_cliffWidth / 2f + 1, _cliffWidth / 2f - 1);
                Cube($"Outcrop_{i}", new Vector3(x, y, -0.5f),
                    new Vector3(2f, 1.5f, 1.5f), darkRockMat);
            }
        }

        private void BuildWaterfall()
        {
            var waterMat = MakeMat(new Color(0.85f, 0.92f, 0.95f), 0.003f);
            Cube("Waterfall", new Vector3(0, _cliffHeight / 2f, 1f),
                new Vector3(_cliffWidth * 0.6f, _cliffHeight, 0.1f), waterMat);

            var poolMat = MakeMat(new Color(0.6f, 0.75f, 0.85f), 0.005f);
            Cube("Pool", new Vector3(0, 0.1f, 3f),
                new Vector3(6f, 0.2f, 4f), poolMat);
        }

        private void BuildClimbHandholds()
        {
            var handholdMat = MakeMat(GongbiColors.Bluestone, 0.01f);
            for (int i = 0; i < 10; i++)
            {
                float y = 1f + i * 1.8f;
                float x = (i % 2 == 0 ? -1.5f : 1.5f) + Random.Range(-0.5f, 0.5f);
                var hold = Cube($"Handhold_{i}", new Vector3(x, y, 1.2f),
                    new Vector3(0.3f, 0.3f, 0.3f), handholdMat);
                hold.AddComponent<BoxCollider>();
            }
        }

        private void BuildMeditationPlatform()
        {
            var mat = MakeMat(GongbiColors.Bluestone, 0.01f);
            Cube("MeditationPlatform", new Vector3(0, _cliffHeight + 0.1f, 2f),
                new Vector3(4f, 0.2f, 4f), mat);
        }
    }
}
