using UnityEngine;

namespace InkRidge.Environment
{
    /// <summary>
    /// Builds the bamboo forest trail scene (Scene 1).
    /// Segmented bamboo stalks, layered ground, scattered rocks, ink-painting skybox.
    /// </summary>
    public class BambooSceneBuilder : SceneBuilder
    {
        [Header("Bamboo Scene Config")]
        [SerializeField] private int _bambooCount = 80;
        [SerializeField] private float _trailLength = 60f;
        [SerializeField] private float _trailWidth = 4f;
        [SerializeField] private int _rockCount = 25;

        protected override void Build()
        {
            base.Build();
            SetupSkybox(new Color(0.75f, 0.82f, 0.88f), new Color(0.92f, 0.90f, 0.85f));
            BuildGround();
            BuildTrail();
            BuildBambooForest();
            BuildRocks();
            BuildMeditationPoint();
            BuildBoundaryWalls();
            // Add wind system for bamboo swaying
            var windObj = new GameObject("WindSystem");
            windObj.transform.SetParent(_root.transform);
            windObj.AddComponent<WindSystem>();
            // Add gradient fog for ink-painting atmosphere
            var fogObj = new GameObject("GradientFog");
            fogObj.transform.SetParent(_root.transform);
            fogObj.AddComponent<GradientFog>();
            SetupLighting(GongbiColors.WarmLight, new Vector3(0.5f, -0.6f, 0.4f), 1.2f);
            SetupFog(new Color(0.90f, 0.87f, 0.82f), 0.025f);
        }

        private void BuildGround()
        {
            // Main ground — deep green
            var groundMat = MakeMat(new Color(0.18f, 0.28f, 0.16f), 0.006f);
            Plane("Ground", Vector3.zero, new Vector3(30, 1, 12), groundMat);

            // Path strip — slightly lighter earth tone along trail
            var pathMat = MakeMat(new Color(0.35f, 0.30f, 0.20f), 0.005f);
            Plane("PathStrip", new Vector3(0, 0.01f, _trailLength / 2f),
                new Vector3(1.5f, 1, _trailLength / 10f), pathMat);

            // Moss patches near trail edges
            var mossMat = MakeMat(new Color(0.22f, 0.38f, 0.18f), 0.004f);
            var rng = new System.Random(7);
            for (int i = 0; i < 15; i++)
            {
                float z = rng.Next(0, (int)_trailLength);
                float x = (_trailWidth / 2f + 1f) * (rng.Next(2) == 0 ? -1 : 1);
                Plane($"Moss_{i}", new Vector3(x, 0.02f, z),
                    new Vector3(0.8f + (float)rng.NextDouble() * 0.5f, 1,
                        0.8f + (float)rng.NextDouble() * 0.5f), mossMat);
            }
        }

        private void BuildTrail()
        {
            var stoneMat = MakeMat(GongbiColors.Bluestone, 0.005f);
            var darkStoneMat = MakeMat(new Color(0.35f, 0.38f, 0.42f), 0.005f);
            var rng = new System.Random(42);

            for (int i = 0; i < 20; i++)
            {
                float z = i * (_trailLength / 20f);
                float x = Mathf.Sin(i * 0.7f) * 0.5f;
                var mat = rng.Next(3) == 0 ? darkStoneMat : stoneMat;
                float w = 1.0f + (float)rng.NextDouble() * 0.4f;
                Cube($"Stone_{i}", new Vector3(x, 0.05f, z),
                    new Vector3(w, 0.1f, 0.7f), mat);
            }
        }

