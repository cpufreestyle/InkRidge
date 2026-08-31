using UnityEngine;

namespace InkRidge.Environment
{
    public class WaterfallSceneBuilder : SceneBuilder
    {
        [Header("Waterfall Config")]
        [SerializeField] private float _cliffHeight = 22f;
        [SerializeField] private float _cliffWidth = 18f;
        [SerializeField] private int _rockCount = 25;
        [SerializeField] private int _bambooCount = 30;

        protected override void Build()
        {
            base.Build();
            SetupSkybox(new Color(0.70f, 0.78f, 0.85f), new Color(0.88f, 0.90f, 0.92f));
            BuildGround();
            BuildEntryPath();
            BuildCliff();
            BuildWaterfall();
            BuildClimbHandholds();
            BuildPoolArea();
            BuildScatteredRocks();
            BuildVegetation();
            BuildMeditationPlatform();
            BuildBoundaryWalls();
            var fogObj = new GameObject("GradientFog");
            fogObj.transform.SetParent(_root.transform);
            fogObj.AddComponent<GradientFog>();
            var windObj = new GameObject("WindSystem");
            windObj.transform.SetParent(_root.transform);
            windObj.AddComponent<WindSystem>();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.3f, -0.7f, 0.3f), 1.1f);
            SetupFog(new Color(0.82f, 0.87f, 0.92f), 0.028f);
        }

        private void BuildGround()
        {
            var groundMat = MakeMat(new Color(0.25f, 0.28f, 0.22f), 0.006f);
            Plane("Ground", Vector3.zero, new Vector3(25, 1, 15), groundMat);

            // Wet stone path near pool
            var wetMat = MakeMat(new Color(0.30f, 0.35f, 0.38f), 0.004f);
            Plane("WetGround", new Vector3(0, 0.01f, 4f), new Vector3(5, 1, 6), wetMat);
        }

        private void BuildEntryPath()
        {
            var stoneMat = MakeMat(GongbiColors.Bluestone, 0.005f);
            var rng = new System.Random(12);
            for (int i = 0; i < 8; i++)
            {
                float z = -2f + i * 1.5f;
                float x = (float)(rng.NextDouble() * 2 - 1) * 0.5f;
                Cube($"Path_{i}", new Vector3(x, 0.05f, z),
                    new Vector3(1.0f + (float)rng.NextDouble() * 0.3f, 0.1f, 0.7f), stoneMat);
            }
        }

        private void BuildCliff()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.012f);
            var darkRockMat = MakeMat(GongbiColors.DarkEarth, 0.012f);
            var mossRockMat = MakeMat(new Color(0.30f, 0.38f, 0.28f), 0.01f);
            var rng = new System.Random(33);

            Cube("CliffMain", new Vector3(0, _cliffHeight / 2f, 0),
                new Vector3(_cliffWidth, _cliffHeight, 2.5f), rockMat);

            for (int i = 0; i < 15; i++)
            {
                float y = 1.5f + i * 1.5f;
                float x = (float)(rng.NextDouble() * 2 - 1) * (_cliffWidth / 2f - 1);
                float w = 1.5f + (float)rng.NextDouble() * 1.5f;
                float d = 0.8f + (float)rng.NextDouble() * 0.8f;
                var mat = rng.Next(3) == 0 ? mossRockMat : (rng.Next(2) == 0 ? darkRockMat : rockMat);
                Cube($"Outcrop_{i}", new Vector3(x, y, -0.8f),
                    new Vector3(w, 1.0f, d), mat);
            }

            for (int i = 0; i < 10; i++)
            {
                float x = -_cliffWidth / 2f + i * (_cliffWidth / 9f) + (float)rng.NextDouble() * 0.5f;
                Cube($"CliffTop_{i}", new Vector3(x, _cliffHeight, 0.5f),
                    new Vector3(1.2f, 0.6f, 1f), rockMat);
            }

            // Cliff side walls (to prevent seeing through)
            Cube("CliffWallL", new Vector3(-_cliffWidth / 2f - 1f, _cliffHeight / 2f, 0),
                new Vector3(2f, _cliffHeight, 3f), darkRockMat);
            Cube("CliffWallR", new Vector3(_cliffWidth / 2f + 1f, _cliffHeight / 2f, 0),
                new Vector3(2f, _cliffHeight, 3f), darkRockMat);
        }

        private void BuildWaterfall()
        {
            var waterMat = MakeMat(new Color(0.80f, 0.90f, 0.95f), 0.002f);
            Cube("Waterfall", new Vector3(0, _cliffHeight / 2f, 1.1f),
                new Vector3(_cliffWidth * 0.45f, _cliffHeight, 0.08f), waterMat);

            var streamMat = MakeMat(new Color(0.75f, 0.88f, 0.92f), 0.002f);
            Cube("Stream_L", new Vector3(-_cliffWidth * 0.3f, _cliffHeight * 0.4f, 1.05f),
                new Vector3(0.08f, _cliffHeight * 0.8f, 0.05f), streamMat);
            Cube("Stream_R", new Vector3(_cliffWidth * 0.3f, _cliffHeight * 0.35f, 1.05f),
                new Vector3(0.08f, _cliffHeight * 0.7f, 0.05f), streamMat);

            // Water cascading over rocks at base
            var splashMat = MakeMat(new Color(0.85f, 0.92f, 0.95f), 0.001f);
            var rng = new System.Random(55);
            for (int i = 0; i < 10; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 4f;
                Sphere($"Splash_{i}", new Vector3(x, 0.3f + (float)rng.NextDouble() * 0.5f, 1.8f),
                    new Vector3(0.6f + (float)rng.NextDouble() * 0.4f, 0.4f, 0.6f), splashMat);
            }

            // Mist at base — atmosphere only. These used to be solid 1.5 m
            // colliders sitting right across the approach to the pool.
            var mistMat = MakeMat(new Color(0.92f, 0.95f, 0.98f, 0.4f), 0.001f);
            for (int i = 0; i < 10; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 3f;
                DecorSphere($"Mist_{i}", new Vector3(x, 0.5f + (float)rng.NextDouble(), 2f),
                    new Vector3(1.5f, 0.8f, 1.5f), mistMat);
            }
        }

        private void BuildPoolArea()
        {
            var poolMat = MakeMat(new Color(0.40f, 0.58f, 0.70f), 0.004f);
            Cube("Pool", new Vector3(0, 0.08f, 4f),
                new Vector3(7f, 0.15f, 5f), poolMat);

            var edgeMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            for (int i = 0; i < 14; i++)
            {
                float angle = i / 14f * Mathf.PI;
                float x = Mathf.Sin(angle) * 4f;
                float z = 4f + Mathf.Cos(angle) * 3f;
                Cube($"PoolEdge_{i}", new Vector3(x, 0.2f, z),
                    new Vector3(0.6f, 0.4f, 0.6f), edgeMat);
            }

            // Lotus leaves on pool
            var lotusMat = MakeMat(new Color(0.15f, 0.40f, 0.20f), 0.003f);
            var rng = new System.Random(66);
            for (int i = 0; i < 5; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 2.5f;
                float z = 4f + (float)(rng.NextDouble() * 2 - 1) * 2f;
                DecorSphere($"Lotus_{i}", new Vector3(x, 0.18f, z),
                    new Vector3(0.6f, 0.05f, 0.6f), lotusMat);
            }
        }

        private void BuildClimbHandholds()
        {
            var handholdMat = MakeMat(GongbiColors.Bluestone, 0.008f);
            var mossMat = MakeMat(new Color(0.28f, 0.42f, 0.22f), 0.008f);
            var rng = new System.Random(77);

            for (int i = 0; i < 14; i++)
            {
                float y = 1.2f + i * 1.5f;
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
            var mossRockMat = MakeMat(new Color(0.28f, 0.38f, 0.22f), 0.01f);
            var rng = new System.Random(88);

            for (int i = 0; i < _rockCount; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 12f;
                float z = 2f + (float)rng.NextDouble() * 10f;
                if (Mathf.Abs(x) < 2f && z < 8f) continue;
                float s = 0.3f + (float)rng.NextDouble() * 0.7f;
                var mat = rng.Next(3) == 0 ? mossRockMat : (rng.Next(2) == 0 ? rockMat : darkRockMat);
                var rock = Sphere($"SRock_{i}", new Vector3(x, s * 0.3f, z),
                    new Vector3(s, s * 0.65f, s), mat);
                rock.transform.rotation = Random.rotation;
            }
        }

        private void BuildVegetation()
        {
            var bambooMat = MakeWindMat(GongbiColors.BambooGreen, 0.5f, 0.008f);
            var leafMat = MakeWindMat(GongbiColors.EmeraldGreen, 0.7f, 0.004f);
            var darkLeafMat = MakeWindMat(new Color(0.12f, 0.38f, 0.16f), 0.6f, 0.004f);
            var rng = new System.Random(99);

            for (int i = 0; i < _bambooCount; i++)
            {
                float x = (float)(rng.NextDouble() * 2 - 1) * 11f;
                float z = 6f + (float)rng.NextDouble() * 8f;
                if (Mathf.Abs(x) < 3f && z < 12f) continue;

                float h = 4f + (float)rng.NextDouble() * 2f;
                Cylinder($"Bamboo_{i}", new Vector3(x, h / 2f, z),
                    new Vector3(0.2f, h, 0.2f), bambooMat);

                var lm = rng.Next(2) == 0 ? leafMat : darkLeafMat;
                DecorSphere($"Leaves_{i}", new Vector3(x, h + 0.3f, z),
                    new Vector3(1.0f, 0.7f, 1.0f), lm);
            }
        }

        private void BuildMeditationPlatform()
        {
            var platformMat = MakeMat(GongbiColors.Bluestone, 0.01f);
            var accentMat = MakeMat(GongbiColors.OchreWall, 0.008f);

            Cube("MeditationPlatform", new Vector3(0, _cliffHeight + 0.1f, 2.5f),
                new Vector3(5f, 0.25f, 5f), platformMat);
            Cube("TopStep", new Vector3(0, _cliffHeight + 0.05f, 1.5f),
                new Vector3(3f, 0.1f, 0.8f), accentMat);
            Cube("StoneBench", new Vector3(0, _cliffHeight + 0.5f, 4.5f),
                new Vector3(2f, 0.4f, 0.5f), platformMat);

            // Stone lanterns
            var lanternMat = MakeMat(GongbiColors.GrayStone, 0.008f);
            for (int side = -1; side <= 1; side += 2)
            {
                Cube($"Lantern_{side}", new Vector3(side * 2.2f, _cliffHeight + 0.3f, 0.5f),
                    new Vector3(0.3f, 0.4f, 0.3f), lanternMat);
                Cylinder($"LanternTop_{side}", new Vector3(side * 2.2f, _cliffHeight + 0.6f, 0.5f),
                    new Vector3(0.25f, 0.2f, 0.25f), lanternMat);
            }
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
            var backWall = Cube("Boundary_Back", new Vector3(0, 1f, -2f),
                new Vector3(25f, 2f, 0.5f), wallMat);
            backWall.GetComponent<Renderer>().enabled = false;
        }
    }
}
