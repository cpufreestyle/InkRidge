using UnityEngine;

namespace InkRidge.Environment
{
    public class WaterfallSceneBuilder : SceneBuilder
    {
        [Header("Waterfall Config")]
        [SerializeField] private float _cliffHeight = 22f;
        [SerializeField] private float _cliffWidth = 18f;
        [SerializeField] private int _rockCount = 20;

        protected override void Build()
        {
            base.Build();
            SetupSkybox(new Color(0.70f, 0.78f, 0.85f), new Color(0.88f, 0.90f, 0.92f));
            BuildCliff();
            BuildWaterfall();
            BuildClimbHandholds();
            BuildPoolArea();
            BuildScatteredRocks();
            BuildMeditationPlatform();
            BuildBoundaryWalls();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.3f, -0.7f, 0.3f), 1.1f);
            SetupFog(new Color(0.82f, 0.87f, 0.92f), 0.03f);
        }

        private void BuildCliff()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.012f);
            var darkRockMat = MakeMat(GongbiColors.DarkEarth, 0.012f);
            var mossRockMat = MakeMat(new Color(0.30f, 0.38f, 0.28f), 0.01f);
            var rng = new System.Random(33);

            // Main cliff face — wider with angled outcrops
            Cube("CliffMain", new Vector3(0, _cliffHeight / 2f, 0),
                new Vector3(_cliffWidth, _cliffHeight, 2.5f), rockMat);

            // Layered rock outcrops with moss
            for (int i = 0; i < 12; i++)
            {
                float y = 1.5f + i * 1.8f;
                float x = (float)(rng.NextDouble() * 2 - 1) * (_cliffWidth / 2f - 1);
                float w = 1.5f + (float)rng.NextDouble() * 1.5f;
                float d = 0.8f + (float)rng.NextDouble() * 0.8f;
                var mat = rng.Next(3) == 0 ? mossRockMat : (rng.Next(2) == 0 ? darkRockMat : rockMat);
                Cube($"Outcrop_{i}", new Vector3(x, y, -0.8f),
                    new Vector3(w, 1.2f, d), mat);
            }

            // Cliff top edge — rough stones
            for (int i = 0; i < 8; i++)
            {
                float x = -_cliffWidth / 2f + i * (_cliffWidth / 7f) + (float)rng.NextDouble() * 0.5f;
                Cube($"CliffTop_{i}", new Vector3(x, _cliffHeight, 0.5f),
                    new Vector3(1.2f, 0.6f, 1f), rockMat);
            }
        }

        private void BuildWaterfall()
        {
            var waterMat = MakeMat(new Color(0.80f, 0.90f, 0.95f), 0.002f);
            // Main waterfall — thinner, taller
            Cube("Waterfall", new Vector3(0, _cliffHeight / 2f, 1.1f),
                new Vector3(_cliffWidth * 0.45f, _cliffHeight, 0.08f), waterMat);

            // Side streams
            var streamMat = MakeMat(new Color(0.75f, 0.88f, 0.92f), 0.002f);
            Cube("Stream_L", new Vector3(-_cliffWidth * 0.3f, _cliffHeight * 0.4f, 1.05f),
                new Vector3(0.08f, _cliffHeight * 0.8f, 0.05f), streamMat);
            Cube("Stream_R", new Vector3(_cliffWidth * 0.3f, _cliffHeight * 0.35f, 1.05f),
                new Vector3(0.08f, _cliffHeight * 0.7f, 0.05f), streamMat);

            // Mist at base (semi-transparent spheres)
            var mistMat = MakeMat(new Color(0.92f, 0.95f, 0.98f, 0.4f), 0.001f);
            var rng = new System.Random(55);
            for (int i = 0; i < 8; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 3f;
                Sphere($"Mist_{i}", new Vector3(x, 0.5f + (float)rng.NextDouble(), 2f),
                    new Vector3(1.5f, 0.8f, 1.5f), mistMat);
            }
        }

        private void BuildPoolArea()
        {
            var poolMat = MakeMat(new Color(0.50f, 0.68f, 0.80f), 0.004f);
            // Pool with raised stone edge
            Cube("Pool", new Vector3(0, 0.08f, 4f),
                new Vector3(7f, 0.15f, 5f), poolMat);

            var edgeMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            // Pool edge stones
            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * Mathf.PI;
                float x = Mathf.Sin(angle) * 4f;
                float z = 4f + Mathf.Cos(angle) * 3f;
                Cube($"PoolEdge_{i}", new Vector3(x, 0.2f, z),
                    new Vector3(0.6f, 0.4f, 0.6f), edgeMat);
            }
        }

        private void BuildClimbHandholds()
        {
            var handholdMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            var mossMat = MakeMat(new Color(0.28f, 0.42f, 0.22f), 0.008f);
            var rng = new System.Random(77);

            for (int i = 0; i < 12; i++)
            {
                float y = 1.2f + i * 1.6f;
                float x = (i % 2 == 0 ? -1.8f : 1.8f) + (float)(rng.NextDouble() * 2 - 1) * 0.6f;
                var mat = rng.Next(4) == 0 ? mossMat : handholdMat;
                var hold = Cube($"Handhold_{i}", new Vector3(x, y, 1.3f),
                    new Vector3(0.35f, 0.25f, 0.35f), mat);
                if (hold.GetComponent<BoxCollider>() == null)
                    hold.AddComponent<BoxCollider>();
            }
        }

        private void BuildScatteredRocks()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.01f);
            var darkRockMat = MakeMat(GongbiColors.DarkEarth, 0.01f);
            var rng = new System.Random(88);

            for (int i = 0; i < _rockCount; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 12f;
                float z = 2f + (float)rng.NextDouble() * 10f;
                if (Mathf.Abs(x) < 2f && z < 8f) continue;
                float s = 0.3f + (float)rng.NextDouble() * 0.7f;
                var mat = rng.Next(2) == 0 ? rockMat : darkRockMat;
                var rock = Sphere($"SRock_{i}", new Vector3(x, s * 0.3f, z),
                    new Vector3(s, s * 0.65f, s), mat);
                rock.transform.rotation = Random.rotation;
            }
        }

        private void BuildMeditationPlatform()
        {
            var platformMat = MakeMat(GongbiColors.Bluestone, 0.01f);
            var accentMat = MakeMat(GongbiColors.OchreWall, 0.008f);

            // Raised platform at cliff top
            Cube("MeditationPlatform", new Vector3(0, _cliffHeight + 0.1f, 2.5f),
                new Vector3(5f, 0.25f, 5f), platformMat);

            // Step
            Cube("TopStep", new Vector3(0, _cliffHeight + 0.05f, 1.5f),
                new Vector3(3f, 0.1f, 0.8f), accentMat);

            // Stone bench
            Cube("StoneBench", new Vector3(0, _cliffHeight + 0.5f, 4.5f),
                new Vector3(2f, 0.4f, 0.5f), platformMat);
        }

        private void BuildBoundaryWalls()
        {
            var wallMat = MakeMat(new Color(0.2f, 0.22f, 0.2f), 0.003f);
            for (int side = -1; side <= 1; side += 2)
            {
                var wall = Cube($"Boundary_{side}", new Vector3(side * 9f, 1f, 5f),
                    new Vector3(0.5f, 2f, 15f), wallMat);
                wall.GetComponent<Renderer>().enabled = false;
            }
            var backWall = Cube("Boundary_Back", new Vector3(0, 1f, -1f),
                new Vector3(25f, 2f, 0.5f), wallMat);
            backWall.GetComponent<Renderer>().enabled = false;
        }
    }
}