        private void BuildBambooForest()
        {
            var bambooMat = MakeMat(GongbiColors.BambooGreen, 0.008f);
            var darkBambooMat = MakeMat(new Color(0.30f, 0.50f, 0.25f), 0.008f);
            var leafMat = MakeMat(GongbiColors.EmeraldGreen, 0.004f);
            var darkLeafMat = MakeMat(new Color(0.12f, 0.38f, 0.16f), 0.004f);
            var rng = new System.Random(42);

            for (int i = 0; i < _bambooCount; i++)
            {
                float x = rng.Next(-14, 14) + (float)rng.NextDouble() * 0.8f;
                float z = rng.Next(0, (int)_trailLength) + (float)rng.NextDouble() * 0.5f;
                if (Mathf.Abs(x) < _trailWidth / 2f + 1.5f) continue;

                float height = 5f + (float)rng.NextDouble() * 4f;
                float radius = 0.12f + (float)rng.NextDouble() * 0.08f;
                var mat = rng.Next(3) == 0 ? darkBambooMat : bambooMat;
                var leafM = rng.Next(2) == 0 ? leafMat : darkLeafMat;

                // Segmented bamboo stalk (3 segments for visible joints)
                int segments = 3;
                float segHeight = height / segments;
                for (int s = 0; s < segments; s++)
                {
                    float segY = segHeight * (s + 0.5f);
                    Cylinder($"Bamboo_{i}_Seg{s}", new Vector3(x, segY, z),
                        new Vector3(radius * 2f, segHeight * 0.95f, radius * 2f), mat);
                    // Joint ring (slightly thicker, darker)
                    Cylinder($"Joint_{i}_{s}", new Vector3(x, segHeight * (s + 1f), z),
                        new Vector3(radius * 2.3f, 0.08f, radius * 2.3f), darkBambooMat);
                }

                // Leaf clusters at top (3 overlapping spheres for fuller look)
                float leafY = height + 0.3f;
                Sphere($"Leaves_{i}_A", new Vector3(x, leafY, z),
                    new Vector3(1.2f, 0.8f, 1.2f), leafM);
                Sphere($"Leaves_{i}_B", new Vector3(x + 0.4f, leafY + 0.3f, z + 0.3f),
                    new Vector3(0.9f, 0.6f, 0.9f), leafM);
                Sphere($"Leaves_{i}_C", new Vector3(x - 0.3f, leafY - 0.2f, z - 0.4f),
                    new Vector3(0.8f, 0.5f, 0.8f), leafM);

                // Small ground bush at bamboo base
                if (rng.Next(3) == 0)
                {
                    Sphere($"Bush_{i}", new Vector3(x, 0.2f, z),
                        new Vector3(0.6f, 0.3f, 0.6f), darkLeafMat);
                }
            }
        }

        private void BuildRocks()
        {
            var rockMat = MakeMat(GongbiColors.GrayStone, 0.01f);
            var darkRockMat = MakeMat(GongbiColors.DarkEarth, 0.01f);
            var rng = new System.Random(99);

            for (int i = 0; i < _rockCount; i++)
            {
                float x = rng.Next(-13, 13);
                float z = rng.Next(0, (int)_trailLength);
                if (Mathf.Abs(x) < _trailWidth / 2f + 1f) continue;

                float s = 0.3f + (float)rng.NextDouble() * 0.6f;
                var mat = rng.Next(2) == 0 ? rockMat : darkRockMat;
                var rock = Sphere($"Rock_{i}", new Vector3(x, s * 0.3f, z),
                    new Vector3(s, s * 0.7f, s), mat);
                rock.transform.rotation = Random.rotation;
            }
        }

        private void BuildMeditationPoint()
        {
            var platformMat = MakeMat(GongbiColors.OchreWall, 0.01f);
            var stoneMat = MakeMat(GongbiColors.Bluestone, 0.008f);

            // Meditation platform — raised stone
            Cube("MeditationPlatform", new Vector3(0, 0.1f, _trailLength),
                new Vector3(3.5f, 0.2f, 3.5f), platformMat);

            // Step leading up
            Cube("Step", new Vector3(0, 0.05f, _trailLength - 1f),
                new Vector3(2.5f, 0.1f, 0.8f), stoneMat);

            // Stone stele
            var stele = Cube("StoneStele", new Vector3(0, 1.3f, _trailLength + 1.2f),
                new Vector3(0.35f, 2.4f, 0.12f), stoneMat);

            // Small stone lanterns flanking the platform
            var lanternMat = MakeMat(GongbiColors.GrayStone, 0.008f);
            for (int side = -1; side <= 1; side += 2)
            {
                Cube($"LanternBase_{side}", new Vector3(side * 1.8f, 0.3f, _trailLength + 0.5f),
                    new Vector3(0.3f, 0.4f, 0.3f), lanternMat);
                Cylinder($"LanternTop_{side}", new Vector3(side * 1.8f, 0.6f, _trailLength + 0.5f),
                    new Vector3(0.25f, 0.2f, 0.25f), lanternMat);
            }
        }

        private void BuildBoundaryWalls()
        {
            // Invisible colliders to prevent walking off the trail
            var wallMat = MakeMat(new Color(0.2f, 0.22f, 0.2f), 0.003f);
            for (int side = -1; side <= 1; side += 2)
            {
                var wall = Cube($"Boundary_{side}", new Vector3(side * 8f, 1f, _trailLength / 2f),
                    new Vector3(0.5f, 2f, _trailLength), wallMat);
                wall.GetComponent<Renderer>().enabled = false;
                var col = wall.GetComponent<BoxCollider>();
                if (col == null) wall.AddComponent<BoxCollider>();
            }
            // Back wall at trail start
            var backWall = Cube("Boundary_Back", new Vector3(0, 1f, -1f),
                new Vector3(20f, 2f, 0.5f), wallMat);
            backWall.GetComponent<Renderer>().enabled = false;
        }
    }
}
